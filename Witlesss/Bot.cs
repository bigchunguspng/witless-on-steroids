using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaToolkit.Util;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Witlesss.Commands;

namespace Witlesss
{
    public class Bot : BotCore
    {
        private readonly FileIO<ChatList> ChatsIO;
        public  readonly ChatList      SussyBakas;

        public  readonly MainJunction Fork = new();
        public  readonly Memes MemeService = new();

        private readonly ConsoleUI PlayStation8;
        public  readonly BanHammer ThorRagnarok;

        public Bot()
        {
            Command.Bot = this;

            PlayStation8 = new ConsoleUI(this);
            ThorRagnarok = new BanHammer(this);
            
            ChatsIO = new FileIO<ChatList>($@"{DBS_FOLDER}\{CHATLIST_FILENAME}.json");
            SussyBakas = ChatsIO.LoadData();
        }

        public void Run()
        {
            ThorRagnarok.GiveBans();

            ClearTempFiles();

            LoadSomeBakas();
            StartListening();
            StartSaveLoopAsync(minutes: 2);

            PlayStation8.EnterConsoleLoop();
        }

        private void StartListening()
        {
            var updates = new[] { UpdateType.Message, UpdateType.EditedMessage };
            var options = new ReceiverOptions { AllowedUpdates = updates };

            Client.StartReceiving(new Handler(this), options);
        }

        private void LoadSomeBakas()
        {
            var directory = new DirectoryInfo(DBS_FOLDER);
            var selection = directory
                .GetFiles(DB_FILE_PREFIX + "*.json")
                .Where(x => DateTime.Now - x.LastWriteTime < TimeSpan.FromHours(2) && x.Length < 4_000_000)
                .Select(x => long.Parse(x.Name.Replace(DB_FILE_PREFIX + "-", "").Replace(".json", "")));
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