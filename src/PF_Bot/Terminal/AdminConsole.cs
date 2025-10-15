using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;

namespace PF_Bot.Terminal;

public partial class AdminConsole
{
    private readonly CommandRegistry<Action> _registry;
    private          ConsoleContext?         _ctx;
    private          long                    _chat;

    public static void Start()
    {
        Thread.CurrentThread.Name = "Console UI";

        new AdminConsole().Loop();
    }

    private AdminConsole
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

            BigBrother.Log_ADMIN(input);

            if      (input.StartsWith('/')) ResolveCommand(input);
            else if (input.StartsWith('+')) SetActiveChat (input);
        }
        catch (Exception e)
        {
            LogError($"[Console] >> BRUH | {e.GetErrorMessage()}");
        }
    }

    private void ResolveCommand(string input)
    {
        var context = new ConsoleContext(input);
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
}