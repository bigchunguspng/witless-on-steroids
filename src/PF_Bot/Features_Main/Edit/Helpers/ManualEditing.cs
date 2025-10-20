// ReSharper disable CoVariantArrayConversion

using PF_Bot.Core;
using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Main.Edit.Helpers;

public static class ManualEditing
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
     || options.Contains(Dir_Log,     StringComparison.OrdinalIgnoreCase)
     || options.Contains(Dir_DB,      StringComparison.OrdinalIgnoreCase);

    //

    private static readonly Regex
        _rgx_args  = new(@"\{(\d+)\}",      RegexOptions.Compiled),
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
        var expr = aliasMatch.Groups[1].Value;
        var bits = expr.Split(':');
        var name = bits[0];
        var path = directory.Combine($"{name}.txt");

        var success = path.FileExists;
        if (success)
        {
            var template = File.ReadAllText(path);
            var args = bits.Skip(1).ToArray();
            try
            {
                var aliasRender = template.Format(args);
                var aliasRegex = new Regex(Regex.Escape(aliasMatch.Value));
                options = aliasRegex.Replace(options, aliasRender, 1);
            }
            catch (FormatException e)
            {
                LogError($"{context.Title} >> ALIAS PARSING FAIL | {e.GetErrorMessage()}");

                var args_log = args.Length == 0 
                    ? "*пусто*" 
                    : $"[{string.Join(", ", args.Select(x => $"<code>{x}</code>"))}], {args.Length} шт.";

                var count = _rgx_args.Count(template);

                var text = ALIAS_FORMAT_FAIL.Format(FAIL_EMOJI.PickAny(), args_log, name, count, count.ED("", "а", "ов"), template);
                App.Bot.SendMessage(context.Origin, text);
                return false;
            }
        }
        else
        {
            var text = ALIAS_NOT_FOUND.Format(name, FAIL_EMOJI.PickAny());
            App.Bot.SendMessage(context.Origin, text);
        }

        return success;
    }
}