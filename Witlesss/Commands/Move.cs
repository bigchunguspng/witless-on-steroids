using System;
using System.IO;

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
            if (SizeInBytes(Baka.Path) > 2)
            {
                string path = UniquePath($@"{EXTRA_DBS_FOLDER}\{name}.json", name == "info" || name == "his");
                Baka.Save();
                File.Copy(Baka.Path, path);
                
                string result = Path.GetFileNameWithoutExtension(path);
                Log($@"{Title} >> DIC SAVED AS ""{result}""", ConsoleColor.Magenta);
                return result;
            }
            else return "*"; // can't be in file name
        }
    }
}