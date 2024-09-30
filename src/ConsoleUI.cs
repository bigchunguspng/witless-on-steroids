using Telegram.Bot;
using Witlesss.Services.Internet.Reddit;

namespace Witlesss
{
    public class ConsoleUI
    {
        private long   _activeChat;
        private string? _input;

        private Bot Bot => Bot.Instance;

        private CopypasterProxy Baka => ChatService.GetBaka(_activeChat);

        public static bool LoggedIntoReddit = false;


        public void EnterConsoleLoop()
        {
            Console.CancelKeyPress              += (_, _) => SaveAndExit();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveAndExit();
            do
            {
                _input = Console.ReadLine();
                try
                {
                    if (_input != null && !_input.EndsWith("_"))
                    {
                        if      (_input.StartsWith("+") && _input.Length > 1) SetActiveChat();
                        else if (_input.StartsWith("/")                     ) DoConsoleCommands();
                    }
                }
                catch
                {
                    Log(">:^< u/stupid >:^<", ConsoleColor.Yellow);
                }
            }
            while (_input != "s");
        }

        private void SaveAndExit()
        {
            Log("На выход…", ConsoleColor.Yellow);
            ChatService.SaveBakas();
            if (LoggedIntoReddit) RedditTool.Instance.SaveExcluded();
        }

        private void DoConsoleCommands()
        {
            if (_input is null) return;

            if      (BotWannaSpeak()) BreakFourthWall();
            else if (_input == "/"  ) Log(CONSOLE_MANUAL, ConsoleColor.Yellow);
            else if (_input == "/s" ) ChatService.PerformAutoSave();
            else if (_input == "/p" ) PacksInfo();
            else if (_input == "/pp") PacksInfoFull();
            else if (_input == "/cc") ClearTempFiles();
            else if (_input == "/db") DeleteBlockers();
            else if (_input == "/DB") DeleteBlocker();
            else if (_input == "/ds") DeleteBySize();
            else if (_input.StartsWith("/ds") && _input.HasIntArgument(out var size)) DeleteBySize(size);
        }

        private bool BotWannaSpeak() => Regex.IsMatch(_input!, @"^\/[aw] ");

        private void SetActiveChat()
        {
            var shit = _input![1..];
            var found = ChatService.SettingsDB.Do(x => x.Keys.FirstOrDefault(chat => $"{chat}".EndsWith(shit)));
            if (found != 0)
            {
                _activeChat = found;
                Log($"ACTIVE CHAT >> {_activeChat}");
            }
        }

        private void BreakFourthWall()
        {
            var arg = _input!.Split (' ', 2)[1];
            if (!ChatService.Knowns(_activeChat)) return;

            if      (_input.StartsWith("/a ") && Baka.Eat(arg, out var eaten)) // add
            {
                foreach (var line in eaten) Log($"{_activeChat} += {line}", ConsoleColor.Yellow);
            }
            else if (_input.StartsWith("/w "))                                  // write
            {
                Bot.SendMessage(_activeChat, arg, preview: true);
                Baka.Eat(arg);
                Log($"{_activeChat} >> {arg}", ConsoleColor.Yellow);
            }
        }

        private void PacksInfo()
        {
            var loaded = ChatService.LoadedBakas.Count;
            var total  = ChatService.SettingsDB .Count;
            Log($"PACKS: {loaded} LOADED / {total} TOTAL", ConsoleColor.Yellow);
        }

        private void PacksInfoFull()
        {
            PacksInfo();
            ChatService.LoadedBakas.ForEachKey(chat => Log($"{chat}", ConsoleColor.DarkYellow));
        }

        private void DeleteBlockers()
        {
            var save = ChatService.SettingsDB.Do(x => x.Keys.Aggregate(false, (b, chat) => b || DeleteBlocker(chat)));
            if (save)  ChatService.SaveChatsDB();
        }

        private void DeleteBlocker()
        {
            if (DeleteBlocker(_activeChat)) ChatService.SaveChatsDB();
        }

        private bool DeleteBlocker(long chat)
        {
            var x = Bot.PingChat(chat, notify: false);
            if (x == -1)
            {
                ChatService.RemoveChat(chat);
                ChatService.BackupPack(chat);
                ChatService.DeletePack(chat);
            }
            else Bot.Client.DeleteMessageAsync(chat, x);

            return x is -1;
        }

        private void DeleteBySize(int size = 34)
        {
            ChatService.SettingsDB.ForEachKey(chat =>
            {
                if (ChatService.GetPath(chat).FileSizeInBytes() > size) return;

                ChatService.RemoveChat(chat);
                ChatService.BackupPack(chat);
                ChatService.DeletePack(chat);
            });
            ChatService.SaveChatsDB();
        }
    }
}