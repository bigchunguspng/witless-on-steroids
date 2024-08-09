using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Settings;
using Witlesss.Generation.Pack;

namespace Witlesss.Commands.Packing
{
    //      /fuse
    //      /fuse <-[file]
    //      /fuse [id  / name / info]
    //      /fuse [*/!] [name / info]

    //      * - history
    //      ! - private packs

    public class Fuse : SettingsCommand
    {
        protected long Size;
        protected int Limit = int.MaxValue;

        private bool _private, _history;
        private Document? _document;

        protected override void RunAuthorized()
        {
            Baka.SaveChanges();
            Size = SizeInBytes(Baka.FilePath);

            GetWordsPerLineLimit();

            var args = Args.SplitN(2);
            _private = args.Length > 1 && args[0] == "!";
            _history = args.Length > 1 && args[0] == "*";

            if      (Message.ProvidesFile("text/plain",       out _document)) ProcessTxtFile();
            else if (Message.ProvidesFile("application/json", out _document)) ProcessJsonFile();
            else if (args.Length > 0 && args[^1] == "info")
            {
                if      (_history)         SendHistoric(new ListPagination(Chat));
                else if (_private)         SendFuseList(new ListPagination(Chat), @private: true);
                else if (args.Length == 1) SendFuseList(new ListPagination(Chat));
            }
            else if (_history)
            {
                var name = string.Join(' ', args.Skip(1));
                var files = GetFiles(GetHistoryFolder(), $"{name}.json");
                if (files.Length == 0)
                {
                    SendHistoric(new ListPagination(Chat), fail: true);
                }
                else
                {
                    if (name == "*")
                        foreach (var file in files)
                            EatFromJsonFile(file);
                    else    EatFromJsonFile(files[0]);
                    GoodEnding();
                }
            }
            else if (_private || args.Length == 1) FuseWitlessDB(args[^1]);
            else Bot.SendMessage(Chat, FUSE_MANUAL, preview: false);
        }

        private void ProcessTxtFile()
        {
            var path = UniquePath(Paths.Dir_History, _document!.FileName ?? "fuse.txt");
            Bot.DownloadFile(_document.FileId, path, Chat).Wait();

            EatFromTxtFile(path);
            GoodEnding();
        }

        private void ProcessJsonFile()
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

        #region LISTING

        private record FusionListData(string Available, string Object, string Key, string Optional);

        private readonly FusionListData ExtraDBp = new("📂 Публичные словари", "словаря", "fi", "");
        private readonly FusionListData ExtraDBs = new("🔐 Приватные словари", "словаря", "f!", "");
        private readonly FusionListData Historic = new("🔐 Архив файлов", "файла", "f*", FUSE_HIS_ALL);

        public void HandleCallback(CallbackQuery query, string[] data)
        {
            var pagination = query.GetPagination(data);

            if (data[0] == "fi") SendFuseList(pagination);
            if (data[0] == "f!") SendFuseList(pagination, @private: true);
            else                 SendHistoric(pagination);
        }

        private void SendFuseList(ListPagination pagination, bool fail = false, bool @private = false)
        {
            var fuseList  = @private ? ExtraDBs : ExtraDBp;
            var directory = GetFuseFolder(@private);
            SendFilesList(fuseList, directory, pagination, fail);
        }

        private void SendHistoric(ListPagination pagination, bool fail = false)
        {
            var directory = GetHistoryFolder();
            SendFilesList(Historic, directory, pagination, fail);
        }

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
            sb.Append("\n\n").AppendJoin('\n', JsonList(files, page, perPage));
            sb.Append("\n\nСловарь <b>этой беседы</b> ");
            var path = Baka.FilePath;
            if (File.Exists(path))
                sb.Append("весит ").Append(FileSize(path));
            else
                sb.Append("пуст");
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

        protected static IEnumerable<string> JsonList(FileInfo[] files, int page = 0, int perPage = 25)
        {
            if (files.Length == 0)
            {
                yield return "*пусто*";
                yield break;
            }

            foreach (var file in files.Skip(page * perPage).Take(perPage))
            {
                var name = file.Name.Replace(".json", "");
                var size = FileSize(file.FullName);
                yield return $"<code>{name}</code> ({size})";
            }
        }

        #endregion


        #region FUSION

        private const string HOLY_MOLY = "CAACAgIAAxkBAAI062a2Yi7myiBzNob7ftdyivXXEdJjAAKYDAACjdb4SeqLf5UfqK3dNQQ";

        private void FuseWitlessDB(string arg)
        {
            var argIsChatId = long.TryParse(arg, out var chat);
            if (chat == Chat)
            {
                Bot.SendSticker(Chat, new InputOnlineFile(HOLY_MOLY));
                return;
            }

            if (argIsChatId && ChatsDealer.WitlessExist(chat, out var baka))
            {
                FuseWithWitlessDB(baka.Baka.DB);
            }
            else if (GetFiles(GetFuseFolder(_private), $"{arg}.json") is { Length: > 0 } files)
            {
                FuseWithWitlessDB(JsonIO.LoadData<GenerationPack>(files[0]));
            }
            else if (argIsChatId) Bot.SendMessage(Chat, FUSE_FAIL_CHAT);
            else SendFuseList(new ListPagination(Chat), fail: true, @private: _private);
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
            JsonIO.SaveData(lines.ToList(), save);
        }

        protected void EatFromJsonFile(string path)
        {
            var lines = JsonIO.LoadData<List<string>>(path);
            EatAllLines(lines);
        }

        private          void EatAllLines(IEnumerable<string> lines) => EatAllLines(lines, Baka, Limit, out _);
        protected static void EatAllLines(IEnumerable<string> lines, Witless baka, int limit, out int eaten)
        {
            eaten = 0;
            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (line.Count(c => c == ' ' || c == '\n') >= limit) continue;
                if (baka.Eat(line)) eaten++;
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
        private string GetPrivateFolder() => Path.Combine(Paths.Dir_Fuse,    Chat.ToString());

        private string GetFuseFolder(bool @private) => @private ? GetPrivateFolder() : Paths.Dir_Fuse;

        private string GetJsonFormatExample()
        {
            var sb = new StringBuilder(ONLY_ARRAY_JSON);
            var count = Random.Shared.Next(3, 7);
            sb.Append("\n\n<pre>[");
            for (var i = 0; i < count; i++)
            {
                sb.Append("\n    \"").Append(Baka.Generate()).Append("\"");
                if (i < count - 1) sb.Append(",");
            }
            sb.Append("\n]</pre>");
            return sb.ToString();
        }
    }

    public record ListPagination(long Chat, int MessageId = -1, int Page = 0, int PerPage = 25);
}