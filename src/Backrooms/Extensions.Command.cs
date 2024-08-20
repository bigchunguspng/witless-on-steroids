// ReSharper disable CoVariantArrayConversion

namespace Witlesss.Backrooms;

public partial class Extensions
{
    public const string TROLLFACE = "CAACAgQAAx0CW-fiGwABBCUKZZ1tWkTgqp6spEH7zvPgyqZ3w0AAAt4BAAKrb-4HuRiqZWTyoLw0BA";

    public static readonly Regex AliasRegex = new(@"\$?([^\s\$]*)!");

    public static bool ApplyAlias(this CommandContext context, Match match, ref string options, string directory)
    {
        var data = match.Groups[1].Value;
        var args = data.Split(':');
        var name = args[0];
        var path = Path.Combine(directory, $"{name}.txt");

        var success = File.Exists(path);
        if (success)
        {
            var content = File.ReadAllText(path);
            options = options.Replace(match.Value, string.Format(content, args.Skip(1).ToArray()));
        }
        else
            Bot.Instance.SendMessage(context.Chat, string.Format(ALIAS_NOT_FOUND, name, FAIL_EMOJI_2.PickAny()));

        return success;
    }
}