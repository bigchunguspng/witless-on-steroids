using PF_Bot.Features_Aux.Packs;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Messages.Commands;

namespace PF_Bot.Commands.Admin.Fun;

public class Spam : CommandHandlerBlocking_Admin
{
    private readonly Regex
        _rgx_days = new(@"a(>|<|>=|<=)?(\d+)",        RegexOptions.Compiled),
        _rgx_size = new(@"s(>|<|>=|<=)?(\d+)([km])?", RegexOptions.Compiled);

    protected override void Run()
    {
        var messageId = Message.ReplyToMessage is { } reply ? reply.Id : -1;

        var textProvided = Args is not null;
        var copyProvided = messageId >= 0;

        if (!textProvided && !copyProvided)
        {
            SendManual(SPAM_MANUAL);
            return;
        }

        var onlyGroups   = Options.Contains('g');
        var onlyPrivates = Options.Contains('p');

        var type
            = onlyGroups   ? GetChatsType.OnlyGroups
            : onlyPrivates ? GetChatsType.OnlyPrivates
            :                GetChatsType.All;
        var matchDays = _rgx_days.Match(Options);
        var matchSize = _rgx_size.Match(Options);
        var daysOperator = matchDays.ExtractGroup(1, s => s);
        var sizeOperator = matchSize.ExtractGroup(1, s => s);
        var daysValue    = matchDays.ExtractGroup(2, int.Parse);
        var sizeValue    = matchSize.ExtractGroup(2, int.Parse);
        var sizeUnits    = matchSize.ExtractGroup(3, s => s is "k" ? 1024 : s is "m" ? 1024 * 1024 : 1, 1);

        var size = new ComparisonExpression(sizeOperator, sizeValue * sizeUnits);
        var days = new ComparisonExpression(daysOperator, daysValue);
        var bakas = GetChats(type, size, days);

        var chat = Chat;
        var text = Args!;

        if (textProvided) Task.Run(() => SendSpam(bakas, text));
        else              Task.Run(() => CopySpam(bakas, chat, messageId));
    }

    private static void SendSpam(IEnumerable<long> chats, string text)
    {
        foreach (var chat in chats)
        {
            Bot.SendMessage(chat, text, preview: true);
            LogSpam(chat);
        }
    }

    private static void CopySpam(IEnumerable<long> chats, long fromChat, int messageId)
    {
        foreach (var chat in chats)
        {
            Bot.CopyMessage(chat, fromChat, messageId);
            LogSpam(chat);
        }
    }

    private enum GetChatsType { All, OnlyGroups, OnlyPrivates }

    private record ComparisonExpression(string? Operator, int Value);

    private static IEnumerable<long> GetChats(GetChatsType type, ComparisonExpression size, ComparisonExpression days)
    {
        return ChatManager.Chats.Lock(x => x.Keys.Where(chat =>
        {
            var path = PackManager.GetPackPath(chat);
            if (File.Exists(path))
            {
                var file = new FileInfo(path);
                var typeMathes
                    = type is GetChatsType.All
                   || type is GetChatsType.OnlyPrivates && chat.ChatIsPrivate()
                   || type is GetChatsType.OnlyGroups   && chat.ChatIsPrivate().Janai();
                var sizeMathces = size.Operator switch
                {
                    ">"  => file.Length >  size.Value,
                    "<"  => file.Length <  size.Value,
                    ">=" => file.Length >= size.Value,
                    "<=" => file.Length <= size.Value,
                    _    => true
                };
                var timeOfInactivity = DateTime.Now - file.LastWriteTime;
                var time = TimeSpan.FromDays(days.Value);
                var daysMatches = days.Operator switch
                {
                    ">"  => timeOfInactivity >  time,
                    "<"  => timeOfInactivity <  time,
                    ">=" => timeOfInactivity >= time,
                    "<=" => timeOfInactivity <= time,
                    _    => true
                };
                return typeMathes && sizeMathces && daysMatches;
            }

            return false;
        }));
    }

    private static void LogSpam(long chat) => Log($"SPAM >> {chat}", LogLevel.Info, LogColor.Yellow);
}