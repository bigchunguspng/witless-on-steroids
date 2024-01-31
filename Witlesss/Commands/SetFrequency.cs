using System;
using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class SetFrequency : SettingsCommand
    {
        protected override void ExecuteAuthorized()
        {
            if (Text.HasIntArgument(out int value))
            {
                Baka.Interval = value;
                Bot.SaveChatList();
                Bot.SendMessage(Chat, SET_FREQUENCY_RESPONSE(Baka.Interval));
                Log($"{Title} >> FUNNY INTERVAL >> {Baka.Interval}");
            }
            else if (Text.Contains(' '))
            {
                string command = null, result = null;
                var typeWasChanged = false;
                var optionsWereChanged = false;
                var args = Text.Split();
                var w = args[1];
                if      (Regex.IsMatch(w, @"^[MmМм]"))       Set(x => Baka.Meme.OptionsM = x, MemeType.Meme, "/meme");
                else if (Regex.IsMatch(w, @"^[TtCcТтСс]"))   Set(x => Baka.Meme.OptionsT = x, MemeType.Top, "/top");
                else if (Regex.IsMatch(w, @"^[DdДд][GgГг]")) Set(x => Baka.Meme.OptionsG = x, MemeType.Dg, "/dg");
                else if (Regex.IsMatch(w, @"^[DdДд]"))       Set(x => Baka.Meme.OptionsD = x, MemeType.Dp, "/dp");
                else if (Regex.IsMatch(w, @"^[NnНнJjЖж]"))   Set(x => Baka.Meme.OptionsN = x, MemeType.Nuke, "/nuke");
                else Bot.SendMessage(Chat, SET_MEMES_MANUAL);

                if (typeWasChanged)
                {
                    Bot.SaveChatList();
                    Bot.SendMessage(Chat, XDDD(string.Format(SET_MEMES_RESPONSE, ChatInfo.Types[Baka.Meme.Type])));
                    Log($"{Title} >> MEMES TYPE >> {Baka.Meme.Type.ToString()[0]}");
                }
                else if (optionsWereChanged)
                {
                    Bot.SaveChatList();
                    Bot.SendMessage(Chat, XDDD(string.Format(SET_MEME_OPS_RESPONSE, command, result)));
                    Log($"{Title} >> MEMES OPTIONS");
                }

                void Set(Action<string> setOptions, MemeType type, string cmd)
                {
                    if (args.Length > 2)
                    {
                        command = cmd;
                        result = args[2] == "0" ? null : command + args[2];
                        setOptions(result);
                        result ??= command;
                        optionsWereChanged = true;
                    }
                    else
                    {
                        Baka.Meme.Type = type;
                        typeWasChanged = true;
                    }
                }
            }
            else Bot.SendMessage(Chat, SET_FREQUENCY_MANUAL);
        }
    }
}