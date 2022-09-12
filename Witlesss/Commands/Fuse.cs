using System;
using System.IO;
using static Witlesss.Strings;
using static Witlesss.Extension;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, int>>;

namespace Witlesss.Commands
{
    public class Fuse : WitlessCommand
    {
        public override void Run()
        {
            var a = Text.Split();
            if (a.Length > 1)
            {
                string name = a[1];
                bool passedID = long.TryParse(name, out long key);
                bool thisChatID = key == Chat;
                if (thisChatID)
                {
                    Bot.SendMessage(Chat, FUSE_FAIL_SELF);
                    return;
                }

                bool chatExist = passedID && Bot.WitlessExist(key);
                bool baseExist = Bot.BaseExists(name);
                if (chatExist || baseExist)
                {
                    Baka.Backup();
                    var fusion = new FusionCollab(Baka.Words, chatExist ? Bot.SussyBakas[key].Words : FromFile());
                    fusion.Fuse();
                    Baka.SaveNoMatterWhat();
                    Bot.SendMessage(Chat, $"{FUSE_SUCCESS_RESPONSE_A} \"{Title}\" {FUSE_SUCCESS_RESPONSE_B}\n{BASE_NEW_SIZE()}");
                }
                else Bot.SendMessage(Chat, passedID ? FUSE_FAIL_CHAT : FUSE_FAIL_BASE + FUSE_AVAILABLE_BASES());

                WitlessDB FromFile() => new FileIO<WitlessDB>($@"{Environment.CurrentDirectory}\{EXTRA_DBS_FOLDER}\{name}.json").LoadData();
            }
            else Bot.SendMessage(Chat, FUSE_MANUAL);

            string BASE_NEW_SIZE() => $"Теперь он весит {FileSize(Baka.Path)}";
            string BASE_SIZE() => $"Словарь <b>этой беседы</b> весит {FileSize(Baka.Path)}";

            string FUSE_AVAILABLE_BASES()
            {
                var files = new DirectoryInfo($@"{Environment.CurrentDirectory}\{EXTRA_DBS_FOLDER}").GetFiles();
                var result = "\n\nДоступные словари:";
                foreach (var file in files)
                    result = result + $"\n<b>{file.Name.Replace(".json", "")}</b> ({FileSize(file.FullName)})";
                result = result + "\n\n" + BASE_SIZE();
                return result;
            }
        }
    }
}