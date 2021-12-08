using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using static System.Environment;
using static Witlesss.Also.Extension;
using static Witlesss.Logger;
using static Witlesss.Also.Strings;
using File = System.IO.File;
using ChatList = System.Collections.Concurrent.ConcurrentDictionary<long, Witlesss.Witless>;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, int>>;

namespace Witlesss
{
    public class Bot : BotCore
    {
        private readonly Random _random = new Random();
        private readonly ChatList _sussyBakas;
        private readonly FileIO<ChatList> _fileIO;
        private readonly Memes _memes;
        private long _activeChat;

        public Bot()
        {
            _memes = new Memes();
            _fileIO = new FileIO<ChatList>($@"{CurrentDirectory}\{DBS_FOLDER}\{CHATLIST_FILENAME}.json");
            _sussyBakas = _fileIO.LoadData();
        }

        public void Run()
        {
            ClearTempFiles();

            var options = new ReceiverOptions() {AllowedUpdates = new[] {UpdateType.Message, UpdateType.EditedMessage}};
            Client.StartReceiving(new Handler(this), options);
            Log("стартуем!");

            StartSaveLoop(2);
            ProcessConsoleInput();
        }
        
        public void TryHandleMessage(Message message)
        {
            try
            {
                HandleMessage(message);
            }
            catch (Exception exception)
            {
                Log(message.Chat.Id + ": Can't handle message: " + exception.Message, ConsoleColor.Red);
            }
        }

