using System;
using System.Collections.Generic;
using System.Linq;

namespace Witlesss.Services;

public class CommandRegistry<T>
{
    private List<(string Command, Func<T> Function)> _lobby = new();
    private Dictionary<char, List<(string Command, Func<T> Function)>> _dictionary = new();

    public CommandRegistry<T> Register(string command, Func<T> function)
    {
        _lobby.Add(('/' + command, function));

        return this;
    }

    public CommandRegistry<T> Build()
    {
        _dictionary = _lobby
            .GroupBy(x => x.Command[1])
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x).ToList());
        _lobby = null;

        return this;
    }

    public Func<T> Resolve(string command)
    {
        var c = command[1];
        if (_dictionary.TryGetValue(c, out var list))
        {
            return list.FirstOrDefault(x => command.StartsWith(x.Command)).Function;
        }

        return null;
    }
}