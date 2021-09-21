using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;
using static Witlesss.Logger;

namespace Witlesss
{
    class Program
    {
        private static string _token;
        private static TelegramBotClient _client;
        private static Dictionary<long, Witless> _sussyBakas;
        private static FileIO<Dictionary<long, Witless>> _fileIO;
        private static long _activeChat;

        static void Main(string[] args)
        {
            _token = File.ReadAllText($@"{Environment.CurrentDirectory}\.token");
            
            _fileIO = new FileIO<Dictionary<long, Witless>>($@"{Environment.CurrentDirectory}\Telegram-ChatsDB.json");
            _sussyBakas = _fileIO.LoadData();

            _client = new TelegramBotClient(_token);
            _client.StartReceiving();
            _client.OnMessage += OnMessageHandler;

            HandleConsoleInput();
        }

        private static async void OnMessageHandler(object sender, MessageEventArgs e)
        {
            var message = e.Message;
            string text = message.Text;
            long chat = message.Chat.Id;
            string title = message.Chat.Title;

            if (WitlessExist(chat))
            {
                var witless = _sussyBakas[chat];
                
                if (text != null && text.StartsWith('/'))
                {
                    if (CommandFrom(text).StartsWith("/set_frequency"))
                    {
                        if (text.Split().Length > 1 && int.TryParse(text.Split()[1], out int value))
                        {
                            witless.Interval = value;
                            SaveChatList();
                            await _client.SendTextMessageAsync(chat, $"я буду писать сюда каждые {witless.Interval} кг сообщений");
                            Log($@"""{title}"": интервал генерации изменен на {witless.Interval}");
                        }
                    
                        return;
                    }
                    if (CommandFrom(text) == "/chat_id")
                    {
                        await _client.SendTextMessageAsync(chat, chat.ToString());
                        return;
                    }

                    if (CommandFrom(text).StartsWith("/fuse"))
                    {
                        if (text.Split().Length > 1 && long.TryParse(text.Split()[1], out long value) && value != chat && _sussyBakas.ContainsKey(value))
                        {
                            witless.Backup();
                            FusionCollab fusion = new FusionCollab(witless.Words, _sussyBakas[value].Words);
                            fusion.Fuse();
                            witless.Save();
                        }
                    }
                }
                
                if (witless.ReceiveSentence(text))
                {
                    Log($@"""{title}"": получено сообщение ""{text}""", ConsoleColor.Blue);
                    witless.TryToSave();
                }
                
                witless.Count();
                
                if (witless.ReadyToGen())
                {
                    await _client.SendTextMessageAsync(chat, witless.Generate());
                    Log($@"""{title}"": сгенерировано прикол");
                }
            }
            else if (text != null && CommandFrom(text) == "/start")
            {
                _sussyBakas.Add(chat, new Witless(chat));
                Log($@"Создано базу для чата {chat} ({title})");

                SaveChatList();

                await _client.SendTextMessageAsync(chat, "ВИРУСНАЯ БАЗА ОБНОВЛЕНА!");
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
                        foreach (var baka in _sussyBakas)
                        {
                            if (baka.Key.ToString().EndsWith(shit))
                            {
                                _activeChat = baka.Key;
                                Log($"Выбрано чат {_activeChat}");
                                break;
                            }
                        }
                    }
                    else if (WitlessExist(_activeChat) && input.Length > 3)
                    {
                        string text = input.Substring(3);
                        Witless witless = _sussyBakas[_activeChat];
                        if (input.StartsWith("/a ") ) //add
                        {
                            witless.ReceiveSentence(text);
                            Log($@"{_activeChat}: в словарь добавлено ""{text}""", ConsoleColor.Yellow);
                        }
                        else if (input.StartsWith("/w ")) //write
                        {
                            _client.SendTextMessageAsync(_activeChat, text);
                            witless.ReceiveSentence(text);
                            Log($@"{_activeChat}: отправлено в чат и добавлено в словарь ""{text}""", ConsoleColor.Yellow);
                        }
                    }
                    else if (input == "/s") SaveDics();
                    else if (input == "/u") ReloadDics();
                }
            } while (input != "s");
            _client.StopReceiving();
            SaveDics();
        }

        private static string CommandFrom(string text) => text.Replace("@piece_fap_bot", "");
        
        private static bool WitlessExist(long chat) => _sussyBakas.ContainsKey(chat);
        
        private static void SaveChatList()
        {
            _fileIO.SaveData(_sussyBakas);
            Log("Список чатов сохранен!", ConsoleColor.Blue);
        }

        private static void SaveDics()
        {
            foreach (KeyValuePair<long, Witless> baka in _sussyBakas) baka.Value.Save();
        }
        
        private static void ReloadDics()
        {
            foreach (KeyValuePair<long,Witless> baka in _sussyBakas) baka.Value.Load();
        }
    }
}