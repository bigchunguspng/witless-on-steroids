﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Settings;
using Witlesss.Generation.Pack;

namespace Witlesss.Commands.Packing
{
    // FUSE modes:
    
    // DIC      - chat DBs      /fuse [id / name]
    // TXT      - text lines    /fuse [file.txt]
    // JSON     - json array    /fuse [xd.json]
    // JSON HIS - json array    /fuse his [name / *]
    // HIS      - json          /fuse [xd.json] -> ERROR >> GUIDE
    //                          /fuse his       -> LIST
    //                          /fuse info      -> LIST
    
    public class Fuse : SettingsCommand
    {
        protected long Size;
        protected int Limit = int.MaxValue;

        private Document? _document;

        protected override void RunAuthorized()
        {
            Baka.SaveChanges();
            Size = SizeInBytes(Baka.FilePath);

            GetWordsPerLineLimit();

            var args = Args.SplitN();
            if (Message.ProvidesFile("text/plain", out _document)) // TXT
            {
                var path = UniquePath(Paths.Dir_History, _document!.FileName ?? "fuse.txt");
                Bot.DownloadFile(_document.FileId, path, Chat).Wait();

                EatFromTxtFile(path);
                GoodEnding();
            }
            else if (Message.ProvidesFile("application/json", out _document)) // JSON  /  ERROR >> JSON HIS GUIDE
            {
                var path = UniquePath(GetHistoryFolder(), _document!.FileName ?? "fuse.json");
                Bot.DownloadFile(_document.FileId, path, Chat).Wait();

                try
                {
                    EatFromJsonFile(path);
                    GoodEnding();
                }
                catch // wrong format
                {
                    File.Delete(path);
                    Bot.SendMessage(Chat, GetJsonFormatExample());
                }
            }
            else if (args.Length > 1 && args[0] == "his") // JSON HIS
            {
                var name = string.Join(' ', args.Skip(1));
                var files = GetFiles(GetHistoryFolder(), $"{name}.json");

                if (files.Length == 0)
                {
                    SendFusionHistory(new ListPagination(Chat), fail: true);
                }
                else if (name == "*")
                {
                    foreach (var file in files) EatFromJsonFile(file);
                    GoodEnding();
                }
                else
                {
                    EatFromJsonFile(files[0]);
                    GoodEnding();
                }
            }
            else if (args.Length == 1) // DIC
            {
                var arg = args[0];

                if      (arg == "info") SendFuseList     (new ListPagination(Chat));
                else if (arg == "his" ) SendFusionHistory(new ListPagination(Chat));
                else
                    FuseWitlessDB(arg);
            }
            else Bot.SendMessage(Chat, FUSE_MANUAL, preview: false);
        }


        #region LISTING

        public void HandleCallback(CallbackQuery query, string[] data)
        {
            var pagination = query.GetPagination(data);

            if (data[0] == "fi") SendFuseList     (pagination);
            else                 SendFusionHistory(pagination);
        }

        private void SendFuseList(ListPagination pagination, bool fail = false)
        {
            var directory = Paths.Dir_Fuse;
            SendFilesList(ExtraDBs, directory, pagination, fail);
        }

        private void SendFusionHistory(ListPagination pagination, bool fail = false)
        {
            var directory = GetHistoryFolder();
            SendFilesList(Historic, directory, pagination, fail);
        }

        private record FusionListData(string Available, string Object, string Key, string Optional);

        private readonly FusionListData ExtraDBs = new("Доступные словари", "словаря", "fi", "");
        private readonly FusionListData Historic = new("Доступные файлы", "файла", "fh", FUSE_HIS_ALL);

        private void SendFilesList
        (
            FusionListData data, string directory, ListPagination pagination, bool fail = false
        )
        {
            var (chat, messageId, page, perPage) = pagination;

            var files = GetFilesInfo(directory);
            var oneshot = files.Length < perPage;
            var empty = files.Length == 0;

            var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
            var sb = new StringBuilder();
            if (fail)
            {
                sb.Append("К сожалению, я не нашёл ").Append(data.Object).Append(" с таким названием\n\n");
            }
            sb.Append("<b>").Append(data.Available).Append(":</b>");
            if (!oneshot) sb.Append(" 📄[").Append(page + 1).Append('/').Append(lastPage + 1).Append(']');
            sb.Append('\n').Append(JsonList(files, page, perPage));
            sb.Append("\n\nСловарь <b>этой беседы</b> весит ").Append(FileSize(Baka.FilePath));
            if (!empty) sb.Append(data.Optional);
            if (!oneshot) sb.Append(USE_ARROWS);

            var buttons = oneshot ? null : GetPaginationKeyboard(page, perPage, lastPage, data.Key);

            SendOrEditMessage(chat, sb.ToString(), messageId, buttons);
        }

