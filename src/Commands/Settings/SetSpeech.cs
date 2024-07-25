using Witlesss.Backrooms.Helpers;

namespace Witlesss.Commands.Settings;

public class SetSpeech : SettingsCommand
{
    protected override void RunAuthorized()
    {
        if (Args is not null && Context.HasIntArgument(out var value))
        {
            Baka.Speech = value.ClampByte();
            ChatsDealer.SaveChatList();
            Bot.SendMessage(Chat, string.Format(SET_FREQUENCY_RESPONSE, Baka.Speech).XDDD());
            Log($"{Title} >> SPEECH >> {Baka.Speech}");
        }
        else
        {
            var message =
                $"""
                 Вероятность ответа: {Baka.Speech}%

                 Изменить: <code>/speech {RandomInt(0, 100)}</code>
                 """;
            Bot.SendMessage(Chat, message);
        }
    }
}