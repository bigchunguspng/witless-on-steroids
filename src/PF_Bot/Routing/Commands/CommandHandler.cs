using System.Text.Json;
using System.Text.Json.Serialization;
using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
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

    protected Message Message  => Context.Message;
    protected long    Chat     => Context.Chat;
    protected string  Title    => Context.Title;
    protected string  Command  => Context.Command;
    protected string  Options  => Context.Options ?? "";
    protected string? Args     => Context.Args;
    protected FilePath? Input  => Context.Input;

    protected MessageOrigin Origin => Context.Origin;

    // KNOWN CHAT CONTEXT

    protected ChatSettings Data     =>  Settings;

    private   ChatSettings?            _settings;
    protected ChatSettings Settings => _settings ??=
        ChatManager.Knowns(Chat, out var settings)
            ? settings
            : Requirements.HasFlag(CommandRequirements.Settings)
                ? throw new CommandRequirementsException()
                : ChatSettingsFactory.GetTemporary();

    private   Copypaster?        _baka;
    protected Copypaster Baka => _baka ??=
        ChatManager.Knowns(Chat)
            ? PackManager.GetBaka(Chat)
            : Requirements.HasFlag(CommandRequirements.Copypaster)
                ? throw new CommandRequirementsException()
                : DementiaCopypaster.Instance;

    [Flags]
    protected enum    CommandRequirements
    {
        None       = 1,
        Settings   = 2,
        Copypaster = 4,
    }

    private   class   CommandRequirementsException : Exception;

    protected virtual CommandRequirements Requirements { get; } = CommandRequirements.None;

    // HANDLE

    public async Task Handle(CommandContext context)
    {
        Context = context;
        try
        {
            await Handle_Internal();
        }
        catch (Exception e)
        {
            HandleCommandError(context, e);
        }
        finally
        {
            Log();
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
        ONLY_STARTED_BOT,
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
            case DenyReason.ONLY_STARTED_BOT:
                if (Chat.ChatIsPrivate() || Context.BotMentioned)
                    Bot.SendMessage(Origin, string.Format(WITLESS_ONLY_COMAND, Bot.Username));
                break;
        }
    }

    protected void SendFile(FilePath output, MediaType type, string? name = null)
    {
        // defalut: send file
        // todo pipe: pass to the next handler

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

    // ERROR / LOGGING

    private void HandleCommandError(CommandContext context, Exception exception)
    {
        Status = CommandResultStatus.FAIL;

        if (MessageToEdit > 0)
        {
            Bot.EditMessage(Chat, MessageToEdit, GetSillyErrorMessage());
        }

        if      (exception is CommandRequirementsException)
        {
            Status = CommandResultStatus.DENY;

            if (Chat.ChatIsPrivate() || Context.BotMentioned)
                Bot.SendMessage(Origin, string.Format(WITLESS_ONLY_COMAND, Bot.Username));
        }
        else if (exception is ProcessException e)
        {
            LogError($"{context.Title} >> PROCESS FAILED | {e.File} / {e.Result.ExitCode}");
            Bot.SendErrorDetails(e, context.Origin);
        }
        else
        {
            // log to err.mkd
            Bot.SendMessage(Origin, GetSillyErrorMessage());
            Bot.LogError_ToFile(exception, this, context.Title);
        }
    }

    private void Log()
    {
        if (Input.HasValue) return; // pipe mode

        var A = GetMessageTypeChar(Message.ReplyToMessage);
        var B = GetMessageTypeChar(Message);
        var text = $"{Status,-4} {A}{B} {Context.Text}";

        Telemetry.LogCommand(Chat, text);
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

    //

    public class JsonConverter : JsonConverter<CommandHandler>
    {
        public override CommandHandler Read
            (ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
            => throw new NotImplementedException();

        public override void Write
        (
            Utf8JsonWriter writer,
            CommandHandler value,
            JsonSerializerOptions options
        ) => writer.WriteObject(() =>
        {
            var context = value.Context;
            writer.WriteObject("message", context.Message, options);
            writer.WriteString("title", context.Title);
            writer.WriteString("text", context.Text);
            writer.WriteString("command", context.Command);
            writer.WriteString("options", context.Options);
            writer.WriteString("args", context.Args);
            writer.WriteNumber("chat", context.Chat);
            writer.WriteString("input", context.Input);
            writer.WriteBoolean("bot_mentioned", context.BotMentioned);
            if (value._settings != null)
            {
                writer.WriteObject("settings", value._settings, options);
            }
        });
    }
}