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

        protected static string MoveDictionary(string name)
        {
            Baka.Save();

            if (SizeInBytes(Baka.Path) > 2)
            {
                string path = UniqueExtraDBsPath(name);

                File.Copy(Baka.Path, path);
                
                string result = Path.GetFileNameWithoutExtension(path);
                Log($@"{Title} >> DIC SAVED AS ""{result}""", ConsoleColor.Magenta);
                return result;
            }
            return "*"; // can't be in file name
        }
        
        public static string UniqueExtraDBsPath(string name)
        {
            return UniquePath($@"{EXTRA_DBS_FOLDER}\{name}.json", name is "info" or "his");
        }
    }
}