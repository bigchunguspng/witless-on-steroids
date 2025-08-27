namespace PF_Bot.Tools_Legacy.BoardScrappers;

public class BoardGroup
{
    public string Title = null!;
    public bool IsNSFW;
    public readonly List<Board> Boards = [];

    public record Board(string Title, string? Key, string URL, bool IsNSFW);
}