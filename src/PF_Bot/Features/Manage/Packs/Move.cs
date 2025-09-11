using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Chats;
using PF_Bot.Features.Manage.Settings;

namespace PF_Bot.Features.Manage.Packs
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
                    ChatManager.ClearPack(Chat, Baka);
                    Log($"{Title} >> DIC CLEARED!", LogLevel.Info, LogColor.Fuchsia);

                    var result = _public ? "опубликовано" : "сохранено";
                    Bot.SendMessage(Origin, string.Format(MOVING_DONE, EMPTY_EMOJI.PickAny(), result, newName));
                }
            }
        }

        private void Publish(string name, FilePath directory, string[] x)
        {
            var filename = $"{name}{Ext_Pack}";
            var fileSource = directory.Combine(Chat.ToString(), filename);
            if (fileSource.FileExists.Janai())
            {
                var text = string.Format(PUB_NOT_FOUND, FAIL_EMOJI.PickAny(), x[0], x[1]);
                Bot.SendMessage(Origin, text);
                return;
            }

            var fileTarget = directory.Combine(filename).MakeUnique();
            File.Move(fileSource, fileTarget);
            Bot.SendMessage(Origin, string.Format(PUB_DONE, x[2], name, x[3]));
        }

        protected string MoveDictionary(string name, long chat)
        {
            ChatManager.SaveBaka(Chat, Baka);

            if (Baka.VocabularyCount == 0)
                return "*"; // can't be in file name

            var path = GetUniqueExtraPackPath(name, chat);

            File.Copy(PackPath, path);

            var result = path.FileNameWithoutExtension;
            Log($"{Title} >> DIC {(chat is 0 ? "PUBLISHED" : "MOVED")} >> {result}", LogLevel.Info, LogColor.Fuchsia);
            return result;
        }

        public static FilePath GetUniqueExtraPackPath(string name, long chat = 0)
        {
            var directory = chat == 0
                ? Dir_Fuse
                : Dir_Fuse.Combine(chat.ToString());
            var suffix =  name is "info" or "his" // todo update - his is not used, test !@*
                ? Desert.GetSand(2)
                : null;

            return directory
                .EnsureDirectoryExist()
                .Combine($"{name}{suffix}{Ext_Pack}")
                .MakeUnique();
        }
    }

    public enum ExportMode
    {
        Public,
        Private,
    }
}