using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.Enums;
using Witlesss.Commands;
using static System.Environment;
using static Witlesss.Extension;
using static Witlesss.Logger;
using static Witlesss.Strings;
using File = System.IO.File;
using ChatList = System.Collections.Concurrent.ConcurrentDictionary<long, Witlesss.Witless>;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, float>>;

namespace Witlesss
{
    public class Bot : BotCore
    {
        private long _activeChat;
        private readonly FileIO<ChatList> _fileIO;
        public readonly ChatList SussyBakas;
        public readonly Memes MemeService;

        public Bot()
        {
            MemeService = new Memes();
            _fileIO = new FileIO<ChatList>($@"{CurrentDirectory}\{DBS_FOLDER}\{CHATLIST_FILENAME}.json");
            SussyBakas = _fileIO.LoadData();
        }

        public void Run()
        {
            ClearTempFiles();

            Command.Bot = this;
            var options = new ReceiverOptions() {AllowedUpdates = new[] {UpdateType.Message, UpdateType.EditedMessage}};
            Client.StartReceiving(new Handler(), options);

            StartSaveLoop(2);
            ProcessConsoleInput();
        }

        private void ProcessConsoleInput()
        {
            string input;
            do
            {
                input = Console.ReadLine();
                
                if (input != null && !input.EndsWith("_"))
                {
                    if      (input.StartsWith("+")     && input.Length > 1) SetActiveChat();
                    else if (input.StartsWith("/"))
                    {
                        if  (WitlessExist(_activeChat) && input.Length > 3) BreakFourthWall();
                        else if (input == "/s") SaveDics();
                        else if (input == "/u") Spam();
                        else if (input == "/c") ClearTempFiles();
                        else if (input == "/k") ClearDics();
                        else if (input == "/e") DeleteBlockers();
                        else if (input == "/r") DeleteBySize();
                        else if (input == "/x") FixDBs();
                        else if (input == "/d") FuseAllDics();
                        else if (input.StartsWith("/u") && HasIntArgument(input, out int x1)) Spam(x1);
                        else if (input.StartsWith("/r") && HasIntArgument(input, out int x2)) DeleteBySize(x2);
                    }
                }
            } while (input != "s");
            SaveDics();

            void SetActiveChat()
            {
                string shit = input.Substring(1);
                foreach (long chat in SussyBakas.Keys)
                {
                    if (chat.ToString().EndsWith(shit))
                    {
                        _activeChat = chat;
                        Log($"ACTIVE CHAT >> {_activeChat}");
                        break;
                    }
                }
            }
            void BreakFourthWall()
            {
                string text = input.Substring(3).Trim();
                var witless = SussyBakas[_activeChat];
                        
                if      (input.StartsWith("/a ") && witless.Eat(text, out text)) //add
                {
                    Log($@"{_activeChat} >> ADDED TO DIC ""{text}""", ConsoleColor.Yellow);
                }
                else if (input.StartsWith("/w ")) //write
                {
                    SendMessage(_activeChat, text);
                    bool accepted = witless.Eat(text, out text);
                    Log($@"{_activeChat} >> SENT {(accepted ? "AND ADDED TO DIC " : "")}""{text}""", ConsoleColor.Yellow);
                }
            }
        }

        public bool WitlessExist(long chat) => SussyBakas.ContainsKey(chat);

        public void SaveChatList()
        {
            _fileIO.SaveData(SussyBakas);
            Log(LOG_CHATLIST_SAVED, ConsoleColor.Green);
        }

        private async void StartSaveLoop(int minutes)
        {
            while (true)
            {
                await Task.Delay(60000 * minutes);
                SaveDics();
            }
        }

        private void SaveDics()
        {
            foreach (var witless in SussyBakas.Values) witless.Save();
        }

        private void ClearDics()
        {
            foreach (var witless in SussyBakas.Values)
            {
                witless.Backup();
                File.Delete(witless.Path);
                witless.Load();
            }
        }

        private void Spam(int size = 2)
        {
            try
            {
                string message = File.ReadAllText($@"{CurrentDirectory}\.spam");
                foreach (var witless in SussyBakas.Values)
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
                throw;
            }
        }

        private void DeleteBlockers()
        {
            var bin = new List<long>();
            foreach (var witless in SussyBakas.Values)
            {
                int x = PingChat(witless.Chat);
                if (x == -1)
                {
                    witless.Backup();
                    File.Delete(witless.Path);
                    bin.Add(witless.Chat);
                }
                else
                {
                    Client.DeleteMessageAsync(witless.Chat, x);
                }
            }

            foreach (long chat in bin) SussyBakas.TryRemove(chat, out _);
            SaveChatList();
        }

        private void DeleteBySize(int size = 3)
        {
            var bin = new List<long>();
            foreach (var witless in SussyBakas.Values)
            {
                if (SizeInBytes(witless.Path) < size)
                {
                    witless.Backup();
                    File.Delete(witless.Path);
                    bin.Add(witless.Chat);
                }
            }
            
            foreach (long chat in bin) SussyBakas.TryRemove(chat, out _);
            SaveChatList();
        }

        private void FuseAllDics()
        {
            foreach (var witless in SussyBakas.Values)
            {
                var path = $@"{CurrentDirectory}\{COPIES_FOLDER}\{DB_FILE_PREFIX}-{witless.Chat}.json";
                if (File.Exists(path))
                {
                    witless.Backup();
                    new FusionCollab(witless.Words, new FileIO<WitlessDB>(path).LoadData()).Fuse();
                    Log($"{LOG_FUSION_DONE} << {witless.Chat}", ConsoleColor.Magenta);
                    witless.SaveNoMatterWhat();
                }
            }
        }
        
        private void FixDBs()
        {
            foreach (var witless in SussyBakas.Values)
            {
                NormalizeWitlessDB(witless.Words);
                witless.SaveNoMatterWhat();
            }
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