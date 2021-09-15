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

        static void Main(string[] args)
        {
            _token = File.ReadAllText($@"{Environment.CurrentDirectory}\.token");
            
            _fileIO = new FileIO<Dictionary<long, Witless>>($@"{Environment.CurrentDirectory}\Telegram-ChatsDB.json");
            _sussyBakas = _fileIO.LoadData();

            _client = new TelegramBotClient(_token);
            _client.StartReceiving();
            _client.OnMessage += OnMessageHandler;
            Console.ReadLine();
            
            _client.StopReceiving();
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
                
                if (text != null && CommandFrom(text).StartsWith("/set_frequency"))
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
                
                if (witless.ReceiveSentence(text))
                {
                    Log($@"""{title}"": получено сообщение ""{text}""");
                }
                
                witless.Count();
                
                if (witless.ReadyToGen())
                {
                    await _client.SendTextMessageAsync(chat, witless.Generate());
                    Log($@"""{title}"": сгенерировано прикол");
                }
            }
            else if (CommandFrom(text) == "/start")
            {
                _sussyBakas.Add(chat, new Witless(chat, 3));
                Log($@"Создано базу для чата {chat} ({title})");

                SaveChatList();

                await _client.SendTextMessageAsync(chat, "ВИРУСНАЯ БАЗА ОБНОВЛЕНА!");
            }
        }

        

        private static string CommandFrom(string text) => text.Replace("@piece_fap_bot", "");
        
        private static bool WitlessExist(long chat) => _sussyBakas.ContainsKey(chat);
        
        private static void SaveChatList()
        {
            _fileIO.SaveData(_sussyBakas);
            Log("Список чатов сохранен!");
        }
    }
}