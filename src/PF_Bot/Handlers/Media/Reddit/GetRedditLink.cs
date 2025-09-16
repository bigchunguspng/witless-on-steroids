using PF_Bot.Core.Internet.Reddit;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Handlers.Media.Reddit;

#pragma warning disable CS8509
public class GetRedditLink : AsyncCommand
{
    protected override async Task Run()
    {
        if (Message.ReplyToMessage is { } message)
        {
            var text = message.GetTextOrCaption();
            if (text != null && await Recognize(text) is { } post)
            {
                Bot.SendMessage(Origin, $"<b><a href='{post.Permalink}'>r/{post.Subreddit}</a></b>");
                Log($"{Title} >> LINK TO r/{post.Subreddit}");
            }
            else
            {
                Bot.SendMessage(Origin, $"{I_FORGOR.PickAny()} {FAIL_EMOJI.PickAny()}");
            }
        }
        else Bot.SendMessage(Origin, string.Format(LINK_MANUAL, RedditTool.KEEP_POSTS));
    }

    private Task<PostData?> Recognize(string text)
    {
        return RedditTool.Queue.Enqueue(() => RedditTool.Instance.Recognize(text));
    }
}