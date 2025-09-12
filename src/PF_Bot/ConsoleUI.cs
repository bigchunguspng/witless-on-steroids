using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core;
using PF_Bot.Core.Chats;
using PF_Bot.Core.Text;
using PF_Bot.Features.Media.MediaDB;
using PF_Bot.Tools_Legacy.RedditSearch;
using Telegram.Bot;
using Exception = System.Exception;

namespace PF_Bot
{
    public class ConsoleUI
    {
        private long   _activeChat;
        private string? _input;

        private Telegram.Bot Bot => Telegram.Bot.Instance;

        private Copypaster Baka => ChatManager.GetBaka(_activeChat);

        public static bool LoggedIntoReddit = false;


        public void EnterConsoleLoop()
        {
            Thread.CurrentThread.Name = "Console UI";
            Console.CancelKeyPress              += (_, _) => SaveAndExit();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => SaveAndExit();
            do
            {
                _input = Console.ReadLine();
                try
                {
                    if (_input != null && _input.EndsWith("_").Janai())
                    {
                        if      (_input.StartsWith("+") && _input.Length > 1) SetActiveChat();
                        else if (_input.StartsWith("/")                     ) DoConsoleCommands();
                    }
                }
                catch (Exception e)
                {
                    LogError($"[Console] >> BRUH | {e.GetErrorMessage()}");
                }
            }
            while (_input != "s");
        }

        private void SaveAndExit()
        {
            Print("На выход…", ConsoleColor.Yellow);
            Telemetry.Log_EXIT(Bot.Me);
            Telemetry.Write();
            ChatManager.Bakas_SaveDirty();
            if (LoggedIntoReddit) RedditTool.Instance.SaveExcluded();
        }

        private void DoConsoleCommands()
        {
            if (_input is null) return;

            if      (BotWannaSpeak()) BreakFourthWall();
            else if (_input == "/"  ) Print(CONSOLE_MANUAL, ConsoleColor.Yellow);
            else if (_input == "/s" ) ChatManager.Bakas_SaveDirty_UnloadIdle();
            else if (_input == "/p" ) PacksInfo();
            else if (_input == "/pp") PacksInfoFull();
            else if (_input == "/cc") ClearTempFiles();
            else if (_input == "/db") DeleteBlockers();
            else if (_input == "/DB") DeleteBlocker();
            else if (_input == "/ds") DeleteBySize();
            else if (_input.StartsWith("/ups") && _input.Contains(' ')) UploadSounds(_input.Split(' ', 2)[1]);
            else if (_input.StartsWith("/upg") && _input.Contains(' ')) UploadGIFs  (_input.Split(' ', 2)[1]);
            else if (_input.StartsWith("/ds")  && _input.HasIntArgument(out var size)) DeleteBySize(size);
        }

        private bool BotWannaSpeak() => Regex.IsMatch(_input!, @"^\/[aw] ");

        private void SetActiveChat()
        {
            var shit = _input![1..];
            var found = ChatManager.SettingsDB.Lock(x => x.Keys.FirstOrDefault(chat => $"{chat}".EndsWith(shit)));
            if (found != 0)
            {
                _activeChat = found;
                Print($"ACTIVE CHAT >> {_activeChat}");
            }
        }

        private void BreakFourthWall()
        {
            var arg = _input!.Split (' ', 2)[1];
            if (ChatManager.KnownsChat(_activeChat).Janai()) return;

            if      (_input.StartsWith("/a ") && Baka.Eat(arg, out var eaten)) // add
            {
                foreach (var line in eaten) Print($"{_activeChat} += {line}", ConsoleColor.Yellow);
            }
            else if (_input.StartsWith("/w "))                                  // write
            {
                Bot.SendMessage((_activeChat, null), arg, preview: true);
                Baka.Eat(arg);
                Print($"{_activeChat} >> {arg}", ConsoleColor.Yellow);
            }
        }

        private void PacksInfo()
        {
            var loaded = ChatManager.LoadedBakas.Count;
            var total  = ChatManager.SettingsDB .Count;
            Print($"PACKS: {loaded} LOADED / {total} TOTAL", ConsoleColor.Yellow);
        }

        private void PacksInfoFull()
        {
            PacksInfo();
            ChatManager.LoadedBakas.ForEachKey(chat => Print($"{chat}", ConsoleColor.DarkYellow));
        }

        private void DeleteBlockers()
        {
            var save = ChatManager.SettingsDB.Lock(x => x.Keys.Aggregate(false, (b, chat) => b || DeleteBlocker(chat)));
            if (save)  ChatManager.SaveChatsDB();
        }

        private void DeleteBlocker()
        {
            if (DeleteBlocker(_activeChat)) ChatManager.SaveChatsDB();
        }

        private bool DeleteBlocker(long chat)
        {
            var x = Bot.PingChat((chat, null), notify: false);
            if (x == -1)
            {
                ChatManager.RemoveChat(chat);
                ChatManager.DeletePack(chat);
            }
            else Bot.Client.DeleteMessage(chat, x);

            return x is -1;
        }

        private void DeleteBySize(int size = 34)
        {
            ChatManager.SettingsDB.ForEachKey(chat =>
            {
                if (ChatManager.GetPackPath(chat).FileSizeInBytes > size) return;

                ChatManager.RemoveChat(chat);
                ChatManager.DeletePack(chat);
            });
            ChatManager.SaveChatsDB();
        }

        private void UploadSounds(string path) => Task.Run(() => SoundDB.Instance.UploadMany(path));
        private void UploadGIFs  (string path) => Task.Run(() => GIF_DB .Instance.UploadMany(path));
    }
}