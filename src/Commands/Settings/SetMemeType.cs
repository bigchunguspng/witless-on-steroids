using System;
using System.Text.RegularExpressions;

namespace Witlesss.Commands.Settings
{
    public class SetMemeType : SettingsCommand
    {
        private readonly Regex _m = new("^[mм]");
        private readonly Regex _t = new("^[tт]");
        private readonly Regex _g = new("^[dд][gг]");
        private readonly Regex _d = new("^[dд]");
        private readonly Regex _n = new("^[nн]");

        protected override void RunAuthorized()
        {
            if (Args is not null)
            {
                string? command = null, result = null;
                var typeWasChanged = false;
                var optionsWereChanged = false;
                var args = Args.ToLower().Split();
                var w = args[0];
                if      (_m.IsMatch(w)) Set(x => Baka.GetMemeOptions().Meme = x, MemeType.Meme, "/meme");
                else if (_t.IsMatch(w)) Set(x => Baka.GetMemeOptions().Top  = x, MemeType.Top,  "/top");
                else if (_g.IsMatch(w)) Set(x => Baka.GetMemeOptions().Dg   = x, MemeType.Dg,   "/dg");
                else if (_d.IsMatch(w)) Set(x => Baka.GetMemeOptions().Dp   = x, MemeType.Dp,   "/dp");
                else if (_n.IsMatch(w)) Set(x => Baka.GetMemeOptions().Nuke = x, MemeType.Nuke, "/nuke");
                else Bot.SendMessage(Chat, SET_MEME_TYPE_MANUAL);

                if (typeWasChanged)
                {
                    ChatsDealer.SaveChatList();
                    Bot.SendMessage(Chat, string.Format(SET_MEMES_RESPONSE, ChatInfo.Types[Baka.Type]).XDDD());
                    Log($"{Title} >> MEMES TYPE >> {Baka.Type.ToString()[0]}");
                }
                else if (optionsWereChanged)
                {
                    ChatsDealer.SaveChatList();
                    Bot.SendMessage(Chat, string.Format(SET_MEME_OPS_RESPONSE, command, result).XDDD());
                    Log($"{Title} >> MEMES OPTIONS");
                }

                void Set(Action<string?> setOptions, MemeType type, string cmd)
                {
                    if (args.Length > 1)
                    {
                        command = cmd;
                        result = args[1] == "0" ? null : args[1];
                        setOptions(result);
                        if (result is null && (Baka.Options?.IsEmpty() ?? false)) Baka.Options = null;
                        result = $"{cmd}{result}";
                        optionsWereChanged = true;
                    }
                    else
                    {
                        Baka.Type = type;
                        typeWasChanged = true;
                    }
                }
            }
            else
                Bot.SendMessage(Chat, SET_MANUAL);
        }
    }
}