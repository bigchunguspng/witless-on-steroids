using System;
using System.IO;

namespace Witlesss.Commands
{
    public class DeleteDictionary : Move
    {
        public override void Run()
        {
            if (SenderIsSus()) return;

            string name = ValidFileName(Title.Split()[0]);
            string result = MoveDictionary(name);
            
            if (result == "*") result = "*👊 никак*";

            string path = Baka.Path;

            if (!Bot.SussyBakas.TryRemove(Chat, out _))
            {
                Bot.SendMessage(Chat, "Чёт не вышло(9, ещё разок пропиши");
                return;
            }
            Bot.SaveChatList();
            
            File.Delete(path);
            Log($"{Title} >> DIC REMOVED >> {Chat}", ConsoleColor.Magenta);
            Bot.SendMessage(Chat, string.Format(DEL_SUCCESS_RESPONSE, Title, result));
        }
    }
}