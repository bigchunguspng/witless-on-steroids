using PF_Bot.Features_Aux.Packs;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Messages.Commands;

namespace PF_Bot.Commands.Admin.Fun;

public class Spam : CommandHandlerBlocking_Admin
{
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

        var request = ChatSelector.ParseOptions(Options);
        var bakas   = ChatSelector.GetChats(request);

        var chat = Chat;
        var text = Args!;

        Bot.SendMessage(Origin, $"Spamming to {bakas.Count} chats… 😙");

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

    private static void LogSpam(long chat) => Log($"SPAM >> {chat}", LogLevel.Info, LogColor.Yellow);
}

public static class ChatSelector
{
    public enum Type { All, OnlyGroups, OnlyPrivates }

    public record ChatSelectorRequest(Type type, ComparisonExpression size, ComparisonExpression days);

    public record ComparisonExpression(string? Operator, int Value);

    private static readonly Regex
        _rgx_days = new(@"a(>|<|>=|<=)(\d+)",        RegexOptions.Compiled),
        _rgx_size = new(@"s(>|<|>=|<=)(\d+)([km])?", RegexOptions.Compiled);

    public static ChatSelectorRequest ParseOptions(string options)
    {
        var onlyGroups   = options.Contains('g');
        var onlyPrivates = options.Contains('p');

        var type
            = onlyGroups   ? Type.OnlyGroups
            : onlyPrivates ? Type.OnlyPrivates
            :                Type.All;
        var matchDays = _rgx_days.Match(options);
        var matchSize = _rgx_size.Match(options);
        var daysOperator = matchDays.ExtractGroup(1, s => s);
        var sizeOperator = matchSize.ExtractGroup(1, s => s);
        var daysValue    = matchDays.ExtractGroup(2, int.Parse);
        var sizeValue    = matchSize.ExtractGroup(2, int.Parse);
        var sizeUnits    = matchSize.ExtractGroup(3, s => s switch
        {
            "k" => 1024,
            "m" => 1024 * 1024,
            _   => 1
        }, 1);

        var size = new ComparisonExpression(sizeOperator, sizeValue * sizeUnits);
        var days = new ComparisonExpression(daysOperator, daysValue);

        return new ChatSelectorRequest(type, size, days);
    }

    public static List<long> GetChats(ChatSelectorRequest request)
    {
        var (type, size, days) = request;

        return ChatManager.Chats.Lock(x => x.Keys.Where(chat =>
        {
            var path = PackManager.GetPackPath(chat);
            if (File.Exists(path))
            {
                var file = new FileInfo(path);
                var typeMathes
                    =  type is Type.All
                    || type is Type.OnlyPrivates && chat.ChatIsPrivate()
                    || type is Type.OnlyGroups   && chat.ChatIsPrivate().Janai();
                var sizeMatches = size.Operator switch
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
                return typeMathes && sizeMatches && daysMatches;
            }

            return false;
        }).ToList());
    }
}