using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Telegram.Bot.Types;
using static Witlesss.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, float>>;

namespace Witlesss.Commands
{
    public class Fuse : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;
            
            var a = Text.Split();
            if (a.Length > 2)
            {
                string name = Text.Substring(Text.IndexOf(' ') + 1);
                var path = $@"{CH_HISTORY_FOLDER}\{Chat}";
                var files = GetFiles(path);
                
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
                
                if      (name == "info")
                    Bot.SendMessage(Chat, FUSE_AVAILABLE_BASES());
                else if (name == "his")
                    Bot.SendMessage(Chat, FUSE_AVAILABLE_DATES());
                else
                    FuseWitlessDB(name);
            }
            else if (CanFuseHistory(out string fileID))
            {
                Directory.CreateDirectory(CH_HISTORY_FOLDER);
                string path = UniquePath($@"{CH_HISTORY_FOLDER}\{CH_HISTORY_FILE_PREFIX}-{Chat}.json", ".json");
                Bot.DownloadFile(fileID, path, Chat).Wait();
                
                EatChatHistory(path);
                GoodEnding();
            }
            else Bot.SendMessage(Chat, FUSE_MANUAL);

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

        private string BASE_SIZE() => $"Словарь <b>этой беседы</b> весит {FileSize(Baka.Path)}";
        private string FUSE_AVAILABLE_BASES() => $"Доступные словари:\n{JsonList(GetFilesInfo(EXTRA_DBS_FOLDER))}\n\n{BASE_SIZE()}";
        private string FUSE_AVAILABLE_DATES()
        {
            var files = GetFilesInfo($@"{CH_HISTORY_FOLDER}\{Chat}");
            var result = $"Доступные диапазоны переписки:\n{JsonList(files)}";
            if (files.Length > 0)
                result = result + "\n\nМожно скормить всё сразу прописав\n\n<code>/fuse@piece_fap_bot his all</code>";

            return result;
        }

        private string JsonList(FileInfo[] files)
        {
            if (files.Length == 0) return "\n*пусто*";
            
            var result = "";
            foreach (var file in files)
            {
                result = result + $"\n<code>{file.Name.Replace(".json", "")}</code> ({FileSize(file.FullName)})";
            }
            return result;
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
            else Bot.SendMessage(Chat, passedID ? FUSE_FAIL_CHAT : $"{FUSE_FAIL_BASE}\n\n{FUSE_AVAILABLE_BASES()}");

            string Path() => $@"{EXTRA_DBS_FOLDER}\{name}.json";
            WitlessDB FromFile() => new FileIO<WitlessDB>(Path()).LoadData();
            bool BaseExists() => GetFiles(EXTRA_DBS_FOLDER).Contains(Path());
        }

        private void EatChatHistory(string path)
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

                Baka.Eat(text, out _);
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
            foreach (string text in list) Baka.Eat(text, out _);
        }
        
        private void GoodEnding()
        {
            Log($"{Title} >> {LOG_FUSION_DONE}", ConsoleColor.Magenta);
            Baka.SaveNoMatterWhat();
            Bot.SendMessage(Chat, string.Format(FUSE_SUCCESS_RESPONSE, Title, FileSize(Baka.Path)));
        }
    }
}