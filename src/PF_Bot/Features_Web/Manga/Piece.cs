using PF_Bot.Routing.Messages.Commands;

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

    private async Task ListTitles()
    {
        // https://tcbonepiecechapters.com/projects
        // f/e:
        // One Piece                                one-piece
        // https://tcbonepiecechapters.com/mangas/5/one-piece
    }

    // One Piece = one-piece | one-pi | one | 5 | . // = full / part / number / one-piece is default

    private async Task ListChapters(string title)
    {
        // https://tcbonepiecechapters.com/mangas/5/one-piece
        // 1162 - God Valley Battle Royale | code - a
        // https://tcbonepiecechapters.com/chapters/7899/one-piece-chapter-1162
    }

    private async Task DownloadChapter(string title, string number)
    {
        // One Piece ch. 1162 - God Valley Battle Royale.cbz
    }
}

public class TCB_Scans_Client
{
    
}