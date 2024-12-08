using Telegram.Bot.Types;
using Witlesss.Commands.Generation;
using Witlesss.Commands.Meme;
using Witlesss.Commands.Meme.Core;
using Witlesss.Commands.Packing;
using Witlesss.Commands.Settings;

namespace Witlesss.Commands.Routing;

public class WitlessCommandRouter : WitlessSyncCommand
{
    private readonly ChatInfo _chat = new();
    private readonly Bouhourt _bouhourt = new();
    private readonly Move _move = new();
    private readonly Set _set = new();
    private readonly SetSpeech _speech = new();
    private readonly SetPics _pics = new();
    private readonly SetQuality _quality = new();
    private readonly ToggleStickers _stickers = new();
    private readonly ToggleAdmins _admins = new();
    private readonly Delete _delete = new();

    private readonly CommandRouter _parent;

    private readonly Dictionary<MemeType, Func<ImageProcessor>> _mematics;

    private readonly CommandRegistry<AnyCommand<WitlessContext>> _witlessCommands;

    public WitlessCommandRouter(CommandRouter parent)
    {
        _parent = parent;

        _mematics = new Dictionary<MemeType, Func<ImageProcessor>>
        {
            { MemeType.Dg,   () => new Demotivate() },
            { MemeType.Meme, () => new MakeMeme() },
            { MemeType.Top,  () => new Top() },
            { MemeType.Dp,   () => new Demotivate3000() },
            { MemeType.Nuke, () => new Nuke() }
        };

        _witlessCommands = new CommandRegistry<AnyCommand<WitlessContext>>()
            .Register("dp"      , () => new Demotivate3000())
            .Register("dg"      , () => new Demotivate().SetMode(Demotivate.Mode.Square))
            .Register("dv"      , () => new Demotivate().SetMode(Demotivate.Mode.Wide))
            .Register("meme"    , () => new MakeMeme())
            .Register("top"     , () => new Top())
            .Register("nuke"    , () => new Nuke())
            .Register("a"       , () => new GenerateByFirstWord())
            .Register("zz"      , () => new GenerateByLastWord())
            .Register("fuse"    , () => new Fuse())
            .Register("board"   , () => new EatBoards())
            .Register("plank"   , () => new EatPlanks())
            .Register("xd"      , () => new EatReddit())
            .Register("b"       , () => _bouhourt)
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
            .Build();
    }

    protected override void Run()
    {
        if (Text is not null)
        {
            if (Context is { Command: not null, IsForMe: true })
            {
                if (!_parent.HandleSimpleCommands()) HandleWitlessCommands(Data);
                return;
            }
            else if (Baka.Eat(Text, out var eaten))
            {
                foreach (var line in eaten) Log($"{Title} >> {line}", LogLevel.Info, 12);
            }
        }

        if      (Message.GetPhoto       () is { } f1 && HaveToMeme       ()) GetMemeMaker(f1).ProcessPhoto(f1);
        else if (Message.GetImageSticker() is { } f2 && HaveToMemeSticker()) GetMemeMaker(f2).ProcessStick(f2);
        else if (Message.GetAnimation   () is { } f3 && HaveToMeme       ()) GetMemeMaker(f3).ProcessVideo(f3);
        else if (Message.GetVideoSticker() is { } f4 && HaveToMemeSticker()) GetMemeMaker(f4).ProcessVideo(f4, ".webm");
        else if (LuckyFor(Data.Speech))
        {
            new PoopText().Execute(Context);
        }

        ImageProcessor GetMemeMaker(FileBase file)
        {
            var mematic = _mematics[Data.Type].Invoke();
            mematic.Pass(Context);
            if (mematic is Demotivate dg)
            {
                var (w, h) = file.TryGetSize();
                dg.SelectMode(w, h);
            }

            return mematic;
        }

        bool HaveToMeme       () => LuckyFor(Data.Pics) && !Message.ContainsSpoilers();
        bool HaveToMemeSticker() => Data.Stickers && HaveToMeme();
    }

    private void HandleWitlessCommands(ChatSettings settings)
    {
        var func = _witlessCommands.Resolve(Command);
        func?.Invoke().Execute(WitlessContext.From(Context, settings));
    }

    private readonly Lazy<EatBoards> _boards = new(new EatBoards());
    private readonly Lazy<EatPlanks> _planks = new(new EatPlanks());

    public void OnCallback(CallbackQuery query)
    {
        var data = query.GetData();
        if      (data[0].StartsWith('b')) _boards.Value.HandleCallback(query, data);
        if      (data[0].StartsWith('p')) _planks.Value.HandleCallback(query, data);
        else if (data[0].StartsWith('f'))          Fuse.HandleCallback(query, data);
        else if (data[0] == "del")
        {
            query.Message!.From = query.From;
            _delete.DoGameStep(query.Message, data[1]);
        }
    }
}