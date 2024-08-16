using Witlesss.Commands.Settings;
using Witlesss.Generation.Pack;

namespace Witlesss.Commands.Packing
{
    //      /move [name]    PRIVATE
    //      /pub  [name]    PUBLIC
    //      /_  ! [name]    PRIVATE -> PUBLIC

    public class Move : SettingsCommand
    {
        private bool _public = true;

        public Move WithMode(ExportMode mode)
        {
            _public = mode == ExportMode.Public;
            return this;
        }

        protected override void RunAuthorized()
        {
            var args = Args.SplitN(2);
            var pubX = args.Length > 1 && args[0] == "!";
            if (pubX) // publish a pack from private storage
            {
                var name = args[^1];
                var file = Path.Combine(Dir_Fuse, Chat.ToString(), $"{name}.json");
                if (File.Exists(file) == false)
                {
                    Bot.SendMessage(Chat, string.Format(PUB_EX_NOT_FOUND, FAIL_EMOJI_1.PickAny()));
                    return;
                }

                File.Move(file, UniquePath(Dir_Fuse, $"{name}.json"));
                Bot.SendMessage(Chat, string.Format(PUB_EX_DONE, name));
            }
            else
            {
                var name = (Args ?? Title).Replace(' ', '_').ValidFileName('-');

                var newName = MoveDictionary(name, _public ? 0 : Chat);
                if (newName == "*")
                {
                    Bot.SendMessage(Chat, "Ваш словарь пуст, сударь 🫥");
                }
                else
                {
                    Baka.Baka.DB = new GenerationPack();
                    Log($"{Title} >> DIC CLEARED!", ConsoleColor.Magenta);
                    Baka.Save();

                    var result = _public ? "опубликовано" : "сохранено";
                    Bot.SendMessage(Chat, string.Format(MOVING_DONE, EMPTY_EMOJI.PickAny(), result, newName));
                }
            }
        }

        protected string MoveDictionary(string name, long chat)
        {
            Baka.SaveChanges();

            if (Baka.Baka.DB.Vocabulary.Count == 0)
                return "*"; // can't be in file name

            var path = GetUniqueExtraPackPath(name, chat);

            File.Copy(Baka.FilePath, path);

            var result = Path.GetFileNameWithoutExtension(path);
            Log($@"{Title} >> DIC SAVED AS ""{result}""", ConsoleColor.Magenta);
            return result;
        }

        public static string GetUniqueExtraPackPath(string name, long chat = 0)
        {
            var path = chat == 0 ? Dir_Fuse : Path.Combine(Dir_Fuse, chat.ToString());
            return UniquePath(path, $"{name}.json", name is "info" or "his");
        }
    }

    public enum ExportMode
    {
        Public, Private
    }
}