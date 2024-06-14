﻿namespace Witlesss.Commands
{
    public class SetQuality : SettingsCommand
    {
        protected override void RunAuthorized()
        {
            if (Args is not null && Context.HasIntArgument(out var value))
            {
                Baka.Meme.Quality = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, XDDD(string.Format(SET_Q_RESPONSE, Baka.Meme.Quality)));
                Log($"{Title} >> JPG QUALITY >> {Baka.Meme.Quality}%");
            }
            else
                Bot.SendMessage(Chat, string.Format(SET_X_MANUAL, "quality"));
        }
    }
}