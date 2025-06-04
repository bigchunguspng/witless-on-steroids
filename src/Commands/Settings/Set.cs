using Witlesss.Commands.Meme.Core;
using Witlesss.Commands.Routing;

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
        private readonly Regex _u = new("^[aа]");

        protected override void RunAuthorized()
        {
            if (Args is not null)
            {
                var args = Args.ToLower().Split();
                var w = args[0];
                if      (_m.IsMatch(w)) Set(MemeType.Meme, "/meme");
                else if (_t.IsMatch(w)) Set(MemeType.Top,  "/top" );
                else if (_g.IsMatch(w)) Set(MemeType.Dg,   "/dg"  );
                else if (_d.IsMatch(w)) Set(MemeType.Dp,   "/dp"  );
                else if (_s.IsMatch(w)) Set(MemeType.Snap, "/snap");
                else if (_n.IsMatch(w)) Set(MemeType.Nuke, "/nuke");
                else if (_u.IsMatch(w)) Set(MemeType.Auto, "*");
                else Bot.SendMessage(Origin, string.Format(SET_MEME_TYPE_MANUAL, w));

                void Set(MemeType type, string command)
                {
                    if      (args.Length > 1 && args[1] == "?") // /set m ?
                    {
                        var commandOptions = Data.GetOrCreateMemeOptions()[type];
                        var options = commandOptions is null
                            ? "А НЕТУ!!!"
                            : $"<code>{commandOptions}</code>";
                        var message = command == "*"
                            ? $"Опции команды <b>{command}</b>: {options}"
                            : $"Опции авто-обработки: {options}";
                        Bot.SendMessage(Origin, message.XDDD());
                    }
                    else if (args.Length > 1 && command == "*") // /set a p:scale 0.5; v:nuke; a:peg rip! 3; u:songcs
                    {
                        var args_ = Args.SplitN(2);
                        if (args_.Length < 2)
                        {
                            Bot.SendMessage(Origin, SET_AUTO_HANDLER_MANUAL);
                            return;
                        }

                        AutoHandler.ClearCache(Origin.Chat);

                        var result = args_[1] == "0"
                            ? null
                            : args_[1];
                        Data.GetOrCreateMemeOptions()[type] = result;
                        if (result is null && (Data.Options?.IsEmpty() ?? false)) Data.Options = null;
                        result ??= "😴";
                        ReportAutoHandlerSet(result);
                    }
                    else if (args.Length > 1) // /set m largmm!!420"!!
                    {
                        var result = args[1] == "0" 
                            ? null 
                            : args[1];
                        Data.GetOrCreateMemeOptions()[type] = result;
                        if (result is null && (Data.Options?.IsEmpty() ?? false)) Data.Options = null;
                        result = $"{command}{result}";
                        ReportOptionsSet(command, result);
                    }
                    else // /set m
                    {
                        Data.Type = type;
                        ReportTypeSet(command);
                    }
                }

                void ReportTypeSet(string command)
                {
                    ChatService.SaveChatsDB();
                    var message = command == "*"
                        ? SET_AUTO_HANDLER_RESPONSE
                        : string.Format(SET_MEMES_RESPONSE, ChatInfo.Types[Data.Type]);
                    Bot.SendMessage(Origin, message.XDDD());
                    Log($"{Title} >> MEMES TYPE >> {Data.Type.ToString()[0]}");
                }

                void ReportOptionsSet(string command, string result)
                {
                    ChatService.SaveChatsDB();
                    Bot.SendMessage(Origin, string.Format(SET_MEME_OPS_RESPONSE, command, result).XDDD());
                    Log($"{Title} >> MEMES OPTIONS >> {result}");
                }
                
                void ReportAutoHandlerSet(string handler)
                {
                    ChatService.SaveChatsDB();
                    Bot.SendMessage(Origin, string.Format(SET_AUTO_HANDLER_OPTIONS_RESPONSE, handler).XDDD());
                    Log($"{Title} >> AUTO HANDLER SET >> {handler}");
                }
            }
            else
                Bot.SendMessage(Origin, SET_MANUAL);
        }
    }
}