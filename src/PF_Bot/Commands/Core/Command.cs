using PF_Bot.Generation;
using PF_Bot.State.Chats;
using Telegram.Bot.Types;

namespace PF_Bot.Commands.Core
{
    public abstract class AnyCommand<TContext> where TContext : CommandContext
    {
        protected static Bot Bot => Bot.Instance;

        public TContext Context { get; protected set; } = default!;

        public Message Message  => Context.Message;
        public long    Chat     => Context.Chat;
        public string  Title    => Context.Title;
        public string? Text     => Context.Text;
        public string? Command  => Context.Command;
        public string? Args     => Context.Args;

        public MessageOrigin Origin => Context.Origin;

        public abstract void Execute(TContext context);
    }

    /// <summary>
    /// Blocking command. Should be used for short simple actions!
    /// The same instance can be used unlimited amount of times.
    /// </summary>
    public abstract class AnySyncCommand<TContext> : AnyCommand<TContext> where TContext : CommandContext
    {
        public sealed override void Execute(TContext context)
        {
            try
            {
                Context = context;
                Run();
            }
            finally
            {
                Context = default!;
            }
        }

        protected abstract void Run();
    }

    /// <summary>
    /// Non-blocking command. Should be used for time consuming actions!
    /// A new instance should be created every time!
    /// </summary>
    public abstract class AnyAsyncCommand<TContext> : AnyCommand<TContext> where TContext : CommandContext
    {
        public sealed override async void Execute(TContext context)
        {
            Context = context;
            try
            {
                await Run();
            }
            catch (Exception e)
            {
                Bot.HandleCommandException(e, Context);
            }
        }

        protected abstract Task Run();
    }

    // SIMPLE / WITLESS

    public abstract class  SyncCommand :  AnySyncCommand<CommandContext>;
    public abstract class AsyncCommand : AnyAsyncCommand<CommandContext>;

    public abstract class  WitlessSyncCommand :  AnySyncCommand<WitlessContext>
    {
        public ChatSettings    Data => Context.Settings;
        public CopypasterProxy Baka => Context.Baka;

        public string PackPath => ChatManager.GetPackPath(Chat);
    }

    public abstract class WitlessAsyncCommand : AnyAsyncCommand<WitlessContext>
    {
        public ChatSettings    Data => Context.Settings;
        public CopypasterProxy Baka => Context.Baka;

        public string PackPath => ChatManager.GetPackPath(Chat);
    }

    // ROUTING

    public abstract class CommandAndCallbackRouter : SyncCommand
    {
        public abstract void OnCallback(CallbackQuery query);
    }
}