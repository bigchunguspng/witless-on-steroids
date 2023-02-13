using System;

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

            Bot.RemoveChat(Chat);
            Bot.SaveChatList();

            Baka.DeleteForever();

            DropBaka();

            Log($"{Title} >> DIC REMOVED >> {Chat}", ConsoleColor.Magenta);
            Bot.SendMessage(Chat, string.Format(DEL_SUCCESS_RESPONSE, Title, result));
        }
    }
}