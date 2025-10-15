using PF_Bot.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Text.Core;
using PF_Bot.Telegram;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Types;

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

    protected CommandMode  Mode     => Context.Mode;
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
            HandleError(e);
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

    private CommandResultStatus Status {  get; set; } = CommandResultStatus.OK;

    protected void SetBadStatus() =>
        Status = CommandResultStatus.BAD;

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

    private void HandleError(Exception exception)
    {
        Status = CommandResultStatus.FAIL;

        if (MessageToEdit > 0)
            Bot.EditMessage(Chat, MessageToEdit, GetSillyErrorMessage());

        if (exception is ProcessException e)
        {
            Unluckies.HandleProcessException(e, Context);
        }
        else
        {
            if (MessageToEdit == 0)
                Bot.SendMessage(Origin, GetSillyErrorMessage());

            Unluckies.Handle(exception, Context, $"COMMAND H. | {Title}");
        }
    }

    private void Log()
    {
        var normal = Mode == CommandMode.NORMAL;
        if (normal) BigBrother.LogCommand(Chat, Status, Message, Context.Text);
        else        BigBrother.LogAuto   (Chat, Status, Message, GetAutoType(), GetAutoId(), $"/{Command}{Options} {Args}");
    }

    private string GetAutoId()
    {
        return Desert.TurnIntoSand(Message.GetHashCode_Binary());
    }

    private AutoType GetAutoType() =>
        Mode == CommandMode.AUTO ? AutoType.AUTO :
        Mode == CommandMode.PIPE ? AutoType.PIPE :
        throw new UnexpectedException("ONLY AUTO & PIPE MODE EXPECTED");

    private void Reset()
    {
        Status = CommandResultStatus.OK;
        MessageToEdit = 0;
    }
}