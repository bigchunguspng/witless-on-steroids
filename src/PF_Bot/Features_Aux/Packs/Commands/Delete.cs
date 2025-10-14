using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Commands;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Features_Aux.Packs.Commands;

public class Delete : CommandHandlerAsync_SettingsBlocking
{
    protected override void RunAuthorized()
    {
        Bot.SendMessage(Origin, TRACTOR_GAME_RULES, TractorGame.NewGameKeyboard(Chat));
    }
}

public class Delete_Callback : CallbackHandler
{
    protected override async Task Run()
    {
        var parts = Content.Split(" - ");
        var obj = parts[0];
        var xy  = parts[1].Split(':');
        var x = int.Parse(xy[0]);
        var y = int.Parse(xy[1]);

        var input = new TractorGame.StepInput(obj, x, y);
        var game  = new TractorGame(GetGameBoard());

        void UpdateGameKeyboard(InlineKeyboardMarkup buttons)
            => Bot.EditMessage(Chat, Message.Id, TRACTOR_GAME_RULES, buttons);

        var result = await game.DoStep(input, UpdateGameKeyboard);
        if (result == TractorGame.StepResult.PASS) return;

        await Task.Delay(1000);
        TractorGame.Games.Remove(Chat);

        if      (result == TractorGame.StepResult.DRAW)
        {
            Bot.SendSticker(Origin, InputFile.FromFileId(GG));
            Bot.SendMessage(Origin, "НИЧЬЯ");
            Log($"{Title} >> DELETE >> 1:1");
        }
        else if (result == TractorGame.StepResult.LOSE)
        {
            Bot.SendSticker(Origin, InputFile.FromFileId(I_WIN));
            Bot.SendMessage(Origin, "RIP 🤣😭😂👌");
            Log($"{Title} >> DELETE >> RIP BOZO LMAO");
        }
        else
        {
            Bot.SendSticker(Origin, InputFile.FromFileId(U_WIN));
            Bot.SendSticker(Origin, InputFile.FromFileId(D_100));
            Log($"{Title} >> DELETE >> IT'S OVER :(");

            DeleteChat();
        }
    }

    private List<List<InlineKeyboardButton>> GetGameBoard()
    {
        if (TractorGame.Games.TryGetValue_Failed(Chat, out var board))
        {
            board = Message.ReplyMarkup!.InlineKeyboard.Select(x => x.ToList()).ToList();
            TractorGame.Games.Add(Chat, board);
        }

        return board;
    }

    // /move, /pub - just clear the pack
    // /delete = /move + remove chat from chatlist
    private void DeleteChat()
    {
        var result = PackManager.Move(Chat, name: Title, publish: false) ?? "*👊 никак*";

        ChatManager.Remove(Chat);
        ChatManager.SaveChats();

        Log($"{Title} >> DIC REMOVED >> {Chat}", LogLevel.Info, LogColor.Fuchsia);
        Bot.SendMessage(Origin, DEL_SUCCESS_RESPONSE.Format(Title, result, Bot.Username));
    }

    private const string
        U_WIN = "CAACAgIAAx0CW-fiGwABBCd4ZaKqPINFugduA9_nLrLPrMYTNYYAAl4pAAL9lYhLKdDUtdY2q940BA",
        I_WIN = "CAACAgIAAx0CW-fiGwABBCd6ZaKqa6CruL6nMqAhzoZcmZFfE_UAAo8sAAJAVolLq24cb6DdB580BA",
        GG    = "CAACAgIAAxkBAAECbcplosGS9yZvxuT57ScUu7Njh2sujQAC4iAAAqC_WEqbDIcoiRMHFDQE",
        D_100 = "CAACAgIAAxkBAAECbW1lorIXFnR08kG7nlgOrIW1exeKDQACuCEAApyYMEqx8_D2SJDpWzQE";
}