using PF_Bot.Core;
using PF_Bot.Core.Chats;
using PF_Bot.Core.Text;
using PF_Bot.Handlers.Media.MediaDB;
using PF_Bot.Routing;
using PF_Tools.Copypaster.Helpers;
using Telegram.Bot;
using Exception = System.Exception;

namespace PF_Bot.Terminal
{
    public class TerminalUI
    {
        private readonly CommandRegistry<Action> _registry;
        private          TerminalContext?        _ctx;
        private          long                    _chat;

        public static void Start()
        {
            Thread.CurrentThread.Name = "Terminal UI";

            new TerminalUI().Loop();
        }

        private TerminalUI
            () => _registry = new CommandRegistry<Action>.Builder()
            .Register("?",  PrintManual)
            .Register("a",    AddTextToPack)
            .Register("w",  WriteTextToChat)
            .Register("s",  PackManager.Bakas_SaveDirty_DropIdle)
            .Register("p",  PacksInfo)
            .Register("pp", PacksInfoFull)
            .Register("xp", PackCopyJson)
            .Register("mg", Migration_JsonToBinary.MigrateAll)
            .Register("cc", ClearTempFiles)
            .Register("UG", UploadGIFs)
            .Register("US", UploadSounds)
            .Register("db", DeleteBlockers_SaveChats)
            .Register("DB", DeleteBlocker__SaveChats)
            .Build();

        private void PrintManual() => Print(CONSOLE_MANUAL, ConsoleColor.Yellow);

        private void Loop()
        {
            string? input;
            do
            {
                input = Console.ReadLine();
                HandleInput(input);
            }
            while (input != "s");
        }

        private void HandleInput(string? input)
        {
            try
            {
                if (input.IsNull_OrWhiteSpace() || input.EndsWith("_")) return;

                if      (input.StartsWith("/")) ResolveCommand(input);
                else if (input.StartsWith("+")) SetActiveChat (input);
            }
            catch (Exception e)
            {
                LogError($"[Console] >> BRUH | {e.GetErrorMessage()}");
            }
        }

        private void ResolveCommand(string input)
        {
            var context = new TerminalContext(input);
            var handler = _registry.Resolve(context.Command);
            if (handler != null)
            {
                _ctx = context;
                handler.Invoke();
                _ctx = null;
            }
        }

        private void SetActiveChat(string input)
        {
            if (input.Length < 2) return;

            var shit = input[1..];
            var chat = ChatManager.Chats.Lock(x => x.Keys.FirstOrDefault(chat => $"{chat}".EndsWith(shit)));
            if (chat != 0)
            {
                _chat = chat;
                Print($"ACTIVE CHAT >> {_chat}", ConsoleColor.Yellow);
            }
            else
                Print("CHAT NOT FOUND :(", ConsoleColor.Red);
        }

        // ADIDAS IS TYPING...

        private void   AddTextToPack() => BreakFourthWall((baka, text) =>
        {
            if (baka.Eat(text, out var eaten) == false) return;

            eaten.ForEach(line => Print($"{_chat} += {line}", ConsoleColor.Yellow));
        });

        private void WriteTextToChat() => BreakFourthWall((baka, text) =>
        {
            App.Bot.SendMessage((_chat, null), text, preview: true);
            baka.Eat(text);
            Print($"{_chat} >> {text}", ConsoleColor.Yellow);
        });

        private void BreakFourthWall(Action<Copypaster, string> doSomeFunnyShit)
        {
            var text = _ctx!.Args;
            if (text == null)
            {
                Print("NO TEXT?", ConsoleColor.Red);
                return;
            }

            if (ChatManager.Knowns(_chat).Janai())
            {
                Print(_chat == 0 ? "CHAT NOT SELECTED" : "UNKNOWN CHAT", ConsoleColor.Red);
                return;
            }

            doSomeFunnyShit(PackManager.GetBaka(_chat), text);
        }

        // PACKS

        private void PacksInfo()
        {
            var loaded = PackManager.Bakas.Count;
            var total  = ChatManager.Chats.Count;
            Print($"PACKS: {loaded} LOADED / {total} TOTAL", ConsoleColor.Yellow);
        }

        private void PacksInfoFull()
        {
            PacksInfo();
            PackManager.Bakas.ForEachKey(chat => Print($"{chat}", ConsoleColor.DarkYellow));
        }

        private void PackCopyJson()
        {
            var chat = long.TryParse(_ctx?.Args, out var value) ? value : _chat;
            var path = PackManager.GetPackPath(chat);
            var pack = GenerationPackIO.Load(path);
            var save = path.Suffix($"{DateTime.Now:yyyy-MM-dd--hh-mm-ss}", ".json");
            GenerationPackIO.Save_Json(pack, save).Wait();
            Print($"PACK EXPORTED >> {save}", ConsoleColor.Yellow);
        }

        // UPLOAD

        private void UploadGIFs()
        {
            var path = _ctx?.Args;
            if (path == null) Print("NO PATH?", ConsoleColor.Red);
            else Task.Run(() => GIF_DB .Instance.UploadMany(path));
        }

        private void UploadSounds()
        {
            var path = _ctx?.Args;
            if (path == null) Print("NO PATH?", ConsoleColor.Red);
            else Task.Run(() => SoundDB.Instance.UploadMany(path));
        }

        // DELETE

        private void DeleteBlockers_SaveChats()
        {
            var save = ChatManager.Chats.Lock(x => x.Keys.Aggregate(false, (b, chat) => b || DeleteBlocker(chat)));
            if (save)  ChatManager.SaveChats();
        }

        private void DeleteBlocker__SaveChats()
        {
            if (DeleteBlocker(_chat)) ChatManager.SaveChats();
        }

        private bool DeleteBlocker(long chat)
        {
            var messageId = App.Bot.PingChat((chat, null), notify: false);
            var delete = messageId == -1;
            if (delete)
            {
                ChatManager.Remove(chat);
                PackManager.Delete(chat);
            }
            else App.Bot.Client.DeleteMessage(chat, messageId);

            return delete;
        }
    }
}