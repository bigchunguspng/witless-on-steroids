// ReSharper disable CoVariantArrayConversion

using PF_Bot.Core;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Handlers.Edit.Direct.Helpers;

public static class DirectEditing
{
    private const string TROLLFACE = "CAACAgQAAx0CW-fiGwABBCUKZZ1tWkTgqp6spEH7zvPgyqZ3w0AAAt4BAAKrb-4HuRiqZWTyoLw0BA";

    public static async Task SendTrollface(MessageOrigin origin, bool extensionInvalid)
    {
        App.Bot.SendSticker(origin, InputFile.FromFileId(TROLLFACE));
        if (extensionInvalid)
        {
            await Task.Delay(Fortune.RandomInt(900, 1100));
            App.Bot.SendMessage(origin, PEG_EXTENSION_MISSING);
        }
    }
    
    public static bool OptionsMentionsPrivateFile(string options) =>
        options.Contains(File_Config, StringComparison.OrdinalIgnoreCase)
     || options.Contains(File_Log,    StringComparison.OrdinalIgnoreCase)
     || options.Contains(File_Errors, StringComparison.OrdinalIgnoreCase);

    //

    private static readonly Regex
        _rgx_alias = new(@"\$?([^\s\$]*)!", RegexOptions.Compiled);

    public static bool ApplyAliases(this CommandContext context, ref string options, FilePath directory)
    {
        var noAliases = options.Contains('!').Janai();
        if (noAliases) return true;

        while (true)
        {
            var match = _rgx_alias.Match(options);
            if (match.Failed()) break;
                
            if (context.ApplyAlias(match, ref options, directory).Failed()) return false;
        }

        return true;
    }

    private static bool ApplyAlias(this CommandContext context, Match aliasMatch, ref string options, FilePath directory)
    {
        var data = aliasMatch.Groups[1].Value;
        var args = data.Split(':');
        var name = args[0];
        var path = directory.Combine($"{name}.txt");

        var success = path.FileExists;
        if (success)
        {
            var aliasRender = string.Format(File.ReadAllText(path), args.Skip(1).ToArray());
            var aliasRegex = new Regex(Regex.Escape(aliasMatch.Value));
            options = aliasRegex.Replace(options, aliasRender, 1);
        }
        else
            App.Bot.SendMessage(context.Origin, string.Format(ALIAS_NOT_FOUND, name, FAIL_EMOJI.PickAny()));

        return success;
    }
}