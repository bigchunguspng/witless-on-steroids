using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace Witlesss.Commands
{
    public class Fuse : SettingsCommand
    {
        private long _size;
        private int   _max;
        
        private FileInfo[] _files;

        protected override void ExecuteAuthorized()
        {
            Baka.Save();
            _size = SizeInBytes(Baka.Path);

            var a = Text.Split();
            if (a.Length > 2)
            {
                string name = Regex.Match(Text, @"(his\sall)|(((\d{2,4}-+){2}\d+)(\s-\s)*){2}").Value;
                var path = $@"{CH_HISTORY_FOLDER}\{Chat}";
                var files = GetFiles(path);

                GetMaxWordsPerLine();
                
                if (name == "his all")
                {
                    foreach (string file in files) EatHistorySimple(file);
                    GoodEnding();
                }
                else
                {
                    var file = $@"{path}\{name}.json";
                    if (files.Contains(file))
                    {
                        EatHistorySimple(file);
                        GoodEnding();
                    }
                    else
                        Bot.SendMessage(Chat, FUSE_FAIL_DATES);
                }
            }
            else if (a.Length > 1)
            {
                string name = a[1];
                
                if      (name == "info") SendFuseList(Chat, 0, 25);
                else if (name == "his" ) Bot.SendMessage(Chat, FUSE_AVAILABLE_DATES());
                else
                    FuseWitlessDB(name);
            }
            else if (CanFuseHistory(out string fileID))
            {
                string path = UniquePath($@"{CH_HISTORY_FOLDER}\{CH_HISTORY_FILE_PREFIX}-{Chat}.json");
                Bot.DownloadFile(fileID, path, Chat).Wait();

                EatChatHistory(path);
                GoodEnding();
            }
            else Bot.SendMessage(Chat, FUSE_MANUAL, preview: false);

            bool CanFuseHistory(out string fileID)
            {
                fileID = "";
                if (IsJsonAttached(Message))
                    fileID = Message.Document?.FileId;
                else if (Message.ReplyToMessage != null && IsJsonAttached(Message.ReplyToMessage))
                    fileID = Message.ReplyToMessage.Document?.FileId;
                return fileID!.Length > 0;
            }
            bool IsJsonAttached(Message message) => message.Document?.MimeType is "application/json";
        }

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

        private void FuseWitlessDB(string name)
        {
            bool passedID = long.TryParse(name, out long key);
            if (key == Chat)
            {
                Bot.SendMessage(Chat, FUSE_FAIL_SELF);
                return;
            }

            bool chatExist = passedID && Bot.WitlessExist(key);
            bool baseExist = BaseExists();
            if (chatExist || baseExist)
            {
                new FusionCollab(Baka, chatExist ? Bot.SussyBakas[key].Words : FromFile()).Fuse();
                GoodEnding();
            }
            else
            {
                if (passedID) Bot.SendMessage(Chat, FUSE_FAIL_CHAT);
                else SendFuseList(Chat, 0, 25, messageId: -1, fail: true);
            }

            string Path() => $@"{EXTRA_DBS_FOLDER}\{name}.json";
            WitlessDB FromFile() => new FileIO<WitlessDB>(Path()).LoadData();
            bool BaseExists() => GetFiles(EXTRA_DBS_FOLDER).Contains(Path());
        }

        private static void EatChatHistory(string path)
        {
            var io = new FileIO<ExpandoObject>(path);

            var data = io.LoadData();
            var list = (IList) data.First(x => x.Key == "messages").Value;
            
            var save = new List<string>(list.Count);
            foreach (var message in list)
            {
                var mess = (IDictionary<string, object>) message;
                if (mess["type"].ToString() == "service")           continue;
                if (mess["from_id"].ToString() == "user1980917094") continue;

                var text = mess["text"].ToString();
                if (string.IsNullOrEmpty(text))                     continue;
                if (text.StartsWith("System.Collections.Generic"))  continue;

                Baka.Eat(text);
                save.Add(text);
            }

            string date1 = FormatDate(((IDictionary<string, object>) list[0] )?["date"]);
            string date2 = FormatDate(((IDictionary<string, object>) list[^1])?["date"]);

            path = $@"{CH_HISTORY_FOLDER}\{Chat}";
            Directory.CreateDirectory(path);
            path = $@"{path}\{date1} - {date2}.json";
            new FileIO<List<string>>(path).SaveData(save);

            string FormatDate(object o) => ((DateTime) o).ToString("yyyy'-'MM'-'dd");
        }
        
        private void EatHistorySimple(string path)
        {
            var list = new FileIO<List<string>>(path).LoadData();
            foreach (string text in list)
            {
                if (text.Count(c => c == ' ') >= _max) continue;
                Baka.Eat(text);
            }
        }
        private void GetMaxWordsPerLine()
        {
            bool valuePassed = int.TryParse(Text[(Text.LastIndexOf(' ') + 1)..], out _max);
            if (!valuePassed) _max = int.MaxValue;
        }
        
        private void GoodEnding()
        {
            Baka.SaveNoMatterWhat();
            Log($"{Title} >> {LOG_FUSION_DONE}", ConsoleColor.Magenta);
            var newSize = SizeInBytes(Baka.Path);
            var difference = FileSize(newSize - _size);
            Bot.SendMessage(Chat, string.Format(FUSE_SUCCESS_RESPONSE, Title, FileSize(newSize), difference));
        }
    }
}