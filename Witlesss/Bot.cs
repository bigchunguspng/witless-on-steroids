using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Witlesss.Commands;
using static System.Environment;
using static Witlesss.Extension;
using static Witlesss.Logger;
using static Witlesss.Also.Strings;
using File = System.IO.File;
using ChatList = System.Collections.Concurrent.ConcurrentDictionary<long, Witlesss.Witless>;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, int>>;

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
        
        public string GetVideoOrAudioID(Message message, long chat)
        {
            var fileID = "";
            var mess = message.ReplyToMessage ?? message;
            for (int cycle = message.ReplyToMessage != null ? 0 : 1; cycle < 2; cycle++)
            {
                if      (mess.Audio != null)
                    fileID = mess.Audio.FileId;
                else if (mess.Video != null)
                    fileID = mess.Video.FileId;
                else if (mess.Animation != null)
                    fileID = mess.Animation.FileId;
                else if (mess.Sticker != null && mess.Sticker.IsVideo)
                    fileID = mess.Sticker.FileId;
                else if (mess.Voice != null)
                    fileID = mess.Voice.FileId;
                else if (mess.Document?.MimeType != null && mess.Document.MimeType.StartsWith("audio"))
                    fileID = mess.Document.FileId;
                
                if (fileID.Length > 0)
                    break;
                else if (cycle == 1)
                {
                    SendMessage(chat, DAMN_MANUAL);
                    return null;
                }
                else mess = message;
            }
            return fileID;
        }

        private void ProcessConsoleInput()
        {
            string input;
            do
            {
                input = Console.ReadLine();
                
                if (input != null && !input.EndsWith("_"))
                {
                    if (input.StartsWith("+") && input.Length > 1)
                    {
                        string shit = input.Substring(1);
                        foreach (long chat in SussyBakas.Keys)
                        {
                            if (chat.ToString().EndsWith(shit))
                            {
                                _activeChat = chat;
                                Log($"{_activeChat} >> ACTIVE CHAT");
                                break;
                            }
                        }
                    }
                    else if (WitlessExist(_activeChat) && input.Length > 3)
                    {
                        string text = input.Substring(3).Trim();
                        var witless = SussyBakas[_activeChat];
                        
                        if (input.StartsWith("/a ") && witless.ReceiveSentence(ref text)) //add
                        {
                            Log($@"{_activeChat} >> ADDED TO DIC ""{text}""", ConsoleColor.Yellow);
                        }
                        else if (input.StartsWith("/w ")) //write
                        {
                            SendMessage(_activeChat, text);
                            bool accepted = witless.ReceiveSentence(ref text);
                            Log($@"{_activeChat} >> SENT {(accepted ? "AND ADDED TO DIC " : "")}""{text}""", ConsoleColor.Yellow);
                        }
                    }
                    else if (input == "/s") SaveDics();
                    else if (input == "/u") Spam();
                    else if (input == "/c") ClearTempFiles();
                    else if (input == "/k") ClearDics();
                    else if (input == "/e") DeleteBlockers();
                    else if (input == "/r") DeleteBySize();
                    else if (input.StartsWith("/r") && input.Contains(" ") && HasIntArgument(input, out int value)) DeleteBySize(value);
                }
            } while (input != "s");
            SaveDics();
        }

        public bool WitlessExist(long chat) => SussyBakas.ContainsKey(chat);

        public bool BaseExists(string name)
        {
            var path = $@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}";
            Directory.CreateDirectory(path);
            return Directory.GetFiles(path).Contains($@"{path}\{name}.json");
        }

        public void SaveChatList()
        {
            _fileIO.SaveData(SussyBakas);
            Log(LOG_CHATLIST_SAVED, ConsoleColor.Green);
        }

        private async void StartSaveLoop(int minutes)
        {
            var saving = new Counter(minutes);
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(60000);
                    saving.Count();
                    if (saving.Ready())
                    {
                        SaveDics();
                    }
                }
            });
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

        private void Spam()
        {
            try
            {
                string message = File.ReadAllText($@"{CurrentDirectory}\.spam");
                foreach (var witless in SussyBakas.Values)
                {
                    SendMessage(witless.Chat, message);
                    Log($"MAIL SENT << {witless.Chat}", ConsoleColor.Yellow);
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
                long bytes = new FileInfo(witless.Path).Length;
                if (bytes < size)
                {
                    witless.Backup();
                    File.Delete(witless.Path);
                    bin.Add(witless.Chat);
                }
            }
            
            foreach (long chat in bin) SussyBakas.TryRemove(chat, out _);
            SaveChatList();
        }
    }
}