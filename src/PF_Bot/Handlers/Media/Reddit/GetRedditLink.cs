using PF_Bot.Core;
using PF_Bot.Core.Internet.Reddit;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Handlers.Media.Reddit;

public class GetRedditLink : SyncCommand
{
    protected override void Run()
    {
        var message = Message.ReplyToMessage;
        if (message != null)
        {
            var key = message.MediaGroupId ?? message.Format_ChatMessage();
            var post = App.Reddit.LastPosts_TryGet(key);
            if (post != null)
            {
                Bot.SendMessage(Origin, FormatPost(post));
                Log($"{Title} >> LINK TO r/{post.Subreddit}");
            }
            else
                Bot.SendMessage(Origin, $"{I_FORGOR.PickAny()} {FAIL_EMOJI.PickAny()}");
        }
        else
            Bot.SendMessage(Origin, string.Format(LINK_MANUAL, RedditApp.KEEP_POSTS));
    }

    private static string FormatPost(RedditPost p)
        => $"<b><a href='https://www.reddit.com{p.Permalink}'>r/{p.Subreddit}</a></b>";
}