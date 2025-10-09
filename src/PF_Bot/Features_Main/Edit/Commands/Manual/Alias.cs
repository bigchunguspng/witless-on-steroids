using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features_Aux.Listing;
using PF_Bot.Routing;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Features_Main.Edit.Commands.Manual;

public class AliasContext
{
    public required string   CommandName { get; init; }
    public required string   CallbackKey { get; init; }
    public required string   Tool        { get; init; }
    public required FilePath Directory   { get; init; }

    public static readonly AliasContext FFMpeg = new()
    {
        CommandName = "peg",
        CallbackKey = Registry.CallbackKey_AliasP,
        Tool = "FFMpeg",
        Directory = Dir_Alias_Peg,
    };

    public static readonly AliasContext Magick = new()
    {
        CommandName = "im",
        CallbackKey = Registry.CallbackKey_AliasI,
        Tool = "ImageMagick",
        Directory = Dir_Alias_Im,
    };
}

public class Alias_Callback(AliasContext ctx) : CallbackHandler
{
    protected override Task Run()
    {
        ListingAliases.SendList(ctx, Query.GetPagination(Content));
        return Task.CompletedTask;
    }
}

public class Alias(AliasContext ctx) : CommandHandlerBlocking
{
    // /a{cmd} [code] [options] // ALIAS CREATION
    // /a{cmd} [code] 0         // ALIAS DELETION (admin only)
    // /a{cmd}  info            // ALIAS INFO

    protected override void Run()
    {
        if (Args != null && Args.EndsWith("info") || Options.StartsWith("_info"))
        {
            ListingAliases.SendList(ctx, new ListPagination(Origin, PerPage: 10));
        }
        else if (Args != null && Args.CanBeSplitN())
        {
            var args = Args!.SplitN(2);
            var name = args[0].ValidFileName();

            var admin = Message.SenderIsBotAdmin();
            var files = Directory.GetFiles($"{name}.*");
            if (files.Length > 0 && admin.Janai())
            {
                Status = CommandResultStatus.BAD;
                var content = File.ReadAllText(files[0]);
                Bot.SendMessage(Origin, string.Format(ALIAS_EXIST_RESPONSE, name, content, FAIL_EMOJI.PickAny()));
            }
            else
            {
                var path = ctx.Directory.Combine($"{name}.txt");

                var options = Regex.Replace(args[1], @"\s+", " ");
                if (options == "0" && admin)
                {
                    File.Delete(path);
                    Bot.SendMessage(Origin, string.Format(ALIAS_DELETED_RESPONSE, name));
                    Log($"{Title} >> {ctx.CommandName.ToUpper()} ALIAS REMOVED [{name}]");
                }
                else
                {
                    File.WriteAllText(path, options);
                    Bot.SendMessage(Origin, string.Format(ALIAS_SAVED_RESPONSE, name));
                    Log($"{Title} >> {ctx.CommandName.ToUpper()} ALIAS ADDED [{name}]");
                }
            }
        }
        else
            SendManual(string.Format(ALIAS_SYNTAX, ctx.CommandName, ctx.Tool));
    }
}