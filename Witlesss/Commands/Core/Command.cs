using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Witlesss.Commands.Core
{
    /*public abstract class BotNeighbour
    {
        protected static Bot Bot => Bot.Instance;
    }

    public abstract class AbstractCommand<TContext> where TContext : CommandContext
    {
        protected static Bot Bot => Bot.Instance;

        public abstract void Run(TContext c);
    }*/

    public abstract class AnyCommand<TContext> where TContext : CommandContext
    {
        public TContext Context { get; protected set; } = default!;

        public Message Message  => Context.Message;
        public long    Chat     => Context.Chat;
        public string  Title    => Context.Title;
        public string? Text     => Context.Text;
        public string? Command  => Context.Command;
        public string? Args     => Context.Args;
        public bool    IsForMe  => Context.IsForMe;

        protected static Bot Bot => Bot.Instance;

        public abstract void Execute(TContext context);
    }

    /// <summary>
    /// Can be reused.
    /// </summary>
    public abstract class AnySyncCommand<TContext> : AnyCommand<TContext> where TContext : CommandContext
    {
        public override void Execute(TContext context)
        {
            Context = context;
            Run();
            Context = default!;
        }

        protected abstract void Run();
    }

    public abstract class SyncCommand : AnySyncCommand<CommandContext>;

    public abstract class WitlessSyncCommand : AnySyncCommand<WitlessContext>
    {
        public Witless Baka => Context.Baka;
    }

    /// <summary>
    /// Is one time use.
    /// </summary>
    public abstract class AnyAsyncCommand<TContext> : AnyCommand<TContext> where TContext : CommandContext
    {
        public override async void Execute(TContext context)
        {
            Context = context;
            try
            {
                await Run();
            }
            catch (Exception e)
            {
                LogError($"ASYNC COMMAND >> {e.Message}");
            }
        }

        protected abstract Task Run();
    }

    public abstract class AsyncCommand : AnyAsyncCommand<CommandContext>;

    public abstract class WitlessAsyncCommand : AnyAsyncCommand<WitlessContext>
    {
        public Witless Baka => Context.Baka;
    }


    /*
    public abstract class Command : AbstractCommand<CommandContext>;

    public abstract class WitlessCommand : AbstractCommand<WitlessContext>;
    */

    // command.Run(request); can be async
    //  sync command -> new command(request).Run(); / command.Pass(request); command.Run();
    // async command -> new command(request).Run();
    
    // Command
    //   FileEditingCommand
    //     VideoCommand
    // WitlessCommand
    //   SettingsCommand

    public abstract class AnyCommandRouter<TContext> where TContext : CommandContext
    {
        protected static Bot Bot => Bot.Instance;

        public TContext Context { get; protected set; } = default!;

        public abstract void Run();
    }

    public abstract class CommandAndCallbackRouter : AnyCommandRouter<CommandContext>
    {
        public void Pass(Message message)
        {
            Context = new CommandContext(message);
        }

        public abstract void OnCallback(CallbackQuery query);
    }
}