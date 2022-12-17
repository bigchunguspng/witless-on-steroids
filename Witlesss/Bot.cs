using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MediaToolkit.Util;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Witlesss.Commands;
using BanList = System.Collections.Generic.Dictionary<long, System.DateTime>;

namespace Witlesss
{
    public class Bot : BotCore
    {
        private long _active;
        private readonly FileIO<ChatList> _chatsIO;
        private readonly FileIO<BanList> _bansIO;
        public readonly ChatList SussyBakas;
        private readonly BanList BannedChats;
        public readonly MainJunction Fork = new();
        public readonly Memes MemeService = new();

        public Bot()
        {
            _bansIO = new FileIO<BanList>($@"{DBS_FOLDER}\bans.json");
            BannedChats = _bansIO.LoadData();
            _chatsIO = new FileIO<ChatList>($@"{DBS_FOLDER}\{CHATLIST_FILENAME}.json");
            SussyBakas = _chatsIO.LoadData();
            foreach (var chat in BannedChats.Keys) SussyBakas[chat].Banned = true;
        }

        public void Run()
        {
            ClearTempFiles();

            Command.Bot = this;
            var options = new ReceiverOptions() {AllowedUpdates = new[] {UpdateType.Message, UpdateType.EditedMessage}};
            Client.StartReceiving(new Handler(this), options);

            StartSaveLoop(2);
            ProcessConsole();
        }

        private void ProcessConsole()
        {
            string input;
            do
            {
                input = Console.ReadLine();
                try
                {
                    if (input != null && !input.EndsWith("_"))
                    {
                        if      (input.StartsWith("+") && input.Length > 1) SetActiveChat();
                        else if (input.StartsWith("/")) DoConsoleCommands();
                    }
                }
                catch
                {
                    Log(">:^< u/stupid >:^<", ConsoleColor.Yellow);
                }
            } while (input != "s");
            SaveDics();

            void DoConsoleCommands()
            {
                if (Regex.IsMatch(input, @"^\/[aw] ")) BreakFourthWall();
                else if (input == "/"  ) Log(CONSOLE_MANUAL, ConsoleColor.Yellow);
                else if (input == "/s" ) SaveDics();
                else if (input == "/sd") SyncDics();
                else if (input == "/sp") Spam();
                else if (input == "/db") DeleteBlockers();
                else if (input == "/DB") DeleteBlocker();
                else if (input == "/ds") DeleteBySize();
                else if (input == "/cc") ClearTempFiles();
                else if (input == "/oo") ClearDics();
                else if (input == "/Oo") ClearDic(Active);
                else if (input == "/xx") FixDBs();
                else if (input == "/Xx") FixDB(Active);
                else if (input == "/l" ) ActivateLastChat();
                else if (input == "/b" ) BanChat(Active.Chat);
                else if (input == "/ub") UnbanChat(Active.Chat);
                else if (input.StartsWith("/sp") && HasIntArgument(input, out int x1)) Spam(x1);
                else if (input.StartsWith("/ds") && HasIntArgument(input, out int x2)) DeleteBySize(x2);
                else if (input.StartsWith("/b" ) && HasIntArgument(input, out int x3)) BanChat(Active.Chat, x3);
            }
            void SetActiveChat()
            {
                string shit = input[1..];
                foreach (long chat in SussyBakas.Keys)
                {
                    if (chat.ToString().EndsWith(shit))
                    {
                        _active = chat;
                        Log($"ACTIVE CHAT >> {_active}");
                        break;
                    }
                }
            }
            void BreakFourthWall()
            {
                string text = input.Split(' ', 2)[1];
                var witless = Active;

                if      (input.StartsWith("/a ") && witless.Eat(text, out text)) // add
                {
                    Log($@"{_active} >> XD << ""{text}""", ConsoleColor.Yellow);
                }
                else if (input.StartsWith("/w "))                                // write
                {
                    SendMessage(_active, text);
                    witless.Eat(text, out _);
                    Log($@"{_active} >> {text}", ConsoleColor.Yellow);
                }
            }
        }

        private Witless Active => SussyBakas[_active];
        private ICollection<Witless> Bakas => SussyBakas.Values;
        private void RemoveChat(long id) => SussyBakas.TryRemove(id, out _);

        private void ActivateLastChat()
        {
            _active = Fork.LastChat;
            Log($"ACTIVE CHAT >> {_active} ({Fork.LastChatTitle})");
        }

