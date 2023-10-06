using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class SetFrequency : ToggleAdmins
    {
        public override void Run()
        {
            if (SenderIsSus()) return;

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
                var t = false;
                var o = false;
                var s = Text.Split();
                var w = s[1];
                if      (Regex.IsMatch(w, @"^[MmМм]"))
                {
                    if (s.Length > 2)
                    {
                        command = "/meme";
                        Baka.Meme.OptionsM = s[2] == "0" ? null : command + s[2];
                        result = Baka.Meme.OptionsM ?? command;
                        o = true;
                    }
                    else
                    {
                        Baka.Meme.Type = MemeType.Meme;
                        t = true;
                    }
                }
                else if (Regex.IsMatch(w, @"^[DdДд][GgГг]"))
                {
                    Baka.Meme.Type = MemeType.Dg;
                    t = true;
                }
                else if (Regex.IsMatch(w, @"^[DdДд]"))
                {
                    if (s.Length > 2)
                    {
                        command = "/dp";
                        Baka.Meme.OptionsD = s[2] == "0" ? null : command + s[2];
                        result = Baka.Meme.OptionsD ?? command;
                        o = true;
                    }
                    else
                    {
                        Baka.Meme.Type = MemeType.Dp;
                        t = true;
                    }
                }
                else if (Regex.IsMatch(w, @"^[TtCcТтСс]"))
                {
                    if (s.Length > 2)
                    {
                        command = "/top";
                        Baka.Meme.OptionsT = s[2] == "0" ? null : command + s[2];
                        result = Baka.Meme.OptionsT ?? command;
                        o = true;
                    }
                    else
                    {
                        Baka.Meme.Type = MemeType.Top;
                        t = true;
                    }
                }
                else Bot.SendMessage(Chat, SET_MEMES_MANUAL);

                if (t)
                {
                    Bot.SaveChatList();
                    Bot.SendMessage(Chat, XDDD(string.Format(SET_MEMES_RESPONSE, MEMES_TYPE())));
                    Log($"{Title} >> MEMES TYPE >> {Baka.Meme.Type.ToString()[0]}");
                }
                else if (o)
                {
                    Bot.SaveChatList();
                    Bot.SendMessage(Chat, XDDD(string.Format(SET_MEME_OPS_RESPONSE, command, result)));
                    Log($"{Title} >> MEMES OPTIONS");
                }
            }
            else Bot.SendMessage(Chat, SET_FREQUENCY_MANUAL);
        }

        private static string MEMES_TYPE() => types[Baka.Meme.Type];
        
        private static readonly Dictionary<MemeType, string> types = new()
        {
            { MemeType.Meme, "мемы"           },
            { MemeType.Dg,   "демотиваторы"   },
            { MemeType.Top,  "подписанки"     },
            { MemeType.Dp,   "демотиваторы-B" }
        };
    }
}