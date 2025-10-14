using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Features_Aux.Packs.Commands
{
    //      /move  [name]    = PACK: ACTIVE  -> PRIVATE
    //      /pub   [name]    = PACK: ACTIVE  -> PUBLIC
    //      /pub ! [name]    = PACK: PRIVATE -> PUBLIC
    //      /pub * [name]    = FILE: PRIVATE -> PUBLIC

    public class Move : CommandHandlerAsync_SettingsBlocking
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
            if      (publishPack) Publish(args[^1], Dir_Fuse   , _ctxFuse);
            else if (publishFile) Publish(args[^1], Dir_History, _ctxHist);
            else
            {
                var name = PackManager.Move(Chat, name: Args ?? Title, _public);
                if (name == null)
                {
                    Status = CommandResultStatus.BAD;
                    Bot.SendMessage(Origin, "Ваш словарь пуст, сударь 🫥");
                }
                else
                {
                    Log($"{Title} >> DIC {(_public ? "PUBLISHED" : "MOVED")} >> {name}", LogLevel.Info, LogColor.Fuchsia);
                    var marker = _public ? "" : "! ";
                    var result = _public ? "опубликовано" : "сохранено";
                    Bot.SendMessage(Origin, MOVING_DONE.Format(EMPTY_EMOJI.PickAny(), result, marker, name));
                }
            }
        }

        private record PublishContext(string What_SentenceCase, string What, string SourceMarker, string TargetMarker);

        private static readonly PublishContext
            _ctxFuse = new("Словарь", "словарь", "! ", ""  ),
            _ctxHist = new("Файл"   , "файл"   , "* ", "@ ");

        private void Publish(string name, FilePath directory, PublishContext ctx)
        {
            var filename = $"{name}{Ext_Pack}";
            var fileSource = directory.Combine(Chat.ToString(), filename);
            if (fileSource.FileExists)
            {
                var fileTarget = directory.Combine(filename).MakeUnique();
                File.Move(fileSource, fileTarget);

                var text = PUB_DONE.Format(ctx.What_SentenceCase, name, ctx.TargetMarker);
                Bot.SendMessage(Origin, text);
            }
            else
            {
                Status = CommandResultStatus.BAD;
                var text = PUB_NOT_FOUND.Format(FAIL_EMOJI.PickAny(), ctx.What, ctx.SourceMarker);
                Bot.SendMessage(Origin, text);
            }
        }
    }

    public enum ExportMode
    {
        Public,
        Private,
    }
}