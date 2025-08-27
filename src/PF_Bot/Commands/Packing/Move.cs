using PF_Bot.Commands.Settings;
using PF_Tools.Copypaster;

namespace PF_Bot.Commands.Packing
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
                    Bot.SendMessage(Origin, "Ваш словарь пуст, сударь 🫥");
                }
                else
                {
                    Baka.Baka = new GenerationPack();
                    Log($"{Title} >> DIC CLEARED!", LogLevel.Info, LogColor.Fuchsia);
                    Baka.Save();

                    var result = _public ? "опубликовано" : "сохранено";
                    Bot.SendMessage(Origin, string.Format(MOVING_DONE, EMPTY_EMOJI.PickAny(), result, newName));
                }
            }
        }

        private void Publish(string name, string directory, string[] x)
        {
            var file = Path.Combine(directory, Chat.ToString(), $"{name}.pack");
            if (File.Exists(file) == false)
            {
                var text = string.Format(PUB_NOT_FOUND, FAIL_EMOJI.PickAny(), x[0], x[1]);
                Bot.SendMessage(Origin, text);
                return;
            }

            File.Move(file, UniquePath(directory, $"{name}.pack"));
            Bot.SendMessage(Origin, string.Format(PUB_DONE, x[2], name, x[3]));
        }

        protected string MoveDictionary(string name, long chat)
        {
            Baka.SaveChanges();

            if (Baka.VocabularySize == 0)
                return "*"; // can't be in file name

            var path = GetUniqueExtraPackPath(name, chat);

            File.Copy(PackPath, path);

            var result = Path.GetFileNameWithoutExtension(path);
            Log($"{Title} >> DIC {(chat is 0 ? "PUBLISHED" : "MOVED")} >> {result}", LogLevel.Info, LogColor.Fuchsia);
            return result;
        }

        public static string GetUniqueExtraPackPath(string name, long chat = 0)
        {
            var path = chat == 0 ? Dir_Fuse : Path.Combine(Dir_Fuse, chat.ToString());
            return UniquePath(path, $"{name}.pack", name is "info" or "his");
        }
    }

    public enum ExportMode
    {
        Public, Private
    }
}