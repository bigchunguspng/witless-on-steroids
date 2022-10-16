using System;
using System.IO;
using static System.Environment;
using static Witlesss.Extension;
using static Witlesss.Strings;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class Move : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;
            
            var a = Text.Split();
            if (a.Length > 1)
            {
                string name = a[1];
                string result = MoveDictionary(name);

                if (result == "*")
                {
                    Bot.SendMessage(Chat, "Сударь, ваш словарь пуст 🫥");
                }
                else
                {
                    Baka.Words.Clear();
                    Log($"{Title} >> DIC CLEARED!", ConsoleColor.Magenta);
                    Baka.SaveNoMatterWhat();

                    Bot.SendMessage(Chat, string.Format(MOVING_DONE, result));
                }
            }
            else Bot.SendMessage(Chat, MOVE_MANUAL);
        }

        protected string MoveDictionary(string name)
        {
            var bytes = new FileInfo(Baka.Path).Length;
            if (bytes > 2)
            {
                string path = UniquePath(ExtraDBPath(name), ".json", name == "info" || name == "his");
                Baka.Save();
                File.Copy(Baka.Path, path);
                
                string result = path.Substring(path.LastIndexOf('\\') + 1).Replace(".json", "");
                Log($@"{Title} >> DIC SAVED AS ""{result}""", ConsoleColor.Magenta);
                return result;
            }
            else return "*"; // can't be in file name
        }
        private string ExtraDBPath(string name) => $@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}\{name}.json";
    }
}