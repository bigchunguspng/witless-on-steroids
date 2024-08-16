using System.Text;
using Telegram.Bot.Types;
using Witlesss.Commands.Packing;

namespace Witlesss.Commands.Editing;

public class AliasFFMpeg : SyncCommand
{
    // /apeg [code] [options] // ALIAS CREATION
    // /apeg  info            // ALIAS INFO

    protected override void Run()
    {
        if (Args is "info")
        {
            SendAliasList(new ListPagination(Chat, PerPage: 10));
        }
        else if (Args is null || !Regex.IsMatch(Args, @"\s"))
        {
            Bot.SendMessage(Chat, PEG_ALIAS_SYNTAX);
        }
        else
        {
            var args = Args!.SplitN(2);
            var name = args[0].ValidFileName();

            var files = GetFiles(Dir_Alias_Peg, $"{name}.*");
            if (files.Length > 0)
            {
                var content = File.ReadAllText(files[0]);
                Bot.SendMessage(Chat, string.Format(PEG_ALIAS_EXIST_RESPONSE, name, content, FAIL_EMOJI_1.PickAny()));
            }
            else
            {
                var options = Regex.Replace(args[1], @"\s+", " ");
                File.WriteAllText(Path.Combine(Dir_Alias_Peg, $"{name}.txt"), options);
                Bot.SendMessage(Chat, string.Format(PEG_ALIAS_SAVED_RESPONSE, name));
                Log($"{Title} >> PEG ALIAS ADDED [{name}]");
            }
        }
    }


    public void HandleCallback(CallbackQuery query, string[] data)
    {
        SendAliasList(query.GetPagination(data));
    }

    private void SendAliasList(ListPagination pagination)
    {
        var (chat, messageId, page, perPage) = pagination;

        var files = GetFiles(Dir_Alias_Peg);

        var single = files.Length <= perPage;

        var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
        var sb = new StringBuilder("🔥 <b>Ярлыки команды /peg:</b> ");
        if (!single) sb.Append("📄[").Append(page + 1).Append('/').Append(lastPage + 1).Append(']');
        sb.Append("\n\n").AppendJoin('\n', AliasList(files, page, perPage));
        if (!single) sb.Append(USE_ARROWS);

        var buttons = single ? null : GetPaginationKeyboard(page, perPage, lastPage, "ap");
        Bot.SendOrEditMessage(chat, sb.ToString(), messageId, buttons);
    }

    private static IEnumerable<string> AliasList(string[] files, int page = 0, int perPage = 25)
    {
        if (files.Length == 0)
        {
            yield return "*пусто*";
            yield break;
        }

        foreach (var file in files.Skip(page * perPage).Take(perPage))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            yield return $"<code>{name}</code>:\n<blockquote>{File.ReadAllText(file)}</blockquote>";
        }
    }
}