        protected static InlineKeyboardMarkup GetPaginationKeyboard(int page, int perPage, int last, string key)
        {
            var inactive = InlineKeyboardButton.WithCallbackData("💀", "-");
            var buttons = new List<InlineKeyboardButton> { inactive, inactive, inactive, inactive };

            if (page > 1       ) buttons[0] = InlineKeyboardButton.WithCallbackData("⏪", CallbackData(0));
            if (page > 0       ) buttons[1] = InlineKeyboardButton.WithCallbackData("⬅️", CallbackData(page - 1));
            if (page < last    ) buttons[2] = InlineKeyboardButton.WithCallbackData("➡️", CallbackData(page + 1));
            if (page < last - 1) buttons[3] = InlineKeyboardButton.WithCallbackData("⏩", CallbackData(last));

            return new InlineKeyboardMarkup(buttons);
            
            string CallbackData(int p) => $"{key} - {p} {perPage}";
        }

        protected static void SendOrEditMessage(long chat, string text, int messageId, InlineKeyboardMarkup? buttons)
        {
            var b = messageId < 0;
            if (b) Bot.SendMessage(chat, text, buttons);
            else   Bot.EditMessage(chat, messageId, text, buttons);
        }

        protected static string JsonList(FileInfo[] files, int page = 0, int perPage = 25)
        {
            if (files.Length == 0) return "\n*пусто*";
            
            var sb = new StringBuilder();
            foreach (var db in files.Skip(page * perPage).Take(perPage))
            {
                sb.Append("\n<code>").Append(db.Name.Replace(".json", ""));
                sb.Append("</code> (").Append(FileSize(db.FullName)).Append(")");
            }
            return sb.ToString();
        }

        #endregion


        #region FUSION

        private void FuseWitlessDB(string arg)
        {
            var argIsID = long.TryParse(arg, out var chat);
            if (chat == Chat)
            {
                Bot.SendMessage(Chat, FUSE_FAIL_SELF);
                return;
            }

            if (argIsID && ChatsDealer.WitlessExist(chat, out var baka))
            {
                FuseWithWitlessDB(baka.Baka.DB);
            }
            else if (GetFiles(Paths.Dir_Fuse, $"{arg}.json") is { Length: > 0 } files)
            {
                FuseWithWitlessDB(new FileIO<GenerationPack>(files[0]).LoadData());
            }
            else if (argIsID) Bot.SendMessage(Chat, FUSE_FAIL_CHAT);
            else SendFuseList(new ListPagination(Chat), fail: true);
        }

        private void FuseWithWitlessDB(GenerationPack source)
        {
            Baka.Fuse(source);
            GoodEnding();
        }

        private void EatFromTxtFile(string path)
        {
            var lines = File.ReadAllLines(path);
            EatAllLines(lines);

            var directory = GetHistoryFolder();
            Directory.CreateDirectory(directory);

            var name = Path.GetFileNameWithoutExtension(path);
            var save = UniquePath(directory, $"{name}.json");
            new FileIO<List<string>>(save).SaveData(lines.ToList());
        }

        protected void EatFromJsonFile(string path)
        {
            var lines = new FileIO<List<string>>(path).LoadData();
            EatAllLines(lines);
        }

        private          void EatAllLines(IEnumerable<string> lines) => EatAllLines(lines, Baka, Limit, out _);
        protected static void EatAllLines(IEnumerable<string> lines, Witless baka, int limit, out int eated)
        {
            eated = 0;
            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (line.Count(c => c == ' ' || c == '\n') >= limit) continue;
                if (baka.Eat(line)) eated++;
            }
        }

        protected void GetWordsPerLineLimit()
        {
            var match = Regex.Match(Command!, @"^\/\S+(\d+)");
            Limit = match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
        }

        protected void GoodEnding()
        {
            SaveChanges(Baka, Title);
            Bot.SendMessage(Chat, FUSION_SUCCESS_REPORT(Baka, Size, Title));
        }

        protected static void SaveChanges(Witless baka, string title)
        {
            Log($"{title} >> {LOG_FUSION_DONE}", ConsoleColor.Magenta);
            baka.Save();
        }

        protected static string FUSION_SUCCESS_REPORT(Witless baka, long size, string title)
        {
            var newSize = SizeInBytes(baka.FilePath);
            var difference = newSize - size;
            return string.Format(FUSE_SUCCESS_RESPONSE, title, FileSize(newSize), FileSize(difference));
        }

        #endregion


        private string GetHistoryFolder() => Path.Combine(Paths.Dir_History, Chat.ToString());

        private string GetJsonFormatExample()
        {
            var sb = new StringBuilder(ONLY_ARRAY_JSON);
            var count = Random.Shared.Next(3, 7);
            sb.Append("\n\n<code>[");
            for (var i = 0; i < count; i++)
            {
                sb.Append("\n    \"").Append(Baka.Generate()).Append("\"");
                if (i < count - 1) sb.Append(",");
            }
            sb.Append("\n]</code>");
            return sb.ToString();
        }
    }

    public record ListPagination(long Chat, int MessageId = -1, int Page = 0, int PerPage = 25);
}