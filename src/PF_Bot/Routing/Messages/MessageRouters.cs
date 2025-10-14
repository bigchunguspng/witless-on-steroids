using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Memes.Commands;
using PF_Bot.Routing.Commands;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Routing.Messages;

public interface IMessageRouter
{
    void Route(Message message);
}

public class MessageRouter_Skip : MessageHandler, IMessageRouter
{
    public void Route(Message message)
    {
        Context = new MessageContext(message);

        Print($"{Title} >> {Text}", ConsoleColor.Gray);
    }
}

public class MessageRouter_Default
    (CommandRegistry<Func<CommandHandler>> registry) : MessageHandler, IMessageRouter
{
    private readonly MessageRouter_Default_KnownChat _branch_KnownChat = new(registry);

    public void Route(Message message)
    {
        Context = new MessageContext(message);

        var messageIsCommand = Text != null && Text.StartsWith('/');
        if (messageIsCommand && HandleCommand())
            return;

        if (ChatManager.Knowns(Chat, out var settings))
            _branch_KnownChat.Route(Context, settings);
    }

    private bool HandleCommand()
    {
        var handler = registry.Resolve(Text, out var command, offset: 1);
        if (handler != null)
        {
            var context = new CommandContext(Context.Message, command!);

            var forMe = CommandIsForMe(context);
            if (forMe)
            {
                _ = handler.Invoke().Handle(context);
            }

            return forMe;
        }

        return false;
    }

    private bool CommandIsForMe(CommandContext context)
    {
        var options = context.Options;
        if (options == null) 
            return true; // no bot mentioned

        var mention_start = options.IndexOf("@", StringComparison.Ordinal);
        if (mention_start < 0) 
            return true; // no bot mentioned x2

        var mention_tail = options.LastIndexOf("bot", StringComparison.OrdinalIgnoreCase);
        if (mention_tail < mention_start)
            return true; // Options: ……@…… / ……bot…@……

        return false; // some other bot mentioned
    }
}

public class MessageRouter_Default_KnownChat
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

        Telemetry.LogAuto(Chat, Settings.Speech, "FUNNY");

        _ = new PoopText().Run(Context);
    }

    // AUTOHANDLER

    private bool TryCallAutoHandler()
    {
        if (Settings.Options == null)
            return false;

        var input = AutoHandler.TryGetMessageHandler(Context, Settings);
        if (input == null)
            return false;

        var func = registry.Resolve(input, out var command);
        if (func == null)
            return false;
        
        Telemetry.LogAutoCommand(Context.Chat, Context.Text);

        var text    = input.Replace("THIS", Context.Text);
        var context = new CommandContext(Message, command!, text);

        var handler = func.Invoke();
        _ = handler.Handle(context);

        return true;
    }

    // AUTOMEMES

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

        var context = new CommandContext(Message);
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

        _ = HandleAutoMeme(makeMeme, context);

        return true;
    }

    [Flags] private enum AutoMemeType
    {
        Image = 1,
        Stick = 2,
        Video = 4,
        VideoStick = Video | Stick,
    }

    private bool WouldMeme => Fortune.LuckyFor(Settings.Pics) && Message.ContainsSpoilers().Janai();

    private async Task HandleAutoMeme(Task makeMeme, CommandContext context)
    {
        try
        {
            await makeMeme;
        }
        catch (Exception exception)
        {
            if (exception is ProcessException e)
            {
                Unluckies.HandleProcessException(e, context);
            }
            else
            {
                App.Bot.SendMessage(context.Origin, GetSillyErrorMessage());
                Unluckies.Handle(exception, context, $"AUTOMEMES | {context.Title}");
            }
        }
        finally
        {
            //todo new logging
            var s = Settings;
            Telemetry.LogAuto(Chat, s.Pics, $"/{s.Type.ToString().ToLower()}{s.Options?[s.Type]}");
            /*var A = GetMessageTypeChar(Message.ReplyToMessage);
            var B = GetMessageTypeChar(Message);
            var text = $"{Status,-4} {A}{B} {Context.Text}";

            Telemetry.LogCommand(Chat, text);*/
        }
    }

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

public class PoopText : MessageHandler
{
    public async Task Run(MessageContext context)
    {
        Context = context;

        await Task.Delay(GetRealisticResponseDelay(Text));

        var baka = PackManager.GetBaka(Chat);
        var text = App.FunnyMessages.TryDequeue(Chat) ?? baka.Generate();

        App.Bot.SendMessage(Origin, text, preview: true);
        Log($"{Title} >> FUNNY");
    }

    private static int GetRealisticResponseDelay
        (string? text) => text == null
        ? 150
        : Math.Min(text.Length, 120) * 25; // 1 second / 40 characters, 3 seconds max
}