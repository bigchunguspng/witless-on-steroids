using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Features_Main.Edit.Helpers;
using PF_Bot.Routing.Messages.Auto;
using PF_Bot.Routing.Messages.Commands;

namespace PF_Bot.Features_Aux.Settings.Commands;

public class Set : CommandHandlerAsync_SettingsBlocking
{
    private readonly Regex
        _m = new("^[mм]",       RegexOptions.Compiled),
        _t = new("^[tт]",       RegexOptions.Compiled),
        _g = new("^[dд][gгvв]", RegexOptions.Compiled),
        _d = new("^[dд]",       RegexOptions.Compiled),
        _s = new("^[sс]",       RegexOptions.Compiled),
        _n = new("^[nн]",       RegexOptions.Compiled),
        _a = new("^[aа]",       RegexOptions.Compiled);

    protected override void RunAuthorized()
    {
        if (Args != null)
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
            else SendManual(SET_MEME_TYPE_MANUAL.Format(w));

            void Set(MemeType type, string command)
            {
                if      (args.Length > 1 && args[1] == "?") // /set m ?
                {
                    var commandOptions = Data.GetOrCreateMemeOptions()[type];
                    var options = commandOptions is null
                        ? "А НЕТУ!!!"
                        : $"<code>{commandOptions}</code>";
                    var message = command == "*"
                        ? $"Текущий авто-обработчик:\n\n{options}"
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
                        var bits = Args.SplitN(2);
                        if (bits[1].Contains("auto"))
                        {
                            Bot.SendSticker(Origin, ManualEditing.TROLLFACE);
                            Thread.Sleep(500);
                            SendBadNews("Не пойдёт 😎");
                            return;
                        }

                        AutoHandler.ClearCache(Origin.Chat);

                        var result = SetOrClearOptions(type, bits);
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
            SendManual(SET_MANUAL);
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
        ChatManager.SaveChats();
        var message = command == "*"
            ? SET_AUTO_HANDLER_RESPONSE.Format(GetAutoHandlerTip())
            : SET_MEMES_RESPONSE.Format(ChatInfo.GetMemeTypeName(Data.Type));
        Bot.SendMessage(Origin, message.XDDD());
        Log($"{Title} >> MEMES TYPE >> {Data.Type.ToString()[0]}");
    }

    private string GetAutoHandlerTip()
    {
        var options = Data.Options?.Auto;
        return options != null
            ? $"<blockquote expandable><b>Текущий авто-обработчик</b>:\n<code>{options}</code></blockquote>"
            : SET_AUTO_HANDLER_EMPTY_TIP;
    }

    private void ReportOptionsSet(string command, string result)
    {
        ChatManager.SaveChats();
        Bot.SendMessage(Origin, SET_MEME_OPS_RESPONSE.Format(command, result).XDDD());
        Log($"{Title} >> MEMES OPTIONS >> {result}");
    }
                
    private void ReportAutoHandlerSet(string? handler)
    {
        ChatManager.SaveChats();
        if (handler != null)
        {
            Bot.SendMessage(Origin, SET_AUTO_HANDLER_OPTIONS_RESPONSE.Format(handler).XDDD());
            Log($"{Title} >> AUTO HANDLER >> {handler}");
        }
        else
        {
            Bot.SendMessage(Origin, SET_AUTO_HANDLER_OPTIONS_CLEAR_RESPONSE.XDDD());
            Log($"{Title} >> AUTO HANDLER CLEAR");
        }
    }
}