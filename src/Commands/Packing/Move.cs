using Witlesss.Commands.Settings;
using Witlesss.Generation.Pack;

namespace Witlesss.Commands.Packing
{
    //      /move  [name]    PRIVATE
    //      /pub   [name]    PUBLIC
    //      /pub ! [name]    PRIVATE -> PUBLIC PACK
    //      /pub * [name]    PRIVATE -> PUBLIC FILE

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
            var publishPack = args.Length > 1 && args[0] == "!";
            var publishFile = args.Length > 1 && args[0] == "*";
            if      (publishPack) Publish(args[^1], Dir_Fuse   , ["словарь", "!", "Словарь",   ""]);
            else if (publishFile) Publish(args[^1], Dir_History, ["файл"   , "*", "Файл"   , "@ "]);
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
                    Log($"{Title} >> DIC CLEARED!", LogLevel.Info, 13);
                    Baka.Save();

                    var result = _public ? "опубликовано" : "сохранено";
                    Bot.SendMessage(Chat, string.Format(MOVING_DONE, EMPTY_EMOJI.PickAny(), result, newName));
                }
            }
        }

        private void Publish(string name, string directory, string[] x)
        {
            var file = Path.Combine(directory, Chat.ToString(), $"{name}.json");
            if (File.Exists(file) == false)
            {
                var text = string.Format(PUB_NOT_FOUND, FAIL_EMOJI_1.PickAny(), x[0], x[1]);
                Bot.SendMessage(Chat, text);
                return;
            }

            File.Move(file, UniquePath(directory, $"{name}.json"));
            Bot.SendMessage(Chat, string.Format(PUB_DONE, x[2], name, x[3]));
        }

        protected string MoveDictionary(string name, long chat)
        {
            Baka.SaveChanges();

            if (Baka.WordCount == 0)
                return "*"; // can't be in file name

            var path = GetUniqueExtraPackPath(name, chat);

            File.Copy(PackPath, path);

            var result = Path.GetFileNameWithoutExtension(path);
            Log($"{Title} >> DIC {(chat is 0 ? "PUBLISHED" : "MOVED")} >> {result}", LogLevel.Info, 13);
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