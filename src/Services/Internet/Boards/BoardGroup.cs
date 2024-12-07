namespace Witlesss.Services.Internet.Boards;

public class BoardGroup
{
    public string Title = null!;
    public bool IsNSFW;
    public readonly List<Board> Boards = [];

    public record Board(string Title, string URL, bool IsNSFW);
}