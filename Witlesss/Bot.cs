using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Witlesss
{
    public partial class Bot
    {
        public readonly BanHammer ThorRagnarok;

        public readonly TelegramBotClient Client;
        public readonly User Me;

        public static Bot Instance = null!;

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
                    LogError("NO INTERNET? >> " + FixedErrorMessage(e.Message));
                    Task.Delay(5000).Wait();
                }
            }

            Router = command;

            Instance = this;
            Config.SetBotUsername(Me.Username!);

            ThorRagnarok = new BanHammer();
        }

        private void Run()
        {
            ThorRagnarok.GiveBans();

            ClearTempFiles();

            ChatsDealer.LoadSomeBakas();
            StartListening();
            ChatsDealer.StartAutoSaveLoop(minutes: 2);

            new ConsoleUI().EnterConsoleLoop();
        }
    }
}