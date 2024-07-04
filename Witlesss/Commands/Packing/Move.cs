using System;
using System.IO;
using Witlesss.Commands.Settings;
using Witlesss.Generation.Pack;

namespace Witlesss.Commands.Packing
{
    public class Move : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            var name = ValidFileName((Args ?? Title).Replace(' ', '-'), '-');

            var result = MoveDictionary(name);
            if (result == "*")
            {
                Bot.SendMessage(Chat, "Сударь, ваш словарь пуст 🫥");
            }
            else
            {
                Baka.Baka.DB = new GenerationPack();
                Log($"{Title} >> DIC CLEARED!", ConsoleColor.Magenta);
                Baka.Save();

                Bot.SendMessage(Chat, string.Format(MOVING_DONE, result));
            }

            // todo explain manual in result report
            // else Bot.SendMessage(Chat, MOVE_MANUAL);
        }

        protected string MoveDictionary(string name)
        {
            Baka.SaveChanges();

            if (Baka.Baka.DB.Vocabulary.Count == 0)
                return "*"; // can't be in file name

            var path = UniqueExtraDBsPath(name);

            File.Copy(Baka.FilePath, path);

            var result = Path.GetFileNameWithoutExtension(path);
            Log($@"{Title} >> DIC SAVED AS ""{result}""", ConsoleColor.Magenta);
            return result;
        }

        public static string UniqueExtraDBsPath(string name)
        {
            return UniquePath(Paths.Dir_Fuse, $"{name}.json", name is "info" or "his");
        }
    }
}