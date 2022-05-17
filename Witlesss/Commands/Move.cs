using System;
using System.IO;
using Witlesss.Also;

namespace Witlesss.Commands
{
    public class Move : WitlessCommand
    {
        public override void Run()
        {
            string[] a = Text.Split();
            if (a.Length > 1)
            {
                string name = a[1];
                string path = Bot.BaseExists(name) ? Extension.UniquePath(ExtraDBPath(name), ".json") : ExtraDBPath(name);
                Baka.Save();
                File.Copy(Baka.Path, path);
                        
                string result = path.Substring(path.LastIndexOf('\\') + 1).Replace(".json", "");
                Logger.Log($@"{Title} >> DIC SAVED AS ""{result}""", ConsoleColor.Magenta);

                Baka.Words.Clear();
                Logger.Log($"{Title} >> DIC CLEARED!", ConsoleColor.Magenta);
                Baka.HasUnsavedStuff = true;
                Baka.Save();

                Bot.SendMessage(Chat, $"{Strings.MOVE_DONE_CLEARED}\n\n{Strings.MOVE_DONE_AS} <b>\"{result}\"</b>");
            }
            else
                Bot.SendMessage(Chat, Strings.MOVE_MANUAL);

            string ExtraDBPath(string name) => $@"{Environment.CurrentDirectory}\{Strings.EXTRA_DBS_FOLDER}\{name}.json";
        }
    }
}