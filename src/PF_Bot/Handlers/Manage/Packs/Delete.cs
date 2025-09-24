using PF_Bot.Backrooms.Listing;
using PF_Bot.Core.Chats;
using PF_Bot.Core.Text;
using PF_Bot.Routing_New.Routers;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Handlers.Manage.Packs;

public class Delete : Move
{
    protected override void RunAuthorized()
    {
        Bot.SendMessage(Origin, TRACTOR_GAME_RULES, TractorGame.NewGameKeyboard(Chat));
    }
}

public class Delete_Callback : CallbackHandler
{
    private WitlessContext _ctx = null!;

    protected override async Task Run()
    {
        var message  = Query.Message!;
        var settings = ChatManager.Chats[message.Chat.Id];
        _ctx = WitlessContext.FromMessage(message, settings);

        var parts = Content.Split(" - ");
        var obj = parts[0];
        var xy  = parts[1].Split(':');
        var x = int.Parse(xy[0]);
        var y = int.Parse(xy[1]);

        var input = new TractorGame.StepInput(obj, x, y);
        var game  = new TractorGame(TractorGame.Games[_ctx.Chat]);

        void UpdateGameKeyboard(InlineKeyboardMarkup buttons)
            => Bot.EditMessage(_ctx.Chat, _ctx.Message.Id, TRACTOR_GAME_RULES, buttons);

        var result = await game.DoStep(input, UpdateGameKeyboard);
        if (result == TractorGame.StepResult.PASS) return;

        await Task.Delay(1000);
        TractorGame.Games.Remove(_ctx.Chat);

        if      (result == TractorGame.StepResult.DRAW)
        {
            Bot.SendSticker(_ctx.Origin, InputFile.FromFileId(GG));
            Bot.SendMessage(_ctx.Origin, "НИЧЬЯ");
        }
        else if (result == TractorGame.StepResult.LOSE)
        {
            Bot.SendSticker(_ctx.Origin, InputFile.FromFileId(I_WIN));
            Bot.SendMessage(_ctx.Origin, "RIP 🤣😭😂👌");
        }
        else
        {
            Bot.SendSticker(_ctx.Origin, InputFile.FromFileId(U_WIN));
            Bot.SendSticker(_ctx.Origin, InputFile.FromFileId(D_100));

            DeleteTheDictionary();
        }
    }

    // /move, /pub - just clear the pack
    // /delete = /move + remove chat from chatlist
    private void DeleteTheDictionary()
    {
        var result = PackManager.Move(_ctx.Chat, name: _ctx.Title, publish: false) ?? "*👊 никак*";

        ChatManager.Remove(_ctx.Chat);
        ChatManager.SaveChats();

        Log($"{_ctx.Title} >> DIC REMOVED >> {_ctx.Chat}", LogLevel.Info, LogColor.Fuchsia);
        Bot.SendMessage(_ctx.Origin, string.Format(DEL_SUCCESS_RESPONSE, _ctx.Title, result, Bot.Username));
    }

    private const string
        U_WIN = "CAACAgIAAx0CW-fiGwABBCd4ZaKqPINFugduA9_nLrLPrMYTNYYAAl4pAAL9lYhLKdDUtdY2q940BA",
        I_WIN = "CAACAgIAAx0CW-fiGwABBCd6ZaKqa6CruL6nMqAhzoZcmZFfE_UAAo8sAAJAVolLq24cb6DdB580BA",
        GG    = "CAACAgIAAxkBAAECbcplosGS9yZvxuT57ScUu7Njh2sujQAC4iAAAqC_WEqbDIcoiRMHFDQE",
        D_100 = "CAACAgIAAxkBAAECbW1lorIXFnR08kG7nlgOrIW1exeKDQACuCEAApyYMEqx8_D2SJDpWzQE";
}