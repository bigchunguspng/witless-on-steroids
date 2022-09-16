using System;
using System.IO;
using static System.Environment;
using static Witlesss.Strings;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class Move : WitlessCommand
    {
        public override void Run()
        {
            var a = Text.Split();
            if (a.Length > 1)
            {
                string name = a[1];
                string result = MoveDictionary(name);

                Baka.Words.Clear();
                Log($"{Title} >> DIC CLEARED!", ConsoleColor.Magenta);
                Baka.SaveNoMatterWhat();

                Bot.SendMessage(Chat, string.Format(MOVING_DONE, result));
            }
            else
                Bot.SendMessage(Chat, MOVE_MANUAL);
        }

        protected string MoveDictionary(string name)
        {
            string path = Bot.BaseExists(name) ? Extension.UniquePath(ExtraDBPath(name), ".json") : ExtraDBPath(name);
            Baka.Save();
            File.Copy(Baka.Path, path);
                        
            string result = path.Substring(path.LastIndexOf('\\') + 1).Replace(".json", "");
            Log($@"{Title} >> DIC SAVED AS ""{result}""", ConsoleColor.Magenta);
            return result;
        }
        private string ExtraDBPath(string name) => $@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}\{name}.json";
    }
}