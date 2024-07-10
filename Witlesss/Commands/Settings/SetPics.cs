using Witlesss.Backrooms.Helpers;

namespace Witlesss.Commands.Settings;

public class SetPics : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Baka.Pics = value.ClampByte();
            ChatsDealer.SaveChatList();
            Bot.SendMessage(Chat, string.Format(SET_P_RESPONSE, Baka.Pics).XDDD());
            Log($"{Title} >> MEME CHANCE >> {Baka.Pics}%");
        }
        else
        {
            var message =
                $"""
                 Вероятность мемчиков: {Baka.Pics}%

                 Изменить: <code>/pics {RandomInt(0, 100)}</code>
                 """;
            Bot.SendMessage(Chat, message);
        }
    }
}