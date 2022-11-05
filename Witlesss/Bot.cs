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
using File = System.IO.File;

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
            _fileIO = new FileIO<ChatList>($@"{DBS_FOLDER}\{CHATLIST_FILENAME}.json");
            SussyBakas = _fileIO.LoadData();
        }

        public void Run()
        {
            ClearTempFiles();

            Command.Bot = this;
            var options = new ReceiverOptions() {AllowedUpdates = new[] {UpdateType.Message, UpdateType.EditedMessage}};
            Client.StartReceiving(new Handler(), options);

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
                        else if (input.StartsWith("/"))
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
                            else if (input.StartsWith("/sp") && HasIntArgument(input, out int x1)) Spam(x1);
                            else if (input.StartsWith("/ds") && HasIntArgument(input, out int x2)) DeleteBySize(x2);
                        }
                    }
                }
                catch
                {
                    Log(">:^< u/stupid >:^<", ConsoleColor.Yellow);
                }
            } while (input != "s");
            SaveDics();

            void SetActiveChat()
            {
                string shit = input[1..];
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
                string text = input.Split(' ', 2)[1];
                var witless = Active;

                if      (input.StartsWith("/a ") && witless.Eat(text, out text)) // add
                {
                    Log($@"{_activeChat} >> XD << ""{text}""", ConsoleColor.Yellow);
                }
                else if (input.StartsWith("/w "))                                // write
                {
                    SendMessage(_activeChat, text);
                    witless.Eat(text, out _);
                    Log($@"{_activeChat} >> {text}", ConsoleColor.Yellow);
                }
            }
        }

        private Witless Active => SussyBakas[_activeChat];
        private ICollection<Witless> Bakas => SussyBakas.Values;

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
            foreach (var w in Bakas) if (DeleteBlocker(w) == -1) SussyBakas.TryRemove(w.Chat, out _);
            SaveChatList();
        }
        private void DeleteBlocker()
        {
            if (DeleteBlocker(Active) == -1)
            {
                SussyBakas.TryRemove(_activeChat, out _);
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
                    SussyBakas.TryRemove(witless.Chat, out _);
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