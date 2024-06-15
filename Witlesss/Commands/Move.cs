using System;
using System.IO;

namespace Witlesss.Commands
{
    public class Move : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            if (Text is not null)
            {
                var name = Text.Replace(' ', '-');
                var result = MoveDictionary(name);

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
            return UniquePath($@"{Paths.Dir_Fuse}\{name}.json", name is "info" or "his");
        }
    }
}