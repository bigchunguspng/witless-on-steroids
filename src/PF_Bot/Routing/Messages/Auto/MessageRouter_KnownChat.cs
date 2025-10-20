using PF_Bot.Features_Aux.Packs;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Memes.Commands;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Routing.Messages.Auto;

public class MessageRouter_KnownChat
    (CommandRegistry<Func<CommandHandler>> registry) : MessageHandler
{
    private ChatSettings Settings = null!;

    public void Route(MessageContext context, ChatSettings settings)
    {
        Context  = context;
        Settings = settings;

        if (Text != null /* && settings.Learn == true */)
        {
            var baka = PackManager.GetBaka(Chat);
            if (baka.Eat(Text, out var eaten))
            {
                foreach (var line in eaten) Log($"{Title} >> {line}", LogLevel.Info, LogColor.Blue);
            }
        }

        if (TryAuto().Failed())
            TrySendFunnyText();
    }

    private bool TryAuto() => Settings.Type switch
    {
        MemeType.Auto => TryCallAutoHandler(),
        _             => TryMakeAutoMeme   (),
    };

    private void TrySendFunnyText()
    {
        if (Fortune.LuckyFor(Settings.Speech).Janai()) return;

        _ = new PoopText(Settings.Speech).Run(Context);
    }

    // AUTOHANDLER // todo: move code below to own modules, move this class back to MR.cs

    private bool TryCallAutoHandler()
    {
        var expression = Settings.Options?[MemeType.Auto];
        if (expression == null)
            return false;

        var input = AutoHandler.TryGetHandlerInput(Context, expression);
        if (input == null)
            return false;

        var func = registry.Resolve(input, out var command);
        if (func == null)
            return false;

        var context = CommandContext.CreateForAuto(Message, command!, input.TrimEnd(), CommandMode.AUTO);

        var handler = func.Invoke();
        _ = handler.Handle(context);

        return true;
    }

    // AUTOMEMES

    [Flags] private enum AutoMemeType
    {
        Image = 1,
        Stick = 2,
        Video = 4,
        VideoStick = Video | Stick,
    }

    private bool TryMakeAutoMeme()
    {
        if (Message.Type == MessageType.Text || Settings.Pics == 0)
            return false;

        FileBase file;
        AutoMemeType type;

        if      (Message.GetPhoto       () is { } f1) (file, type) = (f1, AutoMemeType.Image);
        else if (Message.GetImageSticker() is { } f2) (file, type) = (f2, AutoMemeType.Stick);
        else if (Message.GetAnimation   () is { } f3) (file, type) = (f3, AutoMemeType.Video);
        else if (Message.GetVideoSticker() is { } f4) (file, type) = (f4, AutoMemeType.VideoStick);
        else
            return false;

        var sticker = type.HasFlag(AutoMemeType.Stick);
        var skip = sticker && Settings.Stickers.IsOff() || WouldMeme.Janai();
        if (skip)
            return false;

        var command = Settings.Type.ToLower();
        var context = CommandContext.CreateForAutoMemes(Message, command!);
        var mematic = CreateMemeMaker(Settings.Type);
        if (mematic is Demo_Dg dg)
        {
            var (w, h) = file.TryGetSize();
            dg.SelectMode(w, h);
        }

        mematic.Automemes_PassContext(context);

        var makeMeme = type switch
        {
            AutoMemeType.Image      => mematic.ProcessPhoto(file),
            AutoMemeType.Stick      => mematic.ProcessStick(file),
            AutoMemeType.Video      => mematic.ProcessVideo(file),
            AutoMemeType.VideoStick => mematic.ProcessVideo(file, ".webm"),
            _ => throw new ArgumentException("Bro added a new automeme type..."),
        };

        _ = new AutoMeme().Run(makeMeme, context);

        return true;
    }

    private bool WouldMeme => Fortune.LuckyFor(Settings.Pics) && Message.ContainsSpoilers().Janai();

    private AutoMemesHandler CreateMemeMaker(MemeType type) => type switch
    {
        MemeType.Dg   => new Demo_Dg(),
        MemeType.Dp   => new Demo_Dp(),
        MemeType.Meme => new Meme(),
        MemeType.Top  => new Top(),
        MemeType.Snap => new Snap(),
        MemeType.Nuke => new Nuke(),
        _ => throw new ArgumentException("Bro added a new meme type..."),
    };
}