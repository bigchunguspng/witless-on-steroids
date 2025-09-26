namespace PF_Bot.Routing;

public record struct CommandMapping<T>(string Command, T Handler);

public class CommandRegistry<T>(Dictionary<char, List<CommandMapping<T>>> registry)
{
    public T? Resolve(string? command)
    {
        if (command is null) return default;

        if (registry.TryGetValue(command[0], out var list))
        {
            return list.FirstOrDefault(x => command.StartsWith(x.Command)).Handler;
        }

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