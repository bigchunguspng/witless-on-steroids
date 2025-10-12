using Telegram.Bot.Types;

namespace PF_Bot.Commands.Admin.Fun;

public class React : CommandHandlerBlocking_Admin
{
    protected override void RunAuthourized()
    {
        if (Args is null)
        {
            SendManual(MANUAL);
            return;
        }

        var args = Args.SplitN(3);
        var reaction = args.Length < 2
            ? null
            : new ReactionType[] { new ReactionTypeEmoji() { Emoji = args[1] } };

        var (chat, message) = args[0].GetChatIdAndMessage();

        Bot.ReactAsync(chat, message, reaction);
        Log($"REACTION >> {chat}", LogLevel.Info, LogColor.Yellow);
    }

    private const string
        RE1 = @"👍👎❤🔥🥰👏😁🤔🤯😱🤬😢🎉🤩🤮💩🙏👌🕊🤡🥱🥴😍🐳❤‍",
        RE2 = @"🔥🌚🌭💯🤣⚡🍌🏆💔🤨😐🍓🍾💋🖕😈😴😭🤓👻👨‍💻👀🎃🙈😇",
        RE3 = @"😨🤝✍🤗🫡🎅🎄☃💅🤪🗿🆒💘🙉🦄😘💊🙊😎👾🤷‍♂🤷🤷‍♀😡",
        MANUAL =
            $"""
             <code>/re [message_url] [reaction]</code>

             Reactions: {RE1}{RE2}{RE3}
             """;
}