        private void HandleMessage(Message message)
        {
            string text = message.Caption ?? message.Text;
            long chat = message.Chat.Id;
            string title = TitleOrUsername();

            if (WitlessExist(chat))
            {
                var witless = _sussyBakas[chat];
                
                if (text != null)
                {
                    if (text.StartsWith('/'))
                    {
                        if (TextAsCommand().StartsWith("/dg"))
                        {
                            ChatDemotivate();
                            return;
                        }
                        if (TextAsCommand().StartsWith("/a"))
                        {
                            ChatGenerateByFirstWord();
                            return;
                        }
                        if (TextAsCommand().StartsWith("/damn"))
                        {
                            ChatRemoveBitrate();
                            return;
                        }
                        if (TextAsCommand().StartsWith("/b"))
                        {
                            ChatBuhurt();
                            return;
                        }
                        if (TextAsCommand().StartsWith("/set_frequency"))
                        {
                            ChatSetFrequency();
                            return;
                        }
                        if (TextAsCommand() == "/chat_id")
                        {
                            SendMessage(chat, chat.ToString());
                            return;
                        }
                        if (TextAsCommand().StartsWith("/fuse"))
                        {
                            ChatFuse();
                            return;
                        }
                        if (TextAsCommand().StartsWith("/move"))
                        {
                            ChatMove();
                            return;
                        }
                        if (TextAsCommand() == "/debug")
                        {
                            ChatDebugMessage();
                            return;
                        }
                    }
                    else
                    {
                        var sentence = text.Clone().ToString();
                        if (witless.ReceiveSentence(ref sentence))
                            Log($@"""{title}"": получено сообщение ""{sentence}""", ConsoleColor.Blue);
                    }
                }
                
                witless.Count();
                
                if (witless.ReadyToGen())
                {
                    if (message.Photo != null && _random.Next(witless.Interval) == 0)
                    {
                        string fileID = message.Photo[^1].FileId;
                        SendDemotivator(fileID);
                    }
                    else
                    {
                        Thread.Sleep(AssumedResponseTime(150, text));
                        SendMessage(chat, witless.TryToGenerate());
                        Log($@"""{title}"": сгенерировано прикол");
                    }
                }

                #region local memes

                void ChatSetFrequency()
                {
                    if (HasIntArgument(text, out int value))
                    {
                        witless.Interval = value;
                        SaveChatList();
                        SendMessage(chat, SET_FREQUENCY_RESPONSE(witless.Interval));
                        Log($@"""{title}"": интервал генерации изменен на {witless.Interval}");
                    }
                    else
                        SendMessage(chat, SET_FREQUENCY_MANUAL);
                }

                void ChatFuse()
                {
                    string[] a = text.Split();
                    if (a.Length > 1)
                    {
                        string name = a[1];
                        bool passedID = long.TryParse(name, out long key);
                        bool thisChatID = key == chat;
                        if (thisChatID)
                        {
                            SendMessage(chat, FUSE_FAIL_SELF);
                            return;
                        }

                        bool chatExist = passedID && WitlessExist(key);
                        bool baseExist = BaseExists(name);
                        if (chatExist || baseExist)
                        {
                            witless.Backup();
                            var fusion = new FusionCollab(witless.Words, chatExist ? _sussyBakas[key].Words : FromFile());
                            fusion.Fuse();
                            witless.HasUnsavedStuff = true;
                            witless.Save();
                            SendMessage(chat, $"{FUSE_SUCCESS_RESPONSE_A} \"{title}\" {FUSE_SUCCESS_RESPONSE_B}\n{BASE_NEW_SIZE()}");
                        }
                        else
                            SendMessage(chat, passedID ? FUSE_FAIL_CHAT : FUSE_FAIL_BASE + FUSE_AVAILABLE_BASES());

                        WitlessDB FromFile() => new FileIO<WitlessDB>($@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}\{name}.json").LoadData();
                    }
                    else
                        SendMessage(chat, FUSE_MANUAL);
                    
                    string BASE_NEW_SIZE() => $"Теперь он весит {FileSize(witless.Path)}";
                    string BASE_SIZE() => $"Словарь <b>этой беседы</b> весит {FileSize(witless.Path)}";
                    string FUSE_AVAILABLE_BASES()
                    {
                        FileInfo[] files = new DirectoryInfo($@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}").GetFiles();
                        var result = "\n\nДоступные словари:";
                        foreach (var file in files)
                            result = result + $"\n<b>{file.Name.Replace(".json", "")}</b> ({FileSize(file.FullName)})";
                        result = result + "\n\n" + BASE_SIZE();
                        return result;
                    }
                }

                void ChatGenerateByFirstWord()
                {
                    if (text.Contains(' '))
                    {
                        string word = text.Split()[^1];
                        text = text.Substring(text.IndexOf(' ') + 1);
                        text = text.Remove(text.Length - word.Length) + witless.TryToGenerateFromWord(word.ToLower());
                        SendMessage(chat, TextInRandomLetterCase(text));
                        Log($@"""{title}"": сгенерировано прикол по слову");
                    }
                    else
                        SendMessage(chat, A_MANUAL);
                }

                void ChatBuhurt()
                {
                    var length = 3;
                    if (HasIntArgument(text, out int value))
                        length = Math.Clamp(value, 2, 13);
                    var lines = new string[length];
                    for (var i = 0; i < length; i++)
                        lines[i] = witless.TryToGenerate().ToUpper();
                    string result = string.Join("\n@\n", lines.Distinct());
                    SendMessage(chat, result);
                }

                void ChatDemotivate()
                {
                    string fileID;
                    if (message.Photo != null)
                        fileID = message.Photo[^1].FileId;
                    else if (message.ReplyToMessage?.Photo != null)
                        fileID = message.ReplyToMessage.Photo[^1].FileId;
                    else
                    {
                        if (message.ReplyToMessage?.Animation != null)
                            fileID = message.ReplyToMessage.Animation.FileId;
                        else if (message.Animation != null)
                            fileID = message.Animation.FileId;
                        else if (message.ReplyToMessage?.Video != null)
                            fileID = message.ReplyToMessage.Video.FileId;
                        else if (message.Video != null)
                            fileID = message.Video.FileId;
                        else
                        {
                            if (message.ReplyToMessage?.Sticker != null && message.ReplyToMessage.Sticker.IsAnimated == false)
                                fileID = message.ReplyToMessage.Sticker.FileId;
                            else
                            {
                                SendMessage(chat, DG_MANUAL);
                                return;
                            }
                            SendDemotivatedSticker(fileID);
                            return;
                        }
                        SendAnimatedDemotivator(fileID);
                        return;
                    }
                    SendDemotivator(fileID);
                }

                void SendDemotivator(string fileID)
                {
                    GetDemotivatorSources(fileID, ".jpg", out string a, out string b, out string path);
                    using (var stream = File.OpenRead(_memes.MakeDemotivator(path, a, b)))
                        SendPhoto(chat, new InputOnlineFile(stream));
                    Log($@"""{title}"": сгенерировано демотиватор [_]");
                }
                
                void SendAnimatedDemotivator(string fileID)
                {
                    var time = DateTime.Now;
                    GetDemotivatorSources(fileID, ".mp4", out string a, out string b, out string path);
                    using (var stream = File.OpenRead(_memes.MakeAnimatedDemotivator(path, a, b)))
                        SendAnimation(chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
                    Log($@"""{title}"": сгенерировано GIF-демотиватор [^] за {DateTime.Now - time:s\.fff}");
                }
                
                void SendDemotivatedSticker(string fileID)
                {
                    GetDemotivatorSources(fileID, ".webp", out string a, out string b, out string path);
                    string extension = text.Contains("-j") ? ".jpg" : ".png";
                    using (var stream = File.OpenRead(_memes.MakeStickerDemotivator(path, a, b, extension)))
                        SendPhoto(chat, new InputOnlineFile(stream));
                    Log($@"""{title}"": сгенерировано демотиватор [#] из стикера");
                }
                
                void GetDemotivatorSources(string fileID, string extension, out string textA, out string textB, out string path)
                {
                    GetDemotivatorText(witless, text, out textA, out textB);
                    path = $@"{CurrentDirectory}\{PICTURES_FOLDER}\{ShortID(fileID)}{extension}";
                    path = UniquePath(path, extension);
                    DownloadFile(fileID, path).Wait();
                }

                void ChatRemoveBitrate()
                {
                    var fileID = "";
                    Message mess = message.ReplyToMessage ?? message;
                    for (int cycle = message.ReplyToMessage != null ? 0 : 1; cycle < 2; cycle++)
                    {
                        if (mess.Animation != null)
                            fileID = mess.Animation.FileId;
                        else if (mess.Video != null)
                            fileID = mess.Video.FileId;
                        else if (mess.Audio != null)
                            fileID = mess.Audio.FileId;
                        else if (mess.Document?.MimeType != null && mess.Document.MimeType.StartsWith("audio"))
                            fileID = mess.Document.FileId;
                        else if (mess.Voice != null)
                            fileID = mess.Voice.FileId;
                        if (fileID.Length > 0)
                            break;
                        else if (cycle == 1)
                        {
                            SendMessage(chat, DAMN_MANUAL);
                            return;
                        }
                        else mess = message;
                    }

                    var bitrate = 0;
                    if (HasIntArgument(text, out int value))
                        bitrate = value;

                    string shortID = ShortID(fileID);
                    string extension = ExtensionFromID(shortID);
                    var path = $@"{CurrentDirectory}\{PICTURES_FOLDER}\{shortID}{extension}";
                    path = UniquePath(path, extension);
                    DownloadFile(fileID, path, chat).Wait();

                    string result = _memes.RemoveBitrate(path, bitrate);
                    extension = GetFileExtension(result);
                    using (var stream = File.OpenRead(result))
                        switch (extension)
                        {
                            case ".mp4":
                                if (shortID.StartsWith("BA")) 
                                    SendVideo(chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
                                else
                                    SendAnimation(chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
                                break;
                            case ".mp3":
                                SendAudio(chat, new InputOnlineFile(stream, $"Damn, {ValidFileName(SenderName())}.mp3"));
                                break;
                        }
                    Log($@"""{title}"": что-то сжато [*]");
                }

                void ChatMove()
                {
                    string[] a = text.Split();
                    if (a.Length > 1)
                    {
                        string name = a[1];
                        string path = BaseExists(name) ? UniquePath(ExtraDBPath(name), ".json") : ExtraDBPath(name);
                        witless.Save();
                        File.Copy(witless.Path, path);

                        witless.Words.Clear();
                        Log($@"""{title}"": словарь беседы очищен!", ConsoleColor.Magenta);
                        witless.HasUnsavedStuff = true;
                        witless.Save();

                        string result = path.Substring(path.LastIndexOf('\\') + 1).Replace(".json", "");
                        SendMessage(chat, $"{MOVE_DONE_CLEARED}\n\n{MOVE_DONE_AS} <b>\"{result}\"</b>");
                        Log($@"""{title}"": словарь сохранён как ""{result}""", ConsoleColor.Magenta);
                    }
                    else
                        SendMessage(chat, MOVE_MANUAL);

                    string ExtraDBPath(string name) => $@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}\{name}.json";
                }

                void ChatDebugMessage()
                {
                    if (message.ReplyToMessage == null)
                        return;
                    
                    Message mess = message.ReplyToMessage;
                    var name = $"Message-{mess.MessageId}-{mess.Chat.Id}.json";
                    var path = $@"{CurrentDirectory}\{DEBUG_FOLDER}\{name}";
                    CreatePath(path);
                    new FileIO<Message>(path).SaveData(mess);
                    using var stream = File.OpenRead(path);
                    SendDocument(chat, new InputOnlineFile(stream, name.Replace("--", "-")));
                }

                #endregion
            }
            else if (text != null && TextAsCommand() == "/start")
            {
                ChatStart();
            }

            void ChatStart()
            {
                if (!_sussyBakas.TryAdd(chat, new Witless(chat)))
                    return;
                SaveChatList();
                Log($@"Создано базу для чата {chat} ({title})");
                SendMessage(chat, START_RESPONSE);
            }

            string TitleOrUsername() => chat < 0 ? message.Chat.Title : message.From?.FirstName;
            string SenderName() => message.SenderChat?.Title ?? message.From?.FirstName;
            string TextAsCommand() => text.ToLower().Replace(BOT_USERNAME, "");
        }

        private void ProcessConsoleInput()
        {
            string input;
            do
            {
                input = Console.ReadLine();
                
                if (input != null)
                {
                    if (input.StartsWith("+") && input.Length > 1)
                    {
                        string shit = input.Substring(1);
                        foreach (long chat in _sussyBakas.Keys)
                        {
                            if (chat.ToString().EndsWith(shit))
                            {
                                _activeChat = chat;
                                Log($"Выбрано чат {_activeChat}");
                                break;
                            }
                        }
                    }
                    else if (WitlessExist(_activeChat) && input.Length > 3)
                    {
                        string text = input.Substring(3).Trim();
                        var witless = _sussyBakas[_activeChat];
                        
                        if (input.StartsWith("/a ") && witless.ReceiveSentence(ref text)) //add
                        {
                            Log($@"{_activeChat}: в словарь добавлено ""{text}""", ConsoleColor.Yellow);
                        }
                        else if (input.StartsWith("/w ")) //write
                        {
                            SendMessage(_activeChat, text);
                            bool accepted = witless.ReceiveSentence(ref text);
                            Log($@"{_activeChat}: отправлено {(accepted ? "и добавлено в словарь " : "")}""{text}""", ConsoleColor.Yellow);
                        }
                    }
                    else if (input == "/s") SaveDics();
                    else if (input == "/u") ReloadDics();
                    else if (input == "/r") ClearTempFiles();
                    else if (input == "/f") FuseAllDics();
                }
            } while (input != "s");
            SaveDics();
        }

        private bool WitlessExist(long chat) => _sussyBakas.ContainsKey(chat);
        private bool BaseExists(string name)
        {
            var path = $@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}";
            Directory.CreateDirectory(path);
            return Directory.GetFiles(path).Contains($@"{path}\{name}.json");
        }
        
        private void SaveChatList()
        {
            _fileIO.SaveData(_sussyBakas);
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
        
        private void FuseAllDics()
        {
            foreach (var witless in _sussyBakas.Values)
            {
                var path = $@"{CurrentDirectory}\A\{DB_FILE_PREFIX}-{witless.Chat}.json";
                if (File.Exists(path))
                {
                    witless.Backup();
                    var fusion = new FusionCollab(witless.Words, new FileIO<WitlessDB>(path).LoadData());
                    fusion.Fuse();
                    witless.HasUnsavedStuff = true;
                    witless.Save();
                }
            }
        }

        private void SaveDics()
        {
            foreach (var witless in _sussyBakas.Values) witless.Save();
        }
        
        private void ReloadDics()
        {
            foreach (var witless in _sussyBakas.Values) witless.Load();
        }
    }
}