using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Handlers.Manage.Packs;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Handlers.Edit.Direct.Core;

public abstract class AliasManager : SyncCommand
{
    protected abstract string CMD       { get; }
    protected abstract string Code      { get; }
    protected abstract string Tool      { get; }

    protected abstract FilePath Directory { get; }

    // /a{cmd} [code] [options] // ALIAS CREATION
    // /a{cmd} [code] 0         // ALIAS DELETION (admin only)
    // /a{cmd}  info            // ALIAS INFO

    protected override void Run()
    {
        if (Args != null && Args.EndsWith("info") || Command != null && Command.StartsWith($"/a{CMD}_info"))
        {
            SendAliasList(new ListPagination(Origin, PerPage: 10));
        }
        else if (Args == null || Args.HasArguments().Janai())
        {
            Bot.SendMessage(Origin, string.Format(ALIAS_SYNTAX, CMD, Tool));
        }
        else
        {
            var args = Args!.SplitN(2);
            var name = args[0].ValidFileName();

            var admin = Message.SenderIsBotAdmin();
            var files = Directory.GetFiles($"{name}.*");
            if (files.Length > 0 && admin.Janai())
            {
                var content = File.ReadAllText(files[0]);
                Bot.SendMessage(Origin, string.Format(ALIAS_EXIST_RESPONSE, name, content, FAIL_EMOJI.PickAny()));
            }
            else
            {
                var path = Directory.Combine($"{name}.txt");

                var options = Regex.Replace(args[1], @"\s+", " ");
                if (options == "0" && admin)
                {
                    File.Delete(path);
                    Bot.SendMessage(Origin, string.Format(ALIAS_DELETED_RESPONSE, name));
                    Log($"{Title} >> {CMD.ToUpper()} ALIAS REMOVED [{name}]");
                }
                else
                {
                    File.WriteAllText(path, options);
                    Bot.SendMessage(Origin, string.Format(ALIAS_SAVED_RESPONSE, name));
                    Log($"{Title} >> {CMD.ToUpper()} ALIAS ADDED [{name}]");
                }
            }
        }
    }


    public void HandleCallback(CallbackQuery query, string[] data)
    {
        SendAliasList(query.GetPagination(data));
    }

    protected void SendAliasList(ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        var files = Directory.GetFiles();

        var single = files.Length <= perPage;

        var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
        var sb = new StringBuilder("🔥 <b>Ярлыки команды /").Append(CMD).Append(":</b>");
        if (single.Janai()) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', AliasList(files, page, perPage));
        if (single.Janai()) sb.Append(USE_ARROWS);

        var buttons = single ? null : GetPaginationKeyboard(page, perPage, lastPage, Code);
        Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    private IEnumerable<string> AliasList(string[] files, int page = 0, int perPage = 25)
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