        public bool WitlessExist(long chat) => SussyBakas.ContainsKey(chat);

        public void SaveChatList()
        {
            _chatsIO.SaveData(SussyBakas);
            Log(LOG_CHATLIST_SAVED, ConsoleColor.Green);
        }

        public void PullBanStatus(long chat) => SussyBakas[chat].Banned = BannedChats.ContainsKey(chat);
        public bool ChatIsBanned(Witless witless)
        {
            var banned = witless.Banned;
            if (banned && BanIsOver(witless.Chat)) return false;
            
            return banned;
        }
        public bool ChatIsBanned(long chat)
        {
            var banned = BannedChats.ContainsKey(chat);
            if (banned && BanIsOver(chat)) return false;

            return banned;
        }

        private bool BanIsOver(long chat)
        {
            var date = BannedChats[chat];
            var o = DateTime.Now > date;
            if (o) UnbanChat(chat);
            else SendMessage(chat, $"🤓 BLOCKED TIL {date.Date:dd.MM.yyyy}");
            return o;
        }
        private void BanChat(long chat, int days = 1)
        {
            BannedChats.TryAdd(chat, DateTime.Now + TimeSpan.FromDays(days));
            SussyBakas[chat].Banned = true;
            SaveBanList();
            Log($"{chat} >> BANNED", ConsoleColor.Magenta);
        }
        private void UnbanChat(long chat)
        {
            BannedChats.Remove(chat);
            SussyBakas[chat].Banned = false;
            SaveBanList();
            Log($"{chat} >> UNBANNED", ConsoleColor.Magenta);
        }
        private void SaveBanList() => _bansIO.SaveData(BannedChats);

        private async void StartSaveLoop(int minutes)
        {
            while (true)
            {
                await Task.Delay(60000 * minutes);
                SaveDics();
            }
        }

        private void SaveDics () => Bakas.ForEach(witless => witless.Save());
        private void ClearDics() => Bakas.ForEach(ClearDic);

        private void ClearDic(Witless witless)
        {
            witless.Delete();
            witless.Load();
        }

        private void Spam(int size = 2)
        {
            try
            {
                string message = File.ReadAllText(".spam");
                foreach (var witless in Bakas)
                {
                    if (SizeInBytes(witless.Path) > size)
                    {
                        SendMessage(witless.Chat, message);
                        Log($"MAIL SENT << {witless.Chat}", ConsoleColor.Yellow);
                    }
                }
            }
            catch (Exception e)
            {
                LogError("SPAM FAILED :( " + e.Message);
            }
        }

        private void DeleteBlockers()
        {
            foreach (var w in Bakas) if (DeleteBlocker(w) == -1) RemoveChat(w.Chat);
            SaveChatList();
        }
        private void DeleteBlocker()
        {
            if (DeleteBlocker(Active) == -1)
            {
                RemoveChat(_active);
                SaveChatList();
            }
        }
        
        private int DeleteBlocker(Witless witless)
        {
            int x = PingChat(witless.Chat);
            if (x == -1) witless.Delete();
            else Client.DeleteMessageAsync(witless.Chat, x);

            return x;
        }

        private void DeleteBySize(int size = 3)
        {
            foreach (var witless in Bakas)
            {
                if (SizeInBytes(witless.Path) < size)
                {
                    witless.Delete();
                    RemoveChat(witless.Chat);
                }
            }
            SaveChatList();
        }

        private void SyncDics()
        {
            foreach (var witless in Bakas)
            {
                var path = $@"{COPIES_FOLDER}\{DB_FILE_PREFIX}-{witless.Chat}.json";
                if (File.Exists(path))
                {
                    new FusionCollab(witless, new FileIO<WitlessDB>(path).LoadData()).Fuse();
                    Log($"{LOG_FUSION_DONE} << {witless.Chat}", ConsoleColor.Magenta);
                    witless.SaveNoMatterWhat();
                }
            }
        }
        
        private void FixDBs() => Bakas.ForEach(FixDB);
        private void FixDB(Witless witless)
        {
            NormalizeWitlessDB(witless.Words);
            witless.SaveNoMatterWhat();
        }

        private void NormalizeWitlessDB(WitlessDB words)
        {
            foreach (var word in words)
            foreach (string next in word.Value.Keys)
            {
                if (!words.ContainsKey(next))
                {
                    words.TryAdd(next, new ConcurrentDictionary<string, float>());
                    words[next].TryAdd(Witless.END, 1F);
                }
            }
        }
    }
}