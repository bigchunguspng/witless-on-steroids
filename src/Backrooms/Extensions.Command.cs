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
            var aliasRender = string.Format(File.ReadAllText(path), args.Skip(1).ToArray());
            var regex = new Regex(Regex.Escape(match.Value));
            options = regex.Replace(options, aliasRender, 1);
        }
        else
            Bot.Instance.SendMessage(context.Origin, string.Format(ALIAS_NOT_FOUND, name, FAIL_EMOJI_2.PickAny()));

        return success;
    }
}