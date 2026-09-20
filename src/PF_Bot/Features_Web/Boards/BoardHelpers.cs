using PF_Bot.Features_Web.Boards.Core;

namespace PF_Bot.Features_Web.Boards;

public static class BoardHelpers
{
    // DEBUG METHODS
    public static void Print4chan() => PrintMenu(new BoardService().GetBoardList(File_4chanHtmlPage));
    public static void Print2chan() => PrintMenu(new PlankService().GetBoardList(File_2chanHtmlPage));

    public static void PrintMenu(IEnumerable<BoardGroup> menu)
    {
        foreach (var group in menu)
        {
            Print($"{group.Title}{(group.IsNSFW ? " (18+)" : "")}");
            foreach (var board in group.Boards)
            {
                Print($"\t{board.Title,-25}{board.URL,-30}{(board.IsNSFW ? "18+" : "")}");
            }
        }
    }

    /// Checks if the filename describes a <b>single thread</b> discussion.
    /// <param name="name">filename part without date and time</param>
    public static bool FileNameIsThread(string name) => name.Contains('.') && name.Contains(".zip").Janai();
}