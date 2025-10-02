using PF_Bot.Core.Internet.Reddit;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Handlers.Media.Reddit;

public class GetRedditLink : AsyncCommand
{
    protected override async Task Run()
    {
        var message = Message.ReplyToMessage;
        if (message != null)
        {
            await TrySendLink(message.GetTextOrCaption());
        }
        else
            Bot.SendMessage(Origin, string.Format(LINK_MANUAL, RedditTool.KEEP_POSTS));
    }

    private async Task TrySendLink(string? text)
    {
        if (text != null && await CanRecognize(text) is { } post)
        {
            Bot.SendMessage(Origin, $"<b><a href='https://www.reddit.com{post.Permalink}'>r/{post.Subreddit}</a></b>");
            Log($"{Title} >> LINK TO r/{post.Subreddit}");
        }
        else
            Bot.SendMessage(Origin, $"{I_FORGOR.PickAny()} {FAIL_EMOJI.PickAny()}");
    }

    private Task<PostData?> CanRecognize(string text)
    {
        return RedditTool.Queue.Enqueue(() => RedditTool.Instance.Recognize(text));
    }
}