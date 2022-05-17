using System;
using System.Collections.Concurrent;
using System.IO;
using Witlesss.Also;

namespace Witlesss.Commands
{
    public class Fuse : WitlessCommand
    {
        public override void Run()
        {
            string[] a = Text.Split();
            if (a.Length > 1)
            {
                string name = a[1];
                bool passedID = long.TryParse(name, out long key);
                bool thisChatID = key == Chat;
                if (thisChatID)
                {
                    Bot.SendMessage(Chat, Strings.FUSE_FAIL_SELF);
                    return;
                }

                bool chatExist = passedID && Bot.WitlessExist(key);
                bool baseExist = Bot.BaseExists(name);
                if (chatExist || baseExist)
                {
                    Baka.Backup();
                    var fusion = new FusionCollab(Baka.Words, chatExist ? Bot.SussyBakas[key].Words : FromFile());
                    fusion.Fuse();
                    Baka.HasUnsavedStuff = true;
                    Baka.Save();
                    Bot.SendMessage(Chat, $"{Strings.FUSE_SUCCESS_RESPONSE_A} \"{Title}\" {Strings.FUSE_SUCCESS_RESPONSE_B}\n{BASE_NEW_SIZE()}");
                }
                else Bot.SendMessage(Chat, passedID ? Strings.FUSE_FAIL_CHAT : Strings.FUSE_FAIL_BASE + FUSE_AVAILABLE_BASES());

                ConcurrentDictionary<string, ConcurrentDictionary<string, int>> FromFile() => new FileIO<ConcurrentDictionary<string, ConcurrentDictionary<string, int>>>($@"{Environment.CurrentDirectory}\{Strings.EXTRA_DBS_FOLDER}\{name}.json").LoadData();
            }
            else Bot.SendMessage(Chat, Strings.FUSE_MANUAL);

            string BASE_NEW_SIZE() => $"Теперь он весит {Extension.FileSize(Baka.Path)}";
            string BASE_SIZE() => $"Словарь <b>этой беседы</b> весит {Extension.FileSize(Baka.Path)}";

            string FUSE_AVAILABLE_BASES()
            {
                FileInfo[] files = new DirectoryInfo($@"{Environment.CurrentDirectory}\{Strings.EXTRA_DBS_FOLDER}").GetFiles();
                var result = "\n\nДоступные словари:";
                foreach (var file in files)
                    result = result + $"\n<b>{file.Name.Replace(".json", "")}</b> ({Extension.FileSize(file.FullName)})";
                result = result + "\n\n" + BASE_SIZE();
                return result;
            }
        }
    }
}