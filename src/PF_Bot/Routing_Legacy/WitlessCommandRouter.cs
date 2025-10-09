using PF_Bot.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Memes.Commands;
using PF_Bot.Features_Main.Text.Commands;
using PF_Bot.Routing_Legacy.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Routing_Legacy;

public class WitlessCommandRouter : WitlessSyncCommand
{
    /*
    private readonly ChatInfo _chat = new();
    private readonly Baguette _baguette = new();
    private readonly Move _move = new();
    private readonly Set _set = new();
    private readonly SetSpeech _speech = new();
    private readonly SetPics _pics = new();
    private readonly SetQuality _quality = new();
    private readonly ToggleStickers _stickers = new();
    private readonly ToggleAdmins _admins = new();
    private readonly Delete _delete = new();
    */

    private readonly CommandRouter _parent;

    private readonly Dictionary<MemeType, Func<ImageProcessor>> _mematics;

    private readonly CommandRegistry<Func<AnyCommand<WitlessContext>>> _witlessCommands;

    public WitlessCommandRouter(CommandRouter parent)
    {
        _parent = parent;

        _mematics = new Dictionary<MemeType, Func<ImageProcessor>>
        {
            { MemeType.Dg,   () => new Demo_Dg() },
            { MemeType.Meme, () => new Meme() },
            { MemeType.Top,  () => new Top() },
            { MemeType.Dp,   () => new Demo_Dp() },
            { MemeType.Snap, () => new Snap() },
            { MemeType.Nuke, () => new Nuke() },
        };

        _witlessCommands = new CommandRegistry<Func<AnyCommand<WitlessContext>>>.Builder()
            /*
            .Register("dp"      , () => new Demo_Dp())
            .Register("dg"      , () => new Demo_Dg().SetMode(Demo_Dg.Mode.Square))
            .Register("dv"      , () => new Demo_Dg().SetMode(Demo_Dg.Mode.Wide))
            .Register("meme"    , () => new Meme())
            .Register("top"     , () => new Top())
            .Register("snap"    , () => new Snap())
            .Register("nuke"    , () => new Nuke())
            .Register("a"       , () => new GenerateByFirstWord())
            .Register("zz"      , () => new GenerateByLastWord())
            .Register("fuse"    , () => new Fuse())
            .Register("board"   , () => new EatBoards())
            .Register("plank"   , () => new EatPlanks())
            .Register("xd"      , () => new EatReddit())
            .Register("b"       , () => _baguette)
            .Register("set"     , () => _set)
            .Register("speech"  , () => _speech)
            .Register("quality" , () => _quality)
            .Register("pics"    , () => _pics)
            .Register("stickers", () => _stickers)
            .Register("chat"    , () => _chat)
            .Register("pub"     , () => _move.WithMode(ExportMode.Public))
            .Register("move"    , () => _move.WithMode(ExportMode.Private))
            .Register("delete"  , () => _delete)
            .Register("admins"  , () => _admins)
            */
            .Build();
    }

    protected override void Run()
    {
        if (Text is not null)
        {
            if (Context is { Command: not null, IsForMe: true })
            {
                if (_parent.HandleSimpleCommands().Failed()) HandleWitlessCommands(Data);
                return;
            }
            else if (Baka.Eat(Text, out var eaten))
            {
                foreach (var line in eaten) Log($"{Title} >> {line}", LogLevel.Info, LogColor.Blue);
            }
        }

        if (Data.Type is MemeType.Auto)
        {
            if (Data.Options != null && TryAutoHandleMessage().Failed()) TryLuckForFunnyText();
        }
        else if (Message.GetPhoto       () is { } f1 && HaveToMeme       ()) GetMemeMaker(f1).ProcessPhoto(f1);
        else if (Message.GetImageSticker() is { } f2 && HaveToMemeSticker()) GetMemeMaker(f2).ProcessStick(f2);
        else if (Message.GetAnimation   () is { } f3 && HaveToMeme       ()) GetMemeMaker(f3).ProcessVideo(f3);
        else if (Message.GetVideoSticker() is { } f4 && HaveToMemeSticker()) GetMemeMaker(f4).ProcessVideo(f4, ".webm");
        else TryLuckForFunnyText();

        // LOCALS

        void TryLuckForFunnyText()
        {
            if (Fortune.LuckyFor(Data.Speech).Janai()) return;

            Telemetry.LogAuto(Context.Chat, Data.Speech, "FUNNY");

            new PoopText().Execute(Context);
        }

        bool TryAutoHandleMessage()
        {
            var command = AutoHandler.TryGetMessageHandler(Context, Data);
            if (command is null) return false;
            
            AutoHandleCommand(Data, command);
            
            return true;
        }

        ImageProcessor GetMemeMaker(FileBase file)
        {
            var mematic = _mematics[Data.Type].Invoke();
            mematic.Pass(Context);
            if (mematic is Demo_Dg dg)
            {
                var (w, h) = file.TryGetSize();
                dg.SelectMode(w, h);
            }

            Telemetry.LogAuto(Context.Chat, Data.Pics, $"/{Data.Type.ToString().ToLower()}{Data.Options?[Data.Type]}");

            return mematic;
        }

        bool HaveToMeme       () => Fortune.LuckyFor(Data.Pics) && Message.ContainsSpoilers().Janai();
        bool HaveToMemeSticker() => Data.Stickers && HaveToMeme();
    }

    private void AutoHandleCommand(ChatSettings settings, string command)
    {
        Context.UseText(command);
        Telemetry.LogAutoCommand(Context.Chat, Context.Text);

        var funcS = _parent.SimpleCommands.Resolve(Command?[1..]);
        if (funcS != null)
        {
            funcS.Invoke().Execute(Context);
        }
        else
        {
            var funcW = _witlessCommands.Resolve(Command?[1..]);
            funcW?.Invoke().Execute(WitlessContext.From(Context, settings));
        }
    }

    private void HandleWitlessCommands(ChatSettings settings)
    {
        Telemetry.LogCommand(Context.Chat, Context.Text);

        var func = _witlessCommands.Resolve(Command?[1..]);
        func?.Invoke().Execute(WitlessContext.From(Context, settings));
    }
}