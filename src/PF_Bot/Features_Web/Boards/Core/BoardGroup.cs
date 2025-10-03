namespace PF_Bot.Features_Web.Boards.Core;

public class BoardGroup
{
    public string Title = null!;
    public bool IsNSFW;
    public readonly List<Board> Boards = [];

    public record Board(string Title, string? Key, string URL, bool IsNSFW);
}