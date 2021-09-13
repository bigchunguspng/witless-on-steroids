using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Bot;
using Telegram.Bot.Args;

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
                        await _client.SendTextMessageAsync(chat, $"я буду писать сюда каждые {witless.Interval} кг сообщений");
                        Log($@"интервал генерации изменен на {witless.Interval} для чата ""{title}""");
                    }
                    
                    return;
                }
                
                if (witless.ReceiveSentence(text))
                {
                    Log($@"получено сообщение ""{text}"" с чата ""{title}"", ID: {chat}");
                }
                
                witless.Count();
                Log($@"""{title}"" - {witless.Counter.ToString()}");
                
                if (witless.Counter == 0)
                {
                    await _client.SendTextMessageAsync(chat, witless.Generate());
                    Log($@"сгенерировано прикол в чат ""{title}""");
                }
            }
            else if (CommandFrom(text) == "/start")
            {
                _sussyBakas.Add(chat, new Witless(chat, 3));
                Log($@"создано базу для чата ""{title}"", ID: {chat}");

                _fileIO.SaveData(_sussyBakas);
                Log("список чатов сохранен!");

                await _client.SendTextMessageAsync(chat, "ВИРУСНАЯ БАЗА ОБНОВЛЕНА!");
            }
        }

        private static void Log(string message) => Console.WriteLine(message);

        private static string CommandFrom(string text) => text.Replace("@piece_fap_bot", "");
        
        private static bool WitlessExist(long chat) => _sussyBakas.ContainsKey(chat);
    }
}