using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Telegram.Bot.Types;
using static System.Environment;
using static Witlesss.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, float>>;

namespace Witlesss.Commands
{
    public class Fuse : WitlessCommand
    {
        public override void Run()
        {
            var a = Text.Split();
            if (a.Length > 2)
            {
                string name = Text.Substring(Text.IndexOf(' ') + 1);
                var path = $@"{CurrentDirectory}\{CH_HISTORY_FOLDER}\{Chat}";
                Directory.CreateDirectory(path);
                var files = Directory.GetFiles(path);
                
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
                {
                    bool passedID = long.TryParse(name, out long key);
                    if (key == Chat)
                    {
                        Bot.SendMessage(Chat, FUSE_FAIL_SELF);
                        return;
                    }

                    bool chatExist = passedID && Bot.WitlessExist(key);
                    bool baseExist = Bot.BaseExists(name);
                    if (chatExist || baseExist)
                    {
                        Baka.Backup();
                        new FusionCollab(Baka.Words, chatExist ? Bot.SussyBakas[key].Words : FromFile()).Fuse();
                        GoodEnding();
                    }
                    else Bot.SendMessage(Chat, passedID ? FUSE_FAIL_CHAT : FUSE_FAIL_BASE + FUSE_AVAILABLE_BASES());
                }

                WitlessDB FromFile() => new FileIO<WitlessDB>($@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}\{name}.json").LoadData();
            }
            else if (CanFuseHistory(out string fileID))
            {
                var directory = $@"{CurrentDirectory}\{CH_HISTORY_FOLDER}";
                var path = UniquePath($@"{directory}\{CH_HISTORY_FILE_PREFIX}-{Chat}.json", ".json");
                Directory.CreateDirectory(directory);
                Bot.DownloadFile(fileID, path, Chat).Wait();
                
                EatChatHistory(path);
                GoodEnding();
            }
            else Bot.SendMessage(Chat, FUSE_MANUAL);

            string BASE_SIZE() => $"Словарь <b>этой беседы</b> весит {FileSize(Baka.Path)}";

            string FUSE_AVAILABLE_BASES()
            {
                var files = new DirectoryInfo($@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}").GetFiles();
                var result = "\n\nДоступные словари:\n";
                foreach (var file in files)
                    result = result + $"\n<b>{file.Name.Replace(".json", "")}</b> ({FileSize(file.FullName)})";
                result = result + "\n\n" + BASE_SIZE();
                return result;
            }
            string FUSE_AVAILABLE_DATES()
            {
                var path = $@"{CurrentDirectory}\{CH_HISTORY_FOLDER}\{Chat}";
                Directory.CreateDirectory(path);
                var files = new DirectoryInfo(path).GetFiles();
                var result = "Доступные диапазоны переписки:\n";
                if (files.Length == 0)
                    result = result + "\n\n*пусто*";
                else foreach (var file in files)
                    result = result + $"\n<b>{file.Name.Replace(".json", "")}</b> ({FileSize(file.FullName)})";
                if (files.Length > 0)
                    result = result + "\n\nМожно скормить всё сразу прописав\n\n<code>/fuse@piece_fap_bot his all</code>";

                return result;
            }
            
            bool IsJsonAttached(Message message) => message.Document?.MimeType != null && message.Document?.MimeType == "application/json";

            bool CanFuseHistory(out string fileID)
            {
                fileID = "";
                if (IsJsonAttached(Message))
                    fileID = Message.Document?.FileId;
                else if (Message.ReplyToMessage != null && IsJsonAttached(Message.ReplyToMessage))
                    fileID = Message.ReplyToMessage.Document?.FileId;
                return fileID!.Length > 0;
            }
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

            path = $@"{path.Remove(path.LastIndexOf('\\'))}\{Chat}";
            Directory.CreateDirectory(path);
            path = $@"{path}\{date1} - {date2}.json";
            new FileIO<List<string>>(path).SaveData(save);
            
            string FormatDate(object o) => o.ToString()?.Replace(':', '.').Replace('-', '.').Replace('T', ' ');
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