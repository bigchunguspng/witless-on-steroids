using PF_Bot.Core;
using PF_Bot.Features_Aux.Listing;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Web.Manga;

public class Piece_Callback : CallbackHandler
{
    protected override async Task Run()
    {
        var pagination = GetPagination(Content);

        var c = Key[Registry.CallbackKey_Piece.Length];
        if      (c == 'm') await ListingManga.ListMangas  (pagination);
        else if (c == 'c') await ListingManga.ListChapters(pagination, await GetManga());
    }

    private static TCB_Scans_Cache Cache => TCB_Scans_Cache.Instance;

    private async Task<Manga> GetManga()
    {
        var number = Key.Substring(Key.IndexOf('-') + 1);
        var mangas = await Cache.EnsureMangasCached();
        return mangas.First(x => x.Number == number);
    }
}

/*  /piece                 SHORT GUIDE   *\
    /piece info            list
    /piece one-piece       list chapers
\*  /piece one-piece 1162  dl chaper cbz */

public class /* One */ Piece : CommandHandlerAsync // 🍖
{
    protected override async Task Run()
    {
        if      (Args == null)             SendManual(PIECE_MANUAL);
        else if (Args == "info")     await ListTitles();
        else if (Args.CanBeSplitN()) await DownloadChapter(Args.SplitN(2));
        else /* (single argument) */ await ListChapters(title: Args);
    }

    private static TCB_Scans_Cache Cache => TCB_Scans_Cache.Instance;

    private async Task ListTitles()
    {
        await ListingManga.ListMangas(new ListPagination(Origin, PerPage: 10));
    }

    private const string DEFAULT_TITLE = "one-piece";
    
    private async Task ListChapters(string title)
    {
        if (title == ".") title = DEFAULT_TITLE;

        var manga = await GetManga(title);
        if (manga == null) return;

        await ListingManga.ListChapters(new ListPagination(Origin, Page: -1, PerPage: 25), manga);
    }

    private async Task DownloadChapter(string[] args)
    {
        var title  = args[0];
        var number = args[1];

        if (title == ".") title = DEFAULT_TITLE;

        var manga = await GetManga(title);
        if (manga == null) return;

        var chapter = await GetChapter(manga, number);
        if (chapter == null) return;

        MessageToEdit = Bot.PingChat(Origin, PLS_WAIT[Random.Shared.Next(5)]);

        var pageURLs = await App.TCB.GetPageURLs(chapter.URL);
        var task = new DownloadChapterCbzTask(pageURLs, manga.Code, chapter.Number);
        var path = await task.Run();

        Bot.DeleteMessageAsync(Chat, MessageToEdit);

        await using var stream = File.OpenRead(path);
        Bot.SendDocument(Origin, InputFile.FromStream(stream, GetCbzName(chapter)));
        Log($"{Title} >> {chapter.MangaTitle} ch. {chapter.Number}");
    }

    //

    private async Task<Manga?> GetManga(string title)
    {
        var mangas = await Cache.EnsureMangasCached();
        var manga = int.TryParse(title, out _)
            ? mangas
                .FirstOrDefault(x => x.Number == title)
            : mangas
                .OrderBy(x => x.Code)
                .FirstOrDefault(x => x.Code.Contains(title, StringComparison.OrdinalIgnoreCase));

        if (manga == null)
        {
            var text = PIECE_MANGA_NOT_FOUND.Format(FAIL_EMOJI.PickAny(), title);
            SetBadStatus();
            Bot.SendMessage(Origin, text);
        }

        return manga;
    }

    private async Task<Chapter?> GetChapter
        (Manga manga, string number)
    {
        var chapters = await Cache.EnsureChaptersCached(manga);
        var chapter = chapters.FirstOrDefault(x => x.Number == number);
        if (chapter == null)
        {
            var text = PIECE_CHAPTER_NOT_FOUND.Format(FAIL_EMOJI.PickAny(), number, manga.Code);
            SetBadStatus();
            Bot.SendMessage(Origin, text);
        }

        return chapter;
    }

    private string GetCbzName
        (Chapter c) => c.ChapterTitle == null
        ? $"{c.MangaTitle} ch. {c.Number}.cbz"
        : $"{c.MangaTitle} ch. {c.Number} - {c.ChapterTitle}.cbz";
}