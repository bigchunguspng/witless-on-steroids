using System;
using System.Text.RegularExpressions;
using Witlesss.Backrooms.Helpers;

namespace Witlesss.Commands.Settings
{
    public class SetFrequency : SettingsCommand
    {
        private readonly Regex _m = new(@"^[mм]");
        private readonly Regex _t = new(@"^[tcтс]");
        private readonly Regex _g = new(@"^[dд][gг]");
        private readonly Regex _d = new(@"^[dд]");
        private readonly Regex _n = new(@"^[nнjж]");

        protected override void RunAuthorized()
        {
            if (Args is null)
            {
                Bot.SendMessage(Chat, SET_FREQUENCY_MANUAL);
            }
            else if (Context.HasIntArgument(out var value))
            {
                Baka.Speech = value.ClampByte();
                ChatsDealer.SaveChatList();
                Bot.SendMessage(Chat, SET_FREQUENCY_RESPONSE(Baka.Speech));
                Log($"{Title} >> SPEECH >> {Baka.Speech}");
            }
            else
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
                else Bot.SendMessage(Chat, SET_MEMES_MANUAL);

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
                        result = args[1] == "0" ? null : command + args[1];
                        setOptions(result);
                        result ??= command;
                        optionsWereChanged = true;
                    }
                    else
                    {
                        Baka.Type = type;
                        typeWasChanged = true;
                    }
                }
            }
        }
    }
}