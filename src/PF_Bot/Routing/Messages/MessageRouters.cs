using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Memes.Commands;
using PF_Bot.Routing_Legacy;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

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

        if (Settings.Type is MemeType.Auto)
        {
            App.Bot.SendMessage(Origin, "автообработка закрыта на ремонт");
            // todo invoke automemes handler
            //if (Settings.Options != null && TryAutoHandleMessage().Failed()) TryLuckForFunnyText();
        }
        else if (Message.GetPhoto       () is { } f1 && HaveToMeme       ()) GetMemeMaker(f1).ProcessPhoto(f1);
        else if (Message.GetImageSticker() is { } f2 && HaveToMemeSticker()) GetMemeMaker(f2).ProcessStick(f2);
        else if (Message.GetAnimation   () is { } f3 && HaveToMeme       ()) GetMemeMaker(f3).ProcessVideo(f3);
        else if (Message.GetVideoSticker() is { } f4 && HaveToMemeSticker()) GetMemeMaker(f4).ProcessVideo(f4, ".webm");
        else TryLuckForFunnyText();
    }

    private void TryLuckForFunnyText()
    {
        if (Fortune.LuckyFor(Settings.Speech).Janai()) return;

        Telemetry.LogAuto(Chat, Settings.Speech, "FUNNY");

        _ = new PoopText().Run(Context);
    }

    private bool HaveToMemeSticker
        () => Settings.Stickers
           && HaveToMeme();

    private bool HaveToMeme
        () => Fortune.LuckyFor(Settings.Pics)
           && Message.ContainsSpoilers().Janai();

    private readonly Dictionary<MemeType, Func<ImageProcessor>> _mematics = new()
    {
        { MemeType.Dg,   () => new Demo_Dg() },
        { MemeType.Meme, () => new Meme() },
        { MemeType.Top,  () => new Top() },
        { MemeType.Dp,   () => new Demo_Dp() },
        { MemeType.Snap, () => new Snap() },
        { MemeType.Nuke, () => new Nuke() },
    };

    // making memes:
    // command: {image} [def] | /command[ops] [args]
    // auto:    {image} [def] |               [text]
    // autohan: {image} [def] | /command[ops] [args]
    private ImageProcessor GetMemeMaker(FileBase file)
    {
        var mematic = _mematics[Settings.Type].Invoke();
        mematic.Pass(Context); // todo fix Chat not set
        if (mematic is Demo_Dg dg)
        {
            var (w, h) = file.TryGetSize();
            dg.SelectMode(w, h);
        }

        Telemetry.LogAuto(Context.Chat, Settings.Pics, $"/{Settings.Type.ToString().ToLower()}{Settings.Options?[Settings.Type]}");

        return mematic;
    }

    /*private bool TryAutoHandleMessage()
    {
        var command = AutoHandler.TryGetMessageHandler(Context, Settings);
        if (command is null) return false;

        AutoHandleCommand(command);

        return true;
    }

    private void AutoHandleCommand(string command)
    {
        // split expression into commands
        // construct pipe
        // foreach handler in pipe:
            // run handler
            // for each result => run second handler
            
        // set custom result handler [default- / pipe-]?

        Context.UseText(command);
        Telemetry.LogAutoCommand(Context.Chat, Context.Text);

        if (registry.Resolve_AsMapping(command) is { } mapping)
        {
            var (command_found, handler) = mapping;

            _ = handler.Handle(new CommandContext_New(Message, command_found));
        }
    }*/
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