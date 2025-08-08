using Telegram.Bot.Types;

namespace Witlesss.Commands.Messaging;

public class React : SyncCommand
{
    protected override void Run()
    {
        if (!Message.SenderIsBotAdmin())
        {
            Bot.SendMessage(Origin, FORBIDDEN.PickAny());
            return;
        }

        if (Args is null)
        {
            var manual =
                """
                <code>/re [message_url] [reaction]</code>

                Reactions: 👍👎❤🔥🥰👏😁🤔🤯😱🤬😢🎉🤩🤮💩🙏👌🕊🤡🥱🥴😍🐳❤‍🔥🌚🌭💯🤣⚡🍌🏆💔🤨😐🍓🍾💋🖕😈😴😭🤓👻👨‍💻👀🎃🙈😇😨🤝✍🤗🫡🎅🎄☃💅🤪🗿🆒💘🙉🦄😘💊🙊😎👾🤷‍♂🤷🤷‍♀😡
                """;
            Bot.SendMessage(Origin, manual);
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
}