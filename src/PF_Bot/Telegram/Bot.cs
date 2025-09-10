using PF_Bot.Core;
using PF_Bot.Core.Chats;
using PF_Bot.Routing;
using PF_Bot.Routing.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Telegram
{
    public partial class Bot
    {
        public readonly TelegramBotClient Client;
        public readonly User Me;

        /// <summary> Lowercase bot username with "@" symbol. </summary>
        public static string Username { get; private set; } = null!;
        public static Bot    Instance { get; private set; } = null!;

        public static void LaunchInstance(string? args)
        {
            new Bot(args == null ? new CommandRouter() : new Skip()).Run(listen: args != "!");
        }

        private Bot(CommandAndCallbackRouter command)
        {
            var httpClient = new HttpClient() { Timeout = TimeSpan.FromMinutes(5) };

            Client = new TelegramBotClient(Config.TelegramToken, httpClient);
            Me       = GetMe();
            Username = $"@{Me.Username!.ToLower()}";
            Instance = this;
            Router   = command;
        }

        private void Run(bool listen = true)
        {
            ClearTempFiles();

            if (listen) StartListening();
            ChatManager.StartAutoSaveAsync(TimeSpan.FromMinutes(2));

            new ConsoleUI().EnterConsoleLoop();
        }


        private User GetMe()
        {
            while (true)
            {
                try
                {
                    return Client.GetMe().Result;
                }
                catch (Exception e)
                {
                    LogError($"NO INTERNET? | {e.GetErrorMessage()}");
                    Task.Delay(5000).Wait();
                }
            }
        }
    }
}