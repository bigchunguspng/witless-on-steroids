using PF_Bot.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Text.Core;
using PF_Bot.Telegram;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PF_Bot.Routing.Commands;

public abstract class CommandHandler
{
    protected static Bot Bot => App.Bot;

    // CONTEXT

    protected CommandContext Context { get; set; } = null!;

    protected MessageOrigin Origin => Context.Origin;

    protected Message Message  => Context.Message;
    protected long    Chat     => Context.Chat;
    protected string  Title    => Context.Title;
    protected string  Command  => Context.Command;
    protected string  Options  => Context.Options ?? "";
    protected string? Args     => Context.Args;

    protected      FilePath ? Input   => Context.Input;
    protected List<FilePath>? Output  => Context.Output;

    protected ChatSettings Data     => Context.Settings;
    protected ChatSettings Settings => Context.Settings;
    protected Copypaster   Baka     => Context.Baka;

    [Flags]
    protected enum    CommandRequirements
    {
        None      = 0,
        KnownChat = 1, // chat has to be in chat list
        BotAdmin  = 2, // user has to be a bot admin (defined in config)
    }

    protected virtual CommandRequirements Requirements { get; } = CommandRequirements.None;

    private bool CommandShouldBeHandled()
    {
        if (Requirements == CommandRequirements.None) return true;

        if (Requirements.HasFlag(CommandRequirements.KnownChat))
        {
            if (ChatManager.Knowns(Chat).Janai())
            {
                Deny(DenyReason.ONLY_KNOWN_CHATS);
                return false;
            }
        }

        if (Requirements.HasFlag(CommandRequirements.BotAdmin))
        {
            if (Message.SenderIsBotAdmin().Janai())
            {
                Deny(DenyReason.ONLY_BOT_ADMINS);
                return false;
            }
        }

        return true;
    }

    // HANDLE

    public async Task Handle(CommandContext context)
    {
        Context = context;
        try
        {
            if (CommandShouldBeHandled())
            {
                await Handle_Internal();
            }
        }
        catch (Exception e)
        {
            HandleCommandError(context, e);
        }
        finally
        {
            Log();
            Reset();
        }
    }

    // HANDLE INTERNALS

    protected abstract Task Handle_Internal();

    protected int MessageToEdit { get; set; }

    protected enum CommandResultStatus
    {
        OK,   // all's good, according to keikaku
        MAN,  // bad syntax -> manual's sent
        BAD,  // 404 and similar
        FAIL, // error was thrown ((9
        DENY, // massive skill issue
    }

    protected CommandResultStatus Status { private get; set; } = CommandResultStatus.OK;

    protected void SendManual(string manual)
    {
        Status = CommandResultStatus.MAN;
        Bot.SendMessage(Origin, manual);
    }

    protected enum DenyReason
    {
        ONLY_BOT_ADMINS,
        ONLY_CHAT_ADMINS,
        ONLY_KNOWN_CHATS,
        ONLY_GROUPS,
    }

    protected void Deny(DenyReason reason)
    {
        Status = CommandResultStatus.DENY;
        switch (reason)
        {
            case DenyReason.ONLY_BOT_ADMINS:
                if (Chat.ChatIsPrivate())
                    Bot.SendMessage(Origin, FORBIDDEN.PickAny());
                break;
            case DenyReason.ONLY_CHAT_ADMINS:
                // (message is already sent)
                break;
            case DenyReason.ONLY_GROUPS:
                Bot.SendMessage(Origin, GROUPS_ONLY_COMAND);
                break;
            case DenyReason.ONLY_KNOWN_CHATS:
                if (Chat.ChatIsPrivate() || Context.BotMentioned)
                    Bot.SendMessage(Origin, WITLESS_ONLY_COMAND.Format(Bot.Username));
                break;
        }
    }

    protected void SendFile(FilePath output, MediaType type, string? name = null)
    {
        if (Output != null)
            Output.Add(output);
        else
        {
            using var stream = File.OpenRead(output);
            var file = InputFile.FromStream(stream, name);
            switch (type)
            {
                case MediaType.Photo: Bot.SendPhoto     (Origin, file); break;
                case MediaType.Stick: Bot.SendSticker   (Origin, file); break;
                case MediaType.Audio: Bot.SendAudio     (Origin, file); break;
                case MediaType.Anime: Bot.SendAnimation (Origin, file); break;
                case MediaType.Video: Bot.SendVideo     (Origin, file); break;
                case MediaType.Round: Bot.SendVideoNote (Origin, file); break;
                case MediaType.Voice: Bot.SendVoice     (Origin, file); break;
                case MediaType.Other: Bot.SendDocument  (Origin, file); break;
            }
        }
    }

    // ERROR / LOGGING

    private void HandleCommandError(CommandContext context, Exception exception)
    {
        Status = CommandResultStatus.FAIL;

        if (MessageToEdit > 0)
        {
            Bot.EditMessage(Chat, MessageToEdit, GetSillyErrorMessage());
        }

        if (exception is ProcessException e)
        {
            LogError($"{context.Title} >> PROCESS FAILED | {e.File} / {e.Result.ExitCode}");
            Bot.SendErrorDetails(e, context.Origin);
        }
        else
        {
            // log to err.mkd
            Bot.SendMessage(Origin, GetSillyErrorMessage());
            Bot.LogError_ToFile(exception, context, context.Title);
        }
    }

    private void Log()
    {
        if (Input.HasValue || Output != null) return; // auto mode

        var A = GetMessageTypeChar(Message.ReplyToMessage);
        var B = GetMessageTypeChar(Message);
        var text = $"{Status,-4} {A}{B} {Context.Text}";

        Telemetry.LogCommand(Chat, text);
    }

    private void Reset()
    {
        Status = CommandResultStatus.OK;
        MessageToEdit = 0;
    }

    private static char GetMessageTypeChar
        (Message? m) => m == null ? '-' : m.Type switch
    {
        MessageType.Text      => 'T',
        MessageType.Photo     => 'P',
        MessageType.Sticker   => 'S',
        MessageType.Animation => 'G',
        MessageType.Video     => 'V',
        MessageType.VideoNote => 'V',
        MessageType.Audio     => 'A',
        MessageType.Voice     => 'A',
        MessageType.Document  => 'D',
        _                     => '?',
    };
}