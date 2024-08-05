using System;
using Telegram.Bot.Types;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Editing;
using Witlesss.Commands.Messaging;
using Witlesss.Commands.Packing;

namespace Witlesss.Commands.Routing
{
    public class CommandRouter : CommandAndCallbackRouter
    {
        private readonly Tell _tell = new();
        private readonly Spam _spam = new();
        private readonly Help _help = new();
        private readonly SendMessage _mail = new();
        private readonly Piece _piece = new();
        private readonly DebugMessage _debug = new();
        private readonly BrowseReddit _reddit = new();
        private readonly GetRedditLink _link = new();

        private readonly WitlessCommandRouter _witlessRouter;

        private readonly CommandRegistry<AnyCommand<CommandContext>> _simpleCommands;

        public CommandRouter()
        {
            _witlessRouter = new WitlessCommandRouter(this);

            _simpleCommands = new CommandRegistry<AnyCommand<CommandContext>>()
                .Register("fast"   , () => new ChangeSpeed().SetMode(SpeedMode.Fast))
                .Register("slow"   , () => new ChangeSpeed().SetMode(SpeedMode.Slow))
                .Register("crop"   , () => new Crop().UseDefaultMode())
                .Register("shake"  , () => new Crop().UseShakeMode())
                .Register("scale"  , () => new Scale())
                .Register("slice"  , () => new Slice())
                .Register("cut"    , () => new Cut())
                .Register("sus"    , () => new Sus())
                .Register("song"   , () => new DownloadMusic())
                .Register("damn"   , () => new RemoveBitrate())
                .Register("reverse", () => new Reverse())
                .Register("eq"     , () => new Equalize())
                .Register("vol"    , () => new ChangeVolume())
                .Register("g"      , () => new ToAnimation())
                .Register("sex"    , () => new ToSticker())
                .Register("note"   , () => new ToVideoNote())
                .Register("vova"   , () => new ToVoiceMessage())
                .Register("peg"    , () => new AdvancedEdit())
                .Register("w"      , () => _reddit)
                .Register("link"   , () => _link)
                .Register("piece"  , () => _piece)
                .Register("debug"  , () => _debug)
                .Register("chat_id", () => _mail.WithText($"<code>{Context.Chat}</code>"))
                .Register("op_top" , () => _mail.WithText(TOP_OPTIONS))
                .Register("op_meme", () => _mail.WithText(MEME_OPTIONS))
                .Register("spam"   , () => _spam)
                .Register("tell"   , () => _tell)
                .Register("help"   , () => _help)
                .Build();
        }

        protected override void Run()
        {
            if (ChatsDealer.WitlessExist(Chat, out var baka))
            {
                _witlessRouter.Execute(WitlessContext.From(Context, baka));
            }
            else if (Context is { Command: not null, IsForMe: true })
            {
                var success = HandleSimpleCommands() || HandleStartCommand();

                if (success == false && (Context.ChatIsPrivate || Context.BotMentioned))
                {
                    Bot.SendMessage(Chat, WITLESS_ONLY_COMAND);
                }
            }
        }

        public bool HandleSimpleCommands()
        {
            var func = _simpleCommands.Resolve(Command);

            func?.Invoke().Execute(Context);
            return func is not null;
        }

        private bool HandleStartCommand()
        {
            var success = Command == "/start" && ChatsDealer.SussyBakas.TryAdd(Chat, Witless.GetAverageBaka(Context));
            if (success)
            {
                ChatsDealer.SaveChatList();
                Log($"{Title} >> DIC CREATED >> {Chat}", ConsoleColor.Magenta);
                Bot.SendMessage(Chat, START_RESPONSE);
                Bot.ThorRagnarok.PullBanStatus(Chat);
            }
            return success;
        }

        public override void OnCallback(CallbackQuery query)
        {
            if (query.Data == null || query.Message == null) return;

            var data = query.GetData();
            if (data[0] == "man") _help.HandleCallback(query, data);

            _witlessRouter.OnCallback(query);
        }
    }
}