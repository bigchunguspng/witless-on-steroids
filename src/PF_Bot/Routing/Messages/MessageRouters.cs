using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Memes.Commands;
using PF_Bot.Routing.Commands;
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
        if (Message.Type == MessageType.Text || Settings.Pics == 0) return false;

        if      (Message.GetPhoto       () is { } f1 && WouldMeme     ) GetMemeMaker(f1).ProcessPhoto(f1);
        else if (Message.GetImageSticker() is { } f2 && WouldMemeStick) GetMemeMaker(f2).ProcessStick(f2);
        else if (Message.GetAnimation   () is { } f3 && WouldMeme     ) GetMemeMaker(f3).ProcessVideo(f3);
        else if (Message.GetVideoSticker() is { } f4 && WouldMemeStick) GetMemeMaker(f4).ProcessVideo(f4, ".webm");
        else
            return false;

        return true;
    }

    private bool WouldMeme      => Fortune.LuckyFor(Settings.Pics) && Message.ContainsSpoilers().Janai();
    private bool WouldMemeStick => Settings.Stickers && WouldMeme;

    private AutoMemesHandler GetMemeMaker(FileBase file)
    {
        var context = new CommandContext(Message);
        var mematic = CreateMemeMaker(Settings.Type);
        mematic.Automemes_PassContext(context);

        if (mematic is Demo_Dg dg)
        {
            var (w, h) = file.TryGetSize();
            dg.SelectMode(w, h);
        }

        var s = Settings;
        Telemetry.LogAuto(Chat, s.Pics, $"/{s.Type.ToString().ToLower()}{s.Options?[s.Type]}");

        return mematic;
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