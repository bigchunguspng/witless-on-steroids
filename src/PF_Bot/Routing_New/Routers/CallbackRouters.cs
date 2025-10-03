using PF_Bot.Backrooms.Helpers;
using PF_Bot.Features_Aux.Help.Commands;
using PF_Bot.Features_Aux.Packs.Commands;
using PF_Bot.Features_Main.Edit.Commands.Manual;
using PF_Bot.Features_Main.Memes.Commands;
using PF_Bot.Features_Web.Boards.Commands;
using PF_Bot.Routing;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Routing_New.Routers;

public interface ICallbackRouter
{
    void Route(CallbackQuery query);
}

public class CallbackRouter_Default : ICallbackRouter
{
    public const string
        Key_Manual = "man",
        Key_Fuse   = "f", // i@!*
        Key_AliasP = "ap",
        Key_AliasI = "ai",
        Key_Boards = "b", // i_
        Key_Planks = "p", // i_
        Key_Nukes  = "n", // l
        Key_Delete = "del";

    private readonly    CommandRegistry<Func<CallbackHandler>>
        _registry = new CommandRegistry<Func<CallbackHandler>>.Builder()
            .Register(Key_Manual, () => new Help_Callback())
            .Register(Key_Fuse,   () => new Fuse_Callback())
            .Register(Key_AliasP, () => new Alias_Callback(AliasContext.FFMpeg))
            .Register(Key_AliasI, () => new Alias_Callback(AliasContext.Magick))
            .Register(Key_Boards, () => new ChanEaterCore_Callback(ImageBoardContext.Chan4))
            .Register(Key_Planks, () => new ChanEaterCore_Callback(ImageBoardContext.Chan2))
            .Register(Key_Nukes,  () => new Nuke_Callback())
            .Register(Key_Delete, () => new Delete_Callback())
            .Build();

    public void Route(CallbackQuery query)
    {
        if (query.Data == null) return;

        LogDebug($"[Callback] {query.From.Id,14}.u {query.Message!.Chat.Id,14}.c {query.Message.Id,8}.M  |  {query.Data}");

        var (key, content) = query.ParseData();

        if (key == null) return;

        var handler = _registry.Resolve(key);
        if (handler != null)
            handler.Invoke().Handle(new CallbackContext(query, key, content));
    }
}

public class CallbackRouter_Skip : ICallbackRouter
{
    public void Route(CallbackQuery query)
    {
        Print(query.Data ?? "-", ConsoleColor.Yellow);
    }
}