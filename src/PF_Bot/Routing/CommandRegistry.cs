namespace PF_Bot.Routing;

public class CommandRegistry<T>
{
    private                  List<(string Command, Func<T> Function)>  _lobby = [];
    private Dictionary<char, List<(string Command, Func<T> Function)>> _dictionary = new();

    public CommandRegistry<T> Register(string command, Func<T> function)
    {
        _lobby.Add((command, function));

        return this;
    }

    public CommandRegistry<T> Build()
    {
        _dictionary = _lobby
            .GroupBy(x => x.Command[0])
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x).ToList());
        _lobby = null!;

        return this;
    }

    public Func<T>? Resolve(string? command)
    {
        if (command is null) return null;

        if (_dictionary.TryGetValue(command[0], out var list))
        {
            return list.FirstOrDefault(x => command.StartsWith(x.Command)).Function;
        }

        return null;
    }
}