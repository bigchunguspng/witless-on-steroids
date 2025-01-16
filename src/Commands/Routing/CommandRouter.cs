using Telegram.Bot.Types;
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
        private readonly Piece _piece = new();
        private readonly React _react = new();
        private readonly Reply _reply = new();
        private readonly SendMessage _mail = new();
        private readonly DebugMessage _debug = new();
        private readonly AliasFFMpeg _apeg = new();
        private readonly AliasMagick _aim = new();
        private readonly Htmlizer _html = new();

        private readonly WitlessCommandRouter _witlessRouter;

        private readonly CommandRegistry<AnyCommand<CommandContext>> _simpleCommands;

        public CommandRouter()
        {
            _witlessRouter = new WitlessCommandRouter(this);

            _simpleCommands = new CommandRegistry<AnyCommand<CommandContext>>()
                .Register("fast"   , () => new ChangeSpeed().SetMode(ChangeSpeed.Mode.Fast))
                .Register("slow"   , () => new ChangeSpeed().SetMode(ChangeSpeed.Mode.Slow))
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
                .Register("load"   , () => new Load())
                .Register("upload" , () => new UploadSound())
                .Register("im"     , () => new Magick())
                .Register("peg"    , () => new FFMpeg())
                .Register("w"      , () => new BrowseReddit())
                .Register("link"   , () => new GetRedditLink())
                .Register("run"    , () => new RunProcess())
                .Register("kill"   , () => new KillProcess())
                .Register("piece"  , () => _piece)
                .Register("apeg"   , () => _apeg)
                .Register("aim"    , () => _aim)
                .Register("debug"  , () => _debug)
                .Register("id"     , () => _mail.WithText($"<code>{Context.Chat}</code>"))
                .Register("op_meme", () => _mail.WithText(MEME_OPTIONS))
                .Register("op_top" , () => _mail.WithText(TOP_OPTIONS))
                .Register("op_dp"  , () => _mail.WithText(DP_OPTIONS))
                .Register("op_dg"  , () => _mail.WithText(DG_OPTIONS))
                .Register("op_nuke", () => _mail.WithText(NUKE_OPTIONS))
                .Register("fonts"  , () => _mail.WithText(FONTS_CHEAT_SHEET))
                .Register("html"   , () => _html.WithMode(Htmlizer.Mode.ToHtml))
                .Register("text"   , () => _html.WithMode(Htmlizer.Mode.FromHtml))
                .Register("spam"   , () => _spam)
                .Register("tell"   , () => _tell)
                .Register("help"   , () => _help)
                .Register("man"    , () => _help)
                .Register("re"     , () => _react)
                .Register("rep"    , () => _reply)
                .Register("man_g"    , () => _mail.WithText(G_MANUAL))
                .Register("man_crop" , () => _mail.WithText(CROP_MANUAL))
                .Register("man_shake", () => _mail.WithText(SHAKE_MANUAL))
                .Register("man_scale", () => _mail.WithText(SCALE_MANUAL))
                .Register("man_cut"  , () => _mail.WithText(CUT_MANUAL))
                .Register("man_sus"  , () => _mail.WithText(SUS_MANUAL))
                .Register("man_eq"   , () => _mail.WithText(EQ_MANUAL))
                .Register("man_vol"  , () => _mail.WithText(VOLUME_MANUAL))
                .Build();
        }

        protected override void Run()
        {
            if (ChatService.Knowns(Chat, out var settings))
            {
                _witlessRouter.Execute(WitlessContext.From(Context, settings));
            }
            else if (Context is { Command: not null, IsForMe: true })
            {
                var success = HandleSimpleCommands() || HandleStartCommand();

                if (success == false && (Context.ChatIsPrivate || Context.BotMentioned))
                {
                    Bot.SendMessage(Origin, string.Format(WITLESS_ONLY_COMAND, Bot.Username));
                }
            }
        }

        public bool HandleSimpleCommands()
        {
            var func = _simpleCommands.Resolve(Command);
            if (func != null)
            {
                Telemetry.LogCommand(Context.Chat, Context.Text);

                func.Invoke().Execute(Context);
            }

            return func != null;
        }

        private bool HandleStartCommand()
        {
            var success = Command == "/start" && ChatService.TryAddChat(Chat, ChatSettingsFactory.CreateFrom(Context));
            if (success)
            {
                Telemetry.LogCommand(Context.Chat, Context.Text);

                ChatService.SaveChatsDB();
                Log($"{Title} >> DIC CREATED >> {Chat}", LogLevel.Info, 13);
                Bot.SendMessage(Origin, START_RESPONSE);
            }
            return success;
        }

        public override void OnCallback(CallbackQuery query)
        {
            if (query.Data == null || query.Message == null) return;

            var data = query.GetData();
            if (data[0] == "man") _help.HandleCallback(query, data);
            if (data[0] == "ap" ) _apeg.HandleCallback(query, data);
            if (data[0] == "ai" ) _aim .HandleCallback(query, data);

            _witlessRouter.OnCallback(query);
        }
    }
}