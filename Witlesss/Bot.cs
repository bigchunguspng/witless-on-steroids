using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace Witlesss
{
    public class Bot : BotCore
    {
        private readonly FileIO<ChatList> ChatsIO;
        public  readonly ChatList      SussyBakas;

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
            
            ChatsIO = new FileIO<ChatList>(Paths.File_Chats);
            SussyBakas = ChatsIO.LoadData();
        }

        private void Run(CommandAndCallbackRouter command)
        {
            ThorRagnarok.GiveBans();

            ClearTempFiles();

            LoadSomeBakas();
            StartListening(command);
            StartSaveLoopAsync(minutes: 2);

            PlayStation8.EnterConsoleLoop();
        }

        private void StartListening(CommandAndCallbackRouter command)
        {
            var updates = new[] { UpdateType.Message, UpdateType.EditedMessage, UpdateType.CallbackQuery };
            var options = new ReceiverOptions { AllowedUpdates = updates };

            Client.StartReceiving(new TelegramUpdateHandler(command), options);
            Log(string.Format(BUENOS_DIAS, Config.BOT_USERNAME, Me.FirstName), ConsoleColor.Yellow);
        }

        private void LoadSomeBakas()
        {
            var directory = new DirectoryInfo(Paths.Dir_Chat);
            var selection = directory
                .GetFiles(Paths.Prefix_Pack + "*.json")
                .Where(x => x.LastWriteTime.HappenedWithinLast(TimeSpan.FromHours(2)) && x.Length < 4_000_000)
                .Select(x => long.Parse(x.Name.Replace(Paths.Prefix_Pack + "-", "").Replace(".json", "")));
            foreach (var chat in selection) WitlessExist(chat); // <-- this loads the dictionary;
        }

        public bool WitlessExist(long chat)
        {
            var exist = SussyBakas.ContainsKey(chat);
            if (exist)  SussyBakas[chat].LoadUnlessLoaded();

            return exist;
        }

        public void SaveChatList()
        {
            ChatsIO.SaveData(SussyBakas);
            Log(LOG_CHATLIST_SAVED, ConsoleColor.Green);
        }

        private async void StartSaveLoopAsync(int minutes)
        {
            while (true)
            {
                await Task.Delay(60000 * minutes);
                SaveBakas();
            }
        }

        private void OkBuddies(Action<Witless> action)
        {
            lock (SussyBakas.Sync) SussyBakas.Values.ForEach(action);
        }

        public void SaveBakas () => OkBuddies(witless => witless.SaveAndCount());
        public void SaveDics  () => OkBuddies(witless => witless.Save());

        public void RemoveChat(long id) => SussyBakas.Remove(id);
    }
}