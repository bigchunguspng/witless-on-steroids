using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Messages.Commands;

namespace PF_Bot.Commands.Admin.Fun;

public class QueueMessage : CommandHandlerBlocking_Admin
{
    private static readonly Regex
        _r_repeats = new("(?<![<>=])([2-9])", RegexOptions.Compiled);

    protected override void Run()
    {
        if (Args is null)
        {
            SendManual(MANUAL);
            return;
        }

        var repeats_match = _r_repeats.Match(Options);
        var repeats = repeats_match.ExtractGroup(1, int.Parse, 1);

        if (Options.IsNull_OrEmpty() || Options.Length == repeats_match.Length)
        {
            var args = Args.SplitN(2);
            var chat = args[0] is "." ? Chat : long.Parse(args[0]);
            var text = args[1];

            Queue(chat, text, repeats);
            Bot.ReactAsync(Chat, Message.Id, GetRandomReaction_DONE());
        }
        else
        {
            var text = Args;
            var request = ChatSelector.ParseOptions(Options);
            var bakas   = ChatSelector.GetChats(request);

            Bot.SendMessage(Origin, $"Queued messages to {bakas.Count} chats 😈");

            bakas.ForEach(baka => Queue(baka, text, repeats));
        }
    }

    private static void Queue(long chat, string text, int repeats)
    {
        for (var i = 0; i < repeats; i++)
            App.FunnyMessages.Enqueue(chat, text);

        if (ChatManager.Knowns (chat))
            PackManager.GetBaka(chat).Eat(text);

        Log($"QUEUE >> {chat}", LogLevel.Info, LogColor.Yellow);
    }

    private const string MANUAL =
        $"""
         <code>/que[R]     [chat|.] [text]</code>
         <code>/que[R][g/p/a~D/s~B] [text]</code>

         <code>R</code> = repeats

         {CHAT_FILTERS_MANUAL}
         """;
}