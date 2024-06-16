using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Witlesss
{
    public class Bot : BotCore
    {
        private readonly ConsoleUI PlayStation8;
        public  readonly BanHammer ThorRagnarok;

        public static void LaunchInstance(CommandAndCallbackRouter command) => new Bot().Run(command);

        public static Bot Instance = null!;

        private Bot()
        {
            Instance = this;
            Config.SetBotUsername(Me.Username!);

            PlayStation8 = new ConsoleUI(this);
            ThorRagnarok = new BanHammer(this);
        }

        private void Run(CommandAndCallbackRouter command)
        {
            ThorRagnarok.GiveBans();

            ClearTempFiles();

            ChatsDealer.LoadSomeBakas();
            StartListening(command);
            ChatsDealer.StartAutoSaveLoop(minutes: 2);

            PlayStation8.EnterConsoleLoop();
        }

        private void StartListening(CommandAndCallbackRouter command)
        {
            var updates = new[] { UpdateType.Message, UpdateType.EditedMessage, UpdateType.CallbackQuery };
            var options = new ReceiverOptions { AllowedUpdates = updates };

            Client.StartReceiving(new TelegramUpdateHandler(command), options);
            Log(string.Format(BUENOS_DIAS, Config.BOT_USERNAME, Me.FirstName), ConsoleColor.Yellow);
        }

    }
}