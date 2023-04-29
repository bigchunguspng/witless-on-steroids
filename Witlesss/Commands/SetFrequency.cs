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
                var x = false;
                var w = Text.Split()[1];
                if      (Regex.IsMatch(w, @"^[MmМм]"))
                {
                    Baka.Meme.Type = MemeType.Meme;
                    x = true;
                }
                else if (Regex.IsMatch(w, @"^[DdДд]"))
                {
                    Baka.Meme.Type = MemeType.Dg;
                    x = true;
                }
                else if (Regex.IsMatch(w, @"^[TtCcТтСс]"))
                {
                    Baka.Meme.Type = MemeType.Top;
                    x = true;
                }
                else Bot.SendMessage(Chat, SET_MEMES_MANUAL);

                if (x)
                {
                    Bot.SaveChatList();
                    Bot.SendMessage(Chat, XDDD(string.Format(SET_MEMES_RESPONSE, MEMES_TYPE())));
                    Log($"{Title} >> MEMES TYPE >> {Baka.Meme.Type.ToString()[0]}");
                }
            }
            else Bot.SendMessage(Chat, SET_FREQUENCY_MANUAL);
        }

        private static string MEMES_TYPE() => types[Baka.Meme.Type];
        
        private static readonly Dictionary<MemeType, string> types = new()
        {
            { MemeType.Meme, "мемы"         },
            { MemeType.Dg,   "демотивоторы" },
            { MemeType.Top,  "подписанки"   }
        };
    }
}