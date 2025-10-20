using System.Collections.Frozen;

namespace PF_Tools.Backrooms.Types.Collections;

public record struct CommandMapping<T>(string Command, T Handler);

public class CommandRegistry<T>(FrozenDictionary<char, List<CommandMapping<T>>> registry)
{
    public T? Resolve
        (string? input, int offset = 0)
    {
        if (input != null && registry.TryGetValue(input[offset], out var mappings))
        {
            var mapping = mappings.FirstOrDefault(x_StartsWithTextSubstring(input, offset));
            return mapping.Handler;
        }

        return default;
    }

    public T? Resolve
        (string? input, out string? command, int offset = 0)
    {
        if (input != null && registry.TryGetValue(input[offset], out var mappings))
        {
            var mapping = mappings.FirstOrDefault(x_StartsWithTextSubstring(input, offset));
            command = mapping.Command;
            return    mapping.Handler;
        }

        command = null;
        return default;
    }

    private static Func<CommandMapping<T>, bool>
        x_StartsWithTextSubstring
        (string input, int offset) =>
        x => input.AsSpan(offset).StartsWith(x.Command);

    public class Builder
    {
        private readonly List<CommandMapping<T>> _lobby = [];

        public Builder Register(string command, T handler)
        {
            _lobby.Add(new CommandMapping<T>(command, handler));

            return this;
        }

        public CommandRegistry<T> Build()
        {
            var registry = _lobby
                .GroupBy(x => x.Command[0])
                .ToFrozenDictionary
                (
                    g => g.Key,
                    g => g.OrderByDescending(x => x.Command).ToList()
                );

            return new CommandRegistry<T>(registry);
        }
    }
}