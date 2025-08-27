using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features.Help;
using PF_Bot.Routing;
using PF_Bot.State.Chats;

namespace PF_Bot.Features.Manage.Settings
{
    public class Set : SettingsCommand
    {
        private readonly Regex _m = new("^[mм]");
        private readonly Regex _t = new("^[tт]");
        private readonly Regex _g = new("^[dд][gгvв]");
        private readonly Regex _d = new("^[dд]");
        private readonly Regex _s = new("^[sс]");
        private readonly Regex _n = new("^[nн]");
        private readonly Regex _a = new("^[aа]");

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
                else if (_a.IsMatch(w)) Set(MemeType.Auto, "*");
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
                            ? $"Текущий авто-обработчик: {options}"
                            : $"Опции команды <b>{command}</b>: {options}";
                        Bot.SendMessage(Origin, message.XDDD());
                    }
                    else if (args.Length > 1) 
                    {
                        if (args[0].Contains('!'))
                        {
                            Data.Type = type;
                            ReportTypeSet(command);
                        }

                        if (command == "*") // /set a p:scale 0.5; v:nuke; a:peg rip! 3; u:songcs
                        {
                            AutoHandler.ClearCache(Origin.Chat);

                            var result = SetOrClearOptions(type, Args.SplitN(2));
                            ReportAutoHandlerSet(result);
                        }
                        else // /set m largmm!!420"!!
                        {
                            var result = $"{command}{SetOrClearOptions(type, args)}";
                            ReportOptionsSet(command, result);
                        }
                    }
                    else // /set m
                    {
                        Data.Type = type;
                        ReportTypeSet(command);
                    }
                }
            }
            else
                Bot.SendMessage(Origin, SET_MANUAL);
        }

        private string? SetOrClearOptions(MemeType type, string[] args)
        {
            var add = args[0].Contains('+');
            var rem = args[0].Contains('-');

            var result = args[1] == "0" ? null
                : add ? $"{Data.GetOrCreateMemeOptions()[type]}{args[1]}"
                : rem ?    Data.GetOrCreateMemeOptions()[type]?.Replace(args[1], "").MakeNull_IfEmpty()
                : args[1];

            Data.GetOrCreateMemeOptions()[type] = result;
            if (result is null && (Data.Options?.IsEmpty() ?? false)) Data.Options = null;
            return result;
        }

        private void ReportTypeSet(string command)
        {
            ChatManager.SaveChatsDB();
            var message = command == "*"
                ? string.Format(SET_AUTO_HANDLER_RESPONSE, GetAutoHandlerTip())
                : string.Format(SET_MEMES_RESPONSE, ChatInfo.Types[Data.Type]);
            Bot.SendMessage(Origin, message.XDDD());
            Log($"{Title} >> MEMES TYPE >> {Data.Type.ToString()[0]}");
        }

        private string GetAutoHandlerTip()
        {
            var options = Data.Options?.Auto;
            return options != null
                ? $"Текущий авто-обработчик:\n<code>{options}</code>"
                : SET_AUTO_HANDLER_EMPTY_TIP;
        }

        private void ReportOptionsSet(string command, string result)
        {
            ChatManager.SaveChatsDB();
            Bot.SendMessage(Origin, string.Format(SET_MEME_OPS_RESPONSE, command, result).XDDD());
            Log($"{Title} >> MEMES OPTIONS >> {result}");
        }
                
        private void ReportAutoHandlerSet(string? handler)
        {
            ChatManager.SaveChatsDB();
            if (handler != null)
            {
                Bot.SendMessage(Origin, string.Format(SET_AUTO_HANDLER_OPTIONS_RESPONSE, handler).XDDD());
                Log($"{Title} >> AUTO HANDLER >> {handler}");
            }
            else
            {
                Bot.SendMessage(Origin, SET_AUTO_HANDLER_OPTIONS_CLEAR_RESPONSE.XDDD());
                Log($"{Title} >> AUTO HANDLER CLEAR");
            }
        }
    }
}