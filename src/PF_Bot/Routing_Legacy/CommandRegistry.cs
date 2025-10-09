namespace PF_Bot.Routing_Legacy;

public record struct CommandMapping<T>(string Command, T Handler);

public class CommandRegistry<T>(Dictionary<char, List<CommandMapping<T>>> registry)
{
    public T? Resolve
        (string? text, int offset = 0) =>
        text != null && registry.TryGetValue(text[offset], out var mappings)
            ? mappings.FirstOrDefault(x => text.AsSpan(offset).StartsWith(x.Command)).Handler
            : default;

    public T? Resolve
        (string? text, out string? command, int offset = 0)
    {
        if (text != null && registry.TryGetValue(text[offset], out var mappings))
        {
            var mapping = mappings.FirstOrDefault(x => text.AsSpan(offset).StartsWith(x.Command));
            command = mapping.Command;
            return    mapping.Handler;
        }

        command = null;
        return default;
    }

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
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.Command).ToList());

            return new CommandRegistry<T>(registry);
        }
    }
}