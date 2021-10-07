using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.Logger;
using File = System.IO.File;

namespace Witlesss
{
    class Program
    {
        private static string _token;
        private static TelegramBotClient _client;
        private static ConcurrentDictionary<long, Witless> _sussyBakas;
        private static FileIO<ConcurrentDictionary<long, Witless>> _fileIO;
        private static long _activeChat;
        private static Counter _saving;
        private static Memes _memes;

        static void Main(string[] args)
        {
            _token = File.ReadAllText($@"{Environment.CurrentDirectory}\.token");
            
            _fileIO = new FileIO<ConcurrentDictionary<long, Witless>>($@"{Environment.CurrentDirectory}\Telegram-ChatsDB.json");
            _sussyBakas = _fileIO.LoadData();
            _saving = new Counter(2);
            _memes = new Memes();

            _client = new TelegramBotClient(_token);
            _client.StartReceiving();
            _client.OnMessage += OnMessageHandler;

            SaveLoop();
            HandleConsoleInput();
        }

        private static void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            string text = message.Caption ?? message.Text;
            long chat = message.Chat.Id;
            string title = TitleOrUsername(chat, message);

            if (WitlessExist(chat))
            {
                var witless = _sussyBakas[chat];
                
                if (text != null && text.StartsWith('/'))
                {
                    if (CommandFrom(text).StartsWith("/set_frequency"))
                    {
                        ChatSetFrequency(chat, title, witless, text);
                        return;
                    }
                    if (CommandFrom(text).StartsWith("/dg"))
                    {
                        ChatDemotivate(chat, title, witless, message, text);
                        return;
                    }
                    if (CommandFrom(text) == "/chat_id")
                    {
                        SendMessage(chat, chat.ToString());
                        return;
                    }
                    if (CommandFrom(text).StartsWith("/fuse"))
                    {
                        ChatFuse(chat, title, witless, text);
                        return;
                    }
                }
                
                if (witless.ReceiveSentence(ref text))
                    Log($@"""{title}"": получено сообщение ""{text}""", ConsoleColor.Blue);

                witless.Count();
                
                if (witless.ReadyToGen())
                {
                    SendMessage(chat, witless.TryToGenerate());
                    Log($@"""{title}"": сгенерировано прикол");
                }
            }
            else if (text != null && CommandFrom(text) == "/start")
            {
                ChatStart(chat, title);
            }
        }
        private static string TitleOrUsername(long chat, Message message) => chat < 0 ? message.Chat.Title : message.From.FirstName;
        private static string CommandFrom(string text) => text.ToLower().Replace("@piece_fap_bot", "");

        private static void ChatStart(long chat, string title)
        {
            if (!_sussyBakas.TryAdd(chat, new Witless(chat))) 
                return;
            SaveChatList();
            Log($@"Создано базу для чата {chat} ({title})");
            SendMessage(chat, "ВИРУСНАЯ БАЗА ОБНОВЛЕНА!");
        }
        private static void ChatSetFrequency(long chat, string title, Witless witless, string text)
        {
            if (text.Split().Length > 1 && int.TryParse(text.Split()[1], out int value))
            {
                witless.Interval = value;
                SaveChatList();
                SendMessage(chat, $"я буду писать сюда каждые {witless.Interval} кг сообщений");
                Log($@"""{title}"": интервал генерации изменен на {witless.Interval}");
            }
            else
                SendMessage(chat, "Если че правильно вот так:\n\n/set_frequency@piece_fap_bot 3\n\n(чем меньше значение - тем чаще я буду писать)");
        }
        private static void ChatFuse(long chat, string title, Witless witless, string text)
        {
            if (text.Split().Length > 1 && long.TryParse(text.Split()[1], out long key) && key != chat && _sussyBakas.ContainsKey(key))
            {
                witless.Backup();
                FusionCollab fusion = new FusionCollab(witless.Words, _sussyBakas[key].Words);
                fusion.Fuse();
                witless.HasUnsavedStuff = true;
                witless.Save();
                SendMessage(chat, $@"словарь беседы ""{title}"" обновлён!");
            }
            else
                SendMessage(chat, "Если вы хотите объединить словарь <b>этой беседы</b> со словарём <b>другой беседы</b>, где я состою и где есть вы, то для начала скопируйте <b>ID той беседы</b> с помощью команды\n\n/chat_id@piece_fap_bot\n\nи пропишите <b>здесь</b>\n\n/fuse@piece_fap_bot [полученное число]\n\nпример: /fuse@piece_fap_bot -1001541923355\n\nСлияние разово обновит словарь <b>этой беседы</b>", ParseMode.Html);
        }
        private static void ChatDemotivate(long chat, string title, Witless witless, Message message, string text)
        {
            string fileID;
            if (message.Photo != null)
                fileID = message.Photo[^1].FileId;
            else if (message.ReplyToMessage?.Photo != null)
                fileID = message.ReplyToMessage.Photo[^1].FileId;
            else
            {
                SendMessage(chat, "Для генерации демотиватора отправь мне эту команду вместе с фото или в ответ на фото");
                return;
            }

            string a, b = witless.TryToGenerate();
            if (text.Contains(' ')) // custom upper text
            {
                a = text.Substring(text.IndexOf(' ') + 1);
                if (a.Contains('\n')) // custom bottom text
                {
                    b = a.Substring(a.IndexOf('\n') + 1);
                    a = a.Remove(a.IndexOf('\n'));
                }
            }
            else
                a = witless.TryToGenerate();

            var path = $@"{Environment.CurrentDirectory}\Telegram-Pictures\{chat}-{fileID.Remove(62)}.jpg";
            DownloadFile(fileID, path).Wait();
            using (var stream = File.OpenRead(_memes.MakeDemotivator(path, a, b)))
                _client.SendPhotoAsync(chat, new InputOnlineFile(stream)).Wait();
            Log($@"""{title}"": сгенерировано демотиватор [_]");
        }
        private static async Task DownloadFile(string fileId, string path)
        {
            Directory.CreateDirectory($@"{Environment.CurrentDirectory}\Telegram-Pictures");
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

        private static void HandleConsoleInput()
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
                        string text = input.Substring(3);
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
                    else if (input == "/s") SaveDics();
                    else if (input == "/u") ReloadDics();
                }
            } while (input != "s");
            _client.StopReceiving();
            SaveDics();
        }

        private static async void SendMessage(long chat, string text, ParseMode mode = ParseMode.Default)
        {
            try
            {
                await _client.SendTextMessageAsync(chat, text, mode, disableNotification: true)
                    .ContinueWith(task =>
                    {
                        Log(chat + ": " + task.Exception?.Message, ConsoleColor.Red);
                    }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch
            {
                // u/stupid
            }
        }

        private static bool WitlessExist(long chat) => _sussyBakas.ContainsKey(chat);
        
        private static void SaveChatList()
        {
            _fileIO.SaveData(_sussyBakas);
            Log("Список чатов сохранен!", ConsoleColor.Green);
        }

        private static async void SaveLoop()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(60000);
                    _saving.Count();
                    if (_saving.Ready())
                    {
                        SaveDics();
                    }
                }
            });
        }

        private static void SaveDics()
        {
            foreach (var witless in _sussyBakas.Values) witless.Save();
        }
        
        private static void ReloadDics()
        {
            foreach (var witless in _sussyBakas.Values) witless.Load();
        }
    }
}