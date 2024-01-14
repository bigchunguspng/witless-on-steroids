using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace Witlesss.Commands
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
        private long _size;
        private int _limit;

        private Document _document;
        
        private FileInfo[] _files;

        protected override void ExecuteAuthorized()
        {
            Baka.Save();

            _size = SizeInBytes(Baka.Path);

            GetWordsPerLineLimit();

            var s = Text.Split();
            if (FileAttached("text/plain")) // TXT
            {
                var path = UniquePath($@"{CH_HISTORY_FOLDER}\{_document.FileName}");
                Bot.DownloadFile(_document.FileId, path, Chat).Wait();

                EatFromTxtFile(path);
                GoodEnding();
            }
            else if (FileAttached("application/json")) // JSON  /  ERROR >> JSON HIS GUIDE
            {
                var path = UniquePath($@"{CH_HISTORY_FOLDER}\{Chat}\{_document.FileName}");
                Bot.DownloadFile(_document.FileId, path, Chat).Wait();

                try
                {
                    EatFromJsonFile(path);
                    GoodEnding();
                }
                catch // wrong format
                {
                    // todo send guide
                    Bot.SendMessage(Chat, "get JSONed lmao");
                }
            }
            else if (s.Length > 2 && s[1] == "his") // JSON HIS
            {
                var name = string.Join(' ', s.Skip(2));
                var path = $@"{CH_HISTORY_FOLDER}\{Chat}";
                var files = GetFiles(path, $"{name}.json");

                if (files.Length == 0)
                {
                    Bot.SendMessage(Chat, FUSE_FAIL_DATES); // todo change
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
            else if (s.Length == 2) // DIC
            {
                var arg = s[1];
                
                if      (arg == "info") SendFuseList(Chat, 0, 25);
                else if (arg == "his" ) Bot.SendMessage(Chat, FUSE_AVAILABLE_DATES());
                else
                    FuseWitlessDB(arg);
            }
            else Bot.SendMessage(Chat, FUSE_MANUAL, preview: false); // todo change manual
        }


        private bool FileAttached(string type)
        {
            return HasDocument(Message, type) || HasDocument(Message.ReplyToMessage, type);
        }

        private bool HasDocument(Message message, string type)
        {
            var b = message is not null && message.Document?.MimeType == type;
            if (b) _document = Message.Document;

            return b;
        }

        #region LISTING

        public void SendFuseList(long chat, int page, int perPage, int messageId = -1, bool fail = false)
        {
            var files = GetFilesInfo(EXTRA_DBS_FOLDER);
            if (_files is null || _files.Length != files.Length) _files = files;

            var lastPage = (int)Math.Ceiling(_files.Length / (double)perPage) - 1;
            var sb = new StringBuilder();
            if (fail)
            {
                sb.Append(FUSE_FAIL_BASE).Append("\n\n");
            }
            sb.Append("<b>Доступные словари:</b> ");
            sb.Append("📄[").Append(page + 1).Append("/").Append(lastPage + 1).Append("]\n");
            sb.Append(JsonList(_files, page, perPage));
            sb.Append("\n\nСловарь <b>этой беседы</b> весит ").Append(FileSize(Baka.Path));
            sb.Append(USE_ARROWS);

            var text = sb.ToString();
            var buttons = FuseBoards.GetPaginationKeyboard(page, perPage, lastPage, "fi");

            FuseBoards.SendOrEditMessage(chat, text, messageId, buttons);
        }

        private static string FUSE_AVAILABLE_DATES()
        {
            var files = GetFilesInfo($@"{CH_HISTORY_FOLDER}\{Chat}");
            var result = $"<b>Доступные диапазоны переписки:</b>\n{JsonList(files, 0, 100)}";
            if (files.Length > 0)
                result += "\n\nМожно скормить всё, прописав\n\n<code>/fuse@piece_fap_bot his all</code>";

            return result;
        }

        public static string JsonList(FileInfo[] files, int page = 0, int perPage = 25)
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

            var chatExist = argIsID && Bot.WitlessExist(chat);
            var files = chatExist ? null : GetFiles(EXTRA_DBS_FOLDER, $"{arg}.json");
            var fileExist = files is { Length: > 0 };
            if (chatExist || fileExist)
            {
                var source = chatExist ? Bot.SussyBakas[chat].Words : new FileIO<WitlessDB>(files[0]).LoadData();
                new FusionCollab(Baka, source).Fuse();
                GoodEnding();
            }
            else if (argIsID) Bot.SendMessage(Chat, FUSE_FAIL_CHAT);
            else SendFuseList(Chat, 0, 25, messageId: -1, fail: true);
        }

        private void EatFromJsonFile(string path)
        {
            var lines = new FileIO<List<string>>(path).LoadData();
            EatAllLines(lines);
        }

        private void EatFromTxtFile(string path)
        {
            var lines = File.ReadAllLines(path);
            EatAllLines(lines);

            var directory = $@"{CH_HISTORY_FOLDER}\{Chat}";
            Directory.CreateDirectory(directory);

            var name = Path.GetFileNameWithoutExtension(path);
            var save = UniquePath($@"{directory}\{name}.json");
            new FileIO<List<string>>(save).SaveData(lines.ToList());
        }

        private void EatAllLines(IEnumerable<string> lines)
        {
            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (line.Count(c => c == ' ' || c == '\n') >= _limit) continue;
                Baka.Eat(line);
            }
        }

        private void GetWordsPerLineLimit()
        {
            var match = Regex.Match(Text, @"^\/fuse(\d+)");
            _limit = match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
        }

        private void GoodEnding()
        {
            Log($"{Title} >> {LOG_FUSION_DONE}", ConsoleColor.Magenta);
            Baka.SaveNoMatterWhat();
            var newSize = SizeInBytes(Baka.Path);
            var difference = newSize - _size;
            var message = string.Format(FUSE_SUCCESS_RESPONSE, Title, FileSize(newSize), FileSize(difference));
            Bot.SendMessage(Chat, message);
        }

        #endregion
    }
}