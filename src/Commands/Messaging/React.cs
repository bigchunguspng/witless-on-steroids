using Telegram.Bot.Types;

namespace Witlesss.Commands.Messaging;

public class React : SyncCommand
{
    protected override void Run()
    {
        if (!Message.SenderIsBotAdmin())
        {
            Bot.SendMessage(Chat, "LOL XD)0)");
            return;
        }

        if (Args is null)
        {
            var manual =
                """
                code>/re [message_url] [reaction]</code>

                Reactions: 👍👎❤🔥🥰👏😁🤔🤯😱🤬😢🎉🤩🤮💩🙏👌🕊🤡🥱🥴😍🐳❤‍🔥🌚🌭💯🤣⚡🍌🏆💔🤨😐🍓🍾💋🖕😈😴😭🤓👻👨‍💻👀🎃🙈😇😨🤝✍🤗🫡🎅🎄☃💅🤪🗿🆒💘🙉🦄😘💊🙊😎👾🤷‍♂🤷🤷‍♀😡
                """;
            Bot.SendMessage(Chat, manual);
            return;
        }

        var args = Args.SplitN(3);
        var reaction = args.Length < 2
            ? null
            : new ReactionType[] { new ReactionTypeEmoji() { Emoji = args[1] } };

        var (chat, message) = args[0].GetChatIdAndMessage();

        Bot.React(chat, message, reaction);
    }
}