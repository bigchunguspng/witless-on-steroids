﻿using Witlesss.Commands.Meme.Core;

namespace Witlesss.Commands.Settings
{
    public class Set : SettingsCommand
    {
        private readonly Regex _m = new("^[mм]");
        private readonly Regex _t = new("^[tт]");
        private readonly Regex _g = new("^[dд][gгvв]");
        private readonly Regex _d = new("^[dд]");
        private readonly Regex _s = new("^[sс]");
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
                if      (_m.IsMatch(w)) Set(MemeType.Meme, "/meme");
                else if (_t.IsMatch(w)) Set(MemeType.Top,  "/top" );
                else if (_g.IsMatch(w)) Set(MemeType.Dg,   "/dg"  );
                else if (_d.IsMatch(w)) Set(MemeType.Dp,   "/dp"  );
                else if (_s.IsMatch(w)) Set(MemeType.Snap, "/snap");
                else if (_n.IsMatch(w)) Set(MemeType.Nuke, "/nuke");
                else Bot.SendMessage(Origin, string.Format(SET_MEME_TYPE_MANUAL, w));

                if (typeWasChanged)
                {
                    ChatService.SaveChatsDB();
                    Bot.SendMessage(Origin, string.Format(SET_MEMES_RESPONSE, ChatInfo.Types[Data.Type]).XDDD());
                    Log($"{Title} >> MEMES TYPE >> {Data.Type.ToString()[0]}");
                }
                else if (optionsWereChanged)
                {
                    ChatService.SaveChatsDB();
                    Bot.SendMessage(Origin, string.Format(SET_MEME_OPS_RESPONSE, command, result).XDDD());
                    Log($"{Title} >> MEMES OPTIONS");
                }

                void Set(MemeType type, string cmd)
                {
                    if      (args.Length > 1 && args[1] == "?")
                    {
                        var options = Data.GetMemeOptions()[type];
                        options = options is null ? "А НЕТУ!!!" : $"<code>{options}</code>";
                        Bot.SendMessage(Origin, $"Опции команды <b>{cmd}</b>: {options}".XDDD());
                    }
                    else if (args.Length > 1)
                    {
                        command = cmd;
                        result = args[1] == "0" ? null : args[1];
                        Data.GetMemeOptions()[type] = result;
                        if (result is null && (Data.Options?.IsEmpty() ?? false)) Data.Options = null;
                        result = $"{cmd}{result}";
                        optionsWereChanged = true;
                    }
                    else
                    {
                        Data.Type = type;
                        typeWasChanged = true;
                    }
                }
            }
            else
                Bot.SendMessage(Origin, SET_MANUAL);
        }
    }
}