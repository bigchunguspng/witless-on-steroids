using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Text.Core;
using PF_Bot.Telegram;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Types;

namespace PF_Bot.Routing_Legacy.Commands
{
    public abstract class AnyCommand<TContext> where TContext : CommandContext_Legacy
    {
        protected static Bot Bot => App.Bot;

        public TContext Context { get; protected set; } = null!;

        public Message Message  => Context.Message;
        public long    Chat     => Context.Chat;
        public string  Title    => Context.Title;
        public string? Text     => Context.Text;
        public string? Command  => Context.Command;
        public string? Args     => Context.Args;

        public MessageOrigin Origin => Context.Origin;

        public abstract void Execute(TContext context);
    }

    /// Blocking command. Should be used for short simple actions!
    /// The same instance can be used unlimited amount of times.
    public abstract class AnySyncCommand<TContext> : AnyCommand<TContext> where TContext : CommandContext_Legacy
    {
        public sealed override void Execute(TContext context)
        {
            Context = context;
            Run();
        }

        protected abstract void Run();
    }

    /// <summary>
    /// Non-blocking command. Should be used for time consuming actions!
    /// A new instance should be created every time!
    /// </summary>
    public abstract class AnyAsyncCommand<TContext> : AnyCommand<TContext> where TContext : CommandContext_Legacy
    {
        public sealed override async void Execute(TContext context)
        {
            try
            {
                Context = context;

                await Run();
            }
            catch (ProcessException e)
            {
                LogError($"{Context.Title} >> PROCESS FAILED | {e.File} / {e.Result.ExitCode}");
                Bot.SendErrorDetails(e, Context.Origin);
            }
            catch (Exception e)
            {
                Bot.HandleCommandException(e, Context);
            }
        }

        protected abstract Task Run();
    }

    // SIMPLE / WITLESS

    public abstract class  SyncCommand :  AnySyncCommand<CommandContext_Legacy>;
    public abstract class AsyncCommand : AnyAsyncCommand<CommandContext_Legacy>;

    public abstract class  WitlessSyncCommand :  AnySyncCommand<WitlessContext>
    {
        public ChatSettings Data => Context.Settings;
        public Copypaster   Baka => Context.Baka;

        public FilePath PackPath => PackManager.GetPackPath(Chat);
    }

    public abstract class WitlessAsyncCommand : AnyAsyncCommand<WitlessContext>
    {
        public ChatSettings Data => Context.Settings;
        public Copypaster   Baka => Context.Baka;

        public FilePath PackPath => PackManager.GetPackPath(Chat);
    }
}