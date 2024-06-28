using System;
using System.Text.RegularExpressions;
using Witlesss.Backrooms.Helpers;

namespace Witlesss.Commands.Settings
{
    public class SetFrequency : SettingsCommand
    {
        private readonly Regex _m = new(@"^[MmМм]");
        private readonly Regex _t = new(@"^[TtCcТтСс]");
        private readonly Regex _g = new(@"^[DdДд][GgГг]");
        private readonly Regex _d = new(@"^[DdДд]");
        private readonly Regex _n = new(@"^[NnНнJjЖж]");

        protected override void RunAuthorized()
        {
            if (Args is null)
            {
                Bot.SendMessage(Chat, SET_FREQUENCY_MANUAL);
            }
            else if (Context.HasIntArgument(out var value))
            {
                Baka.Interval = value;
                ChatsDealer.SaveChatList();
                Bot.SendMessage(Chat, SET_FREQUENCY_RESPONSE(Baka.Interval));
                Log($"{Title} >> FUNNY INTERVAL >> {Baka.Interval}");
            }
            else
            {
                string? command = null, result = null;
                var typeWasChanged = false;
                var optionsWereChanged = false;
                var args = Args.Split();
                var w = args[0];
                if      (_m.IsMatch(w)) Set(x => Baka.Meme.GetMemeOptions().Meme = x, MemeType.Meme, "/meme");
                else if (_t.IsMatch(w)) Set(x => Baka.Meme.GetMemeOptions().Top  = x, MemeType.Top,  "/top");
                else if (_g.IsMatch(w)) Set(x => Baka.Meme.GetMemeOptions().Dg   = x, MemeType.Dg,   "/dg");
                else if (_d.IsMatch(w)) Set(x => Baka.Meme.GetMemeOptions().Dp   = x, MemeType.Dp,   "/dp");
                else if (_n.IsMatch(w)) Set(x => Baka.Meme.GetMemeOptions().Nuke = x, MemeType.Nuke, "/nuke");
                else Bot.SendMessage(Chat, SET_MEMES_MANUAL);

                if (typeWasChanged)
                {
                    ChatsDealer.SaveChatList();
                    Bot.SendMessage(Chat, string.Format(SET_MEMES_RESPONSE, ChatInfo.Types[Baka.Meme.Type]).XDDD());
                    Log($"{Title} >> MEMES TYPE >> {Baka.Meme.Type.ToString()[0]}");
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
                        Baka.Meme.Type = type;
                        typeWasChanged = true;
                    }
                }
            }
        }
    }
}