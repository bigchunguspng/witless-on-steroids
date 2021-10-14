using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using static System.Environment;
using static Witlesss.Also.Extension;
using static Witlesss.Logger;
using static Witlesss.Also.Strings;
using File = System.IO.File;
using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, int>>;

namespace Witlesss
{
    public class Bot
    {
        private readonly Random _random = new Random();
        private readonly TelegramBotClient _client;
        private readonly ConcurrentDictionary<long, Witless> _sussyBakas;
        private readonly FileIO<ConcurrentDictionary<long, Witless>> _fileIO;
        private readonly Memes _memes;
        private long _activeChat;

        public Bot()
        {
            string token = File.ReadAllText($@"{CurrentDirectory}\.token");
            
            _memes = new Memes();
            _fileIO = new FileIO<ConcurrentDictionary<long, Witless>>($@"{CurrentDirectory}\{CHATLIST_FILENAME}.json");
            _sussyBakas = _fileIO.LoadData();
            _client = new TelegramBotClient(token);
        }

        public void Run()
        {
            ClearExtractedFrames();
            
            _client.StartReceiving();
            _client.OnMessage += OnMessageHandler;

            StartSaveLoop(2);
            ProcessConsoleInput();
        }

        private void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            string text = message.Caption ?? message.Text;
            long chat = message.Chat.Id;
            string title = TitleOrUsername();

            if (WitlessExist(chat))
            {
                var witless = _sussyBakas[chat];
                
                if (text != null && text.StartsWith('/'))
                {
                    if (TextAsCommand().StartsWith("/dg"))
                    {
                        ChatDemotivate();
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
                }
                
                if (witless.ReceiveSentence(ref text))
                    Log($@"""{title}"": получено сообщение ""{text}""", ConsoleColor.Blue);

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
                        SendMessage(chat, witless.TryToGenerate());
                        Log($@"""{title}"": сгенерировано прикол");
                    }
                }

                #region local memes

                void ChatSetFrequency()
                {
                    if (text.Split().Length > 1 && int.TryParse(text.Split()[1], out int value))
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
                            SendMessage(chat,
                                $"{FUSE_SUCCESS_RESPONSE_A} \"{title}\" {FUSE_SUCCESS_RESPONSE_B}\n{BASE_NEW_SIZE()}");
                        }
                        else
                            SendMessage(chat, passedID ? FUSE_FAIL_CHAT : FUSE_FAIL_BASE + FUSE_AVAILABLE_BASES(),
                                ParseMode.Html);

                        WitlessDB FromFile() =>
                            new FileIO<WitlessDB>($@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}\{name}.json")
                                .LoadData();
                    }
                    else
                        SendMessage(chat, FUSE_MANUAL, ParseMode.Html);
                    
                    string BASE_NEW_SIZE() => $"Теперь он весит {new FileInfo(witless.Path).Length / 1024} КБ";
                    string BASE_SIZE() => $"Словарь <b>этой беседы</b> весит {new FileInfo(witless.Path).Length / 1024} КБ";
                    string FUSE_AVAILABLE_BASES()
                    {
                        FileInfo[] files = new DirectoryInfo($@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}").GetFiles();
                        var result = "\n\nДоступные словари:";
                        foreach (var file in files)
                            result = result + $"\n<b>{file.Name.Replace(".json", "")}</b> ({file.Length / 1024} КБ)";
                        result = result + "\n\n" + BASE_SIZE();
                        return result;
                    }
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
                            SendAnimatedDemotivator(message.ReplyToMessage.Animation.FileId);
                        else
                            SendMessage(chat, DG_MANUAL);
                        return;
                    }

                    SendDemotivator(fileID);
                }

                void SendDemotivator(string fileID)
                {
                    GetDemotivatorSources(fileID, "jpg", out string a, out string b, out string path);
                    using (var stream = File.OpenRead(_memes.MakeDemotivator(path, a, b)))
                        SendPhoto(chat, new InputOnlineFile(stream));
                    Log($@"""{title}"": сгенерировано демотиватор [_]");
                }
                
                void SendAnimatedDemotivator(string fileID)
                {
                    GetDemotivatorSources(fileID, "mp4", out string a, out string b, out string path);
                    using (var stream = File.OpenRead(_memes.MakeAnimatedDemotivator(path, a, b)))
                        SendAnimation(chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
                    Log($@"""{title}"": сгенерировано GIF-демотиватор [^]");
                }
                
                void GetDemotivatorSources(string fileID, string extension, out string textA, out string textB, out string path)
                {
                    GetDemotivatorText(witless, text, out textA, out textB);
                    path = $@"{CurrentDirectory}\{PICTURES_FOLDER}\{chat}-{fileID.Remove(62)}.{extension}";
                    DownloadFile(fileID, path).Wait();
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
                        SendMessage(chat, $"{MOVE_DONE_AS} \"{result}\"\n\n{MOVE_DONE_CLEARED}");
                        Log($@"""{title}"": словарь сохранён как ""{result}""", ConsoleColor.Magenta);
                    }
                    else
                        SendMessage(chat, MOVE_MANUAL, ParseMode.Html);

                    string ExtraDBPath(string name) => $@"{CurrentDirectory}\{EXTRA_DBS_FOLDER}\{name}.json";
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

            string TitleOrUsername() => chat < 0 ? message.Chat.Title : message.From.FirstName;
            string TextAsCommand() => text.ToLower().Replace(BOT_USERNAME, "");
        }
        private async Task DownloadFile(string fileId, string path)
        {
            Directory.CreateDirectory($@"{CurrentDirectory}\{PICTURES_FOLDER}");
            try
            {
                var file = await _client.GetFileAsync(fileId);
                var stream = new FileStream(path, FileMode.Create);
                _client.DownloadFileAsync(file.FilePath, stream).Wait();
                await stream.DisposeAsync();
            }
            catch (Exception e)
            {
                Log(e.Message, ConsoleColor.Red);
            }
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
                        
                        if (input.StartsWith("/a ") ) //add
                        {
                            if (witless.ReceiveSentence(ref text))
                                Log($@"{_activeChat}: в словарь добавлено ""{text}""", ConsoleColor.Yellow);
                        }
                        else if (input.StartsWith("/w ")) //write
                        {
                            SendMessage(_activeChat, text);
                            bool accepted = witless.ReceiveSentence(ref text);
                            Log($@"{_activeChat}: отправлено {(accepted ? "и добавлено в словарь " : "")}""{text}""", ConsoleColor.Yellow);
                        }
                    }
                    else if (input == "/r") ClearExtractedFrames();
                    else if (input == "/s") SaveDics();
                    else if (input == "/u") ReloadDics();
                }
            } while (input != "s");
            _client.StopReceiving();
            SaveDics();
        }

        private async void SendMessage(long chat, string text, ParseMode mode = ParseMode.Default)
        {
            Task task = _client.SendTextMessageAsync(chat, text, mode, disableNotification: true);
            await TrySend(task, chat, "message");
        }
        private void SendPhoto(long chat, InputOnlineFile photo)
        {
            Task task = _client.SendPhotoAsync(chat, photo);
            TrySend(task, chat, "photo").Wait();
        }
        private void SendAnimation(long chat, InputOnlineFile animation)
        {
            Task task = _client.SendAnimationAsync(chat, animation);
            TrySend(task, chat, "GIF").Wait();
        }
        private async Task TrySend(Task task, long chat, string what)
        {
            try
            {
                await task.ContinueWith(action =>
                {
                    Log(chat + $": Can't send {what}: " + action.Exception?.Message, ConsoleColor.Red);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch
            {
                // 21
            }
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