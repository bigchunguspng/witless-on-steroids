using PF_Bot.Core;
using PF_Bot.Routing_New.Routers;
using PF_Bot.Routing.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Telegram
{
    public partial class Bot
    {
        public readonly TelegramBotClient Client;
        public readonly User Me;

        /// Lowercase bot username with "@" symbol.
        public readonly string Username;

        public static async Task<Bot> Create(SyncCommand command, ICallbackRouter callback)
        {
            var options = new TelegramBotClientOptions(Config.TelegramToken)
            {
                RetryThreshold = 300,
                RetryCount = 5,
            };

            var client = new TelegramBotClient(options)
            {
                Timeout = TimeSpan.FromMinutes(5),
            };

            var me = await client.GetMe_AtAllCost();
            return new Bot(client, me, command, callback);
        }

        private Bot(TelegramBotClient client, User me, SyncCommand command, ICallbackRouter callback)
        {
            Client   = client;
            Me       =     me;
            Username = $"@{me.Username!.ToLower()}";
            Router_Command  = command;
            Router_Callback = callback;
        }
    }
}