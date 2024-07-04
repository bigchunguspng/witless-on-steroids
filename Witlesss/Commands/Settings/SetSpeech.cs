﻿using Witlesss.Backrooms.Helpers;

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
            Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "speech"));
    }
}