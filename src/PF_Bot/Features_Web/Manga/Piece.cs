using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Web.Manga;

// /piece                 SHORT MAN
// /piece titles          list
// /piece titlekey        list chapers
// /piece titlekey? N     dl chaper cbz
// /man_piece             FULL MAN
public class /* One */ Piece : CommandHandlerAsync // 🍖
{
    protected override async Task Run()
    {
        if (Args != null)
        {
            if (Args == "info")
            {
                await ListTitles();
            }
            else if (Args.CanBeSplitN())
            {
                var bits = Args.SplitN(2);
                var title   = bits[0];
                var chapter = bits[1];

                await DownloadChapter(title, chapter);
            }
            else
                await ListChapters(title: Args);
        }
        else
            SendManual(PIECE_MANUAL);
    }

    private static readonly TCB_Scans_Client _client = new();

    private async Task ListTitles()
    {
        // https://tcbonepiecechapters.com/projects
        // f/e:
        // one-piece - One Piece | code - a
        // https://tcbonepiecechapters.com/mangas/5/one-piece

        // todo - show paginated, cache for 15 min

        Bot.SendMessage(Origin, "<i>фича в разработке</i>");
    }

    // One Piece = one-piece | one-pi | one | 5 | . // = full / part / number / one-piece is default

    private async Task ListChapters(string title)
    {
        // https://tcbonepiecechapters.com/mangas/5/one-piece
        // 1162 - God Valley Battle Royale | code - a
        // https://tcbonepiecechapters.com/chapters/7899/one-piece-chapter-1162

        // todo - show paginated, cache for 15 min

        Bot.SendMessage(Origin, "<i>фича в разработке</i>");
    }

    private async Task DownloadChapter(string title, string number)
    {
        if (title == ".") title = "one-piece";

        var titleURL = await _client.GetTitleURL(title);
        if (titleURL == null)
        {
            SetBadStatus();
            var text = PIECE_MANGA_NOT_FOUND.Format(FAIL_EMOJI.PickAny());
            Bot.SendMessage(Origin, text);
            return;
        }

        var chapterInfo = await _client.GetChapterInfo(titleURL, number);
        if (chapterInfo == null)
        {
            SetBadStatus();
            var code = title.Substring(title.LastIndexOf('/') + 1);
            var text = PIECE_CHAPTER_NOT_FOUND.Format(FAIL_EMOJI.PickAny(), code);
            Bot.SendMessage(Origin, text);
            return;
        }

        MessageToEdit = Bot.PingChat(Origin, PLS_WAIT[Random.Shared.Next(5)]);

        var pageURLs = await _client.GetPageURLs(chapterInfo.URL);
        var path = await new DownloadChapterCbzTask(pageURLs, title, number).Run();

        Bot.DeleteMessageAsync(Chat, MessageToEdit);

        await using var stream = File.OpenRead(path);
        Bot.SendDocument(Origin, InputFile.FromStream(stream, GetCbzName(chapterInfo)));
        Log($"{Title} >> {chapterInfo.MangaTitle} ch. {chapterInfo.Number}");
    }

    private string GetCbzName
        (TCB_Scans_Client.Chapter c) => c.ChapterTitle == null
        ? $"{c.MangaTitle} ch. {c.Number}.cbz"
        : $"{c.MangaTitle} ch. {c.Number} - {c.ChapterTitle}.cbz";
}