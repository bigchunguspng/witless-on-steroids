using PF_Bot.Backrooms.Helpers;
using PF_Bot.Handlers.Edit.Direct;
using PF_Bot.Handlers.Help;
using PF_Bot.Handlers.Manage.Packs;
using PF_Bot.Handlers.Manage.Packs.Core;
using PF_Bot.Handlers.Memes;
using PF_Bot.Routing;
using Telegram.Bot.Types;

namespace PF_Bot.Routing_New.Routers;

public interface ICallbackRouter
{
    void Route(CallbackQuery query);
}

public class CallbackRouter_Default : ICallbackRouter
{
    private readonly CommandRegistry<CallbackHandler> _registry = new ();

    public const string
        Key_Manual = "man",
        Key_Fuse   = "f", // i@!*
        Key_AliasP = "ap",
        Key_AliasI = "ai",
        Key_Boards = "b", // i_
        Key_Planks = "p", // i_
        Key_Nukes  = "n", // l
        Key_Delete = "del";

    public CallbackRouter_Default()
    {
        _registry
            .Register(Key_Manual, () => new Help_Callback())
            .Register(Key_Fuse,   () => new Fuse_Callback())
            .Register(Key_AliasP, () => new AliasManager_Callback(AliasContext.FFMpeg))
            .Register(Key_AliasI, () => new AliasManager_Callback(AliasContext.Magick))
            .Register(Key_Boards, () => new ChanEaterCore_Callback(ImageBoardContext.Chan4))
            .Register(Key_Planks, () => new ChanEaterCore_Callback(ImageBoardContext.Chan2))
            .Register(Key_Nukes,  () => new Nuke_Callback())
            .Register(Key_Delete, () => new Delete_Callback())
            .Build();
    }

    public void Route(CallbackQuery query)
    {
        if (query.Data == null) return;

        LogDebug($"[Callback] {query.From.Id,14} @ {query.Message!.Chat.Id,14} : {query.Message.Id,8} >> {query.Data}");

        var (key, content) = query.ParseData();

        if (key == null) return;

        var handler = _registry.Resolve(key);
        if (handler != null)
            handler.Invoke().Handle(query, key, content);
    }
}

public class CallbackRouter_Skip : ICallbackRouter
{
    public void Route(CallbackQuery query)
    {
        Print(query.Data ?? "-", ConsoleColor.Yellow);
    }
}