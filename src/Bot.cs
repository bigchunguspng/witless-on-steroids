using Telegram.Bot;
using Telegram.Bot.Types;

namespace Witlesss
{
    public partial class Bot
    {
        public readonly BanHammer ThorRagnarok;

        public readonly TelegramBotClient Client;
        public readonly User Me;

        /// <summary> Lowercase bot username with "@" symbol. </summary>
        public static string Username { get; private set; } = null!;
        public static Bot    Instance { get; private set; } = null!;

        public static void LaunchInstance(CommandAndCallbackRouter command) => new Bot(command).Run();

        private Bot(CommandAndCallbackRouter command)
        {
            Client = new TelegramBotClient(Config.TelegramToken);
            while (true)
            {
                try
                {
                    Me = Client.GetMeAsync().Result;
                    break;
                }
                catch (Exception e)
                {
                    LogError("NO INTERNET? >> " + e.GetFixedMessage());
                    Task.Delay(5000).Wait();
                }
            }

            Router = command;

            Instance = this;
            Username = $"@{Me.Username!.ToLower()}";

            ThorRagnarok = new BanHammer();
        }

        private void Run()
        {
            ThorRagnarok.GiveBans();

            ClearTempFiles();

            StartListening();
            ChatsDealer.StartAutoSaveLoop(minutes: 2);

            new ConsoleUI().EnterConsoleLoop();
        }
    }
}