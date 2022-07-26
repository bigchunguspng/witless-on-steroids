using System;
using System.IO;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class DeleteDictionary : Move
    {
        public override void Run()
        {
            string name = ValidFileName(Title.Split()[0]);
            string result = MoveDictionary(name);

            string path = Baka.Path;

            if (!Bot.SussyBakas.TryRemove(Chat, out _))
            {
                Bot.SendMessage(Chat, "Чёт не вышло(9, ещё разок пропиши");
                return;
            }
            Bot.SaveChatList();
            
            File.Delete(path);
            Log($"{Title} >> DIC REMOVED >> {Chat}", ConsoleColor.Magenta);
            Bot.SendMessage(Chat, $"Поздравляю, чат <b>{Title}</b> был удалён из списка чатов, а словарь сохранён как <b>{result}</b>!\n\nЕсли хотите начать заново - пропишите /start@piece_fap_bot");
        }
    }
}