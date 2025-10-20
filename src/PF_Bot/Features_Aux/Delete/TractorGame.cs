using PF_Bot.Core;
using Telegram.Bot.Types.ReplyMarkups;

namespace PF_Bot.Features_Aux.Delete;

/// <i>pov: your super gaming house is getting destroyed by a tractor</i>
public class TractorGame(List<List<InlineKeyboardButton>> game)
{
    private const string
        _default = "🚧",
        _house   = "🏠",
        _detroit = "🏚",
        _bricks  = "🧱",
        _tractor = "🚜",
        _tnt     = "🧨",
        _boom    = "💥",
        _fire    = "🔥";

    // INIT

    public static readonly SyncDictionary<long, List<List<InlineKeyboardButton>>> Games = new();

    public static InlineKeyboardMarkup NewGameKeyboard(long chat)
    {
        var cell = NewButton(_default);
        var four = Enumerable.Range(0, 4).ToArray();
        var game = four.Select(_ => four.Select(_ => cell).ToList()).ToList();

        var instance = new TractorGame(game);
        instance.AddObjects(_house, 3, withCoords: false);
        instance.AddObjects(_tractor, 1);
        instance.AddObjects(_tnt, 2);

        Games[chat] = game;

        return new InlineKeyboardMarkup(game);
    }

    private void AddObjects(string obj, int count, bool withCoords = true)
    {
        for (var i = 0; i < count;)
        {
            var x = Random.Shared.Next(4);
            var y = Random.Shared.Next(4);
            if (game[x][y].Text == _default)
            {
                game[x][y] = withCoords ? NewButton(obj, x, y) : NewButton(obj);
                i++;
            }
        }
    }

    // UPDATE

    public record StepInput(string Obj, int X, int Y);
    public enum   StepResult
    {
        PASS, DRAW, /* WOULD YOU */ LOSE, /* ? *NAH, I'D */ WIN, /*😎*/
    }

    public async Task<StepResult> DoStep(StepInput input, Action<InlineKeyboardMarkup> updateKeyboard)
    {
        // Fight!

        var update = ProcessUserInput(input);
        if (update.Janai()) return StepResult.PASS;

        updateKeyboard(new InlineKeyboardMarkup(game));

        var objects = game.SelectMany(row => row.Select(cell => cell.Text)).ToArray();
        if (objects.Contains(_boom) || objects.Contains(_fire))
        {
            await Task.Delay(500);
            DissolveExplosions();

            updateKeyboard(new InlineKeyboardMarkup(game));
        }

        // Who has survived?

        var housing = objects.Contains(_house);
        var tractor = objects.Contains(_tractor);

        return tractor && housing
            ? StepResult.PASS
            : tractor.IsOff() && housing.IsOff()
                ? StepResult.DRAW
                : tractor
                    ? StepResult.WIN
                    : StepResult.LOSE;
    }

    /// Returns true if game keyboard needs to be updated.
    private bool ProcessUserInput(StepInput input)
    {
        var (obj, x, y) = input;

        if (obj == _tractor)
        {
            var move = Fortune.IsOneIn(2) ? 1 : -1; // forward | backward
            var vert = Fortune.IsOneIn(2);          // along x | along y

            var tx = vert ? x : (x + move + 4) % 4;
            var ty = vert ?     (y + move + 4) % 4 : y;

            var target = GetCell(tx, ty);
            if (target == _default || target == _bricks)
            {
                SetCell(tx, ty, NewButton(_tractor, tx, ty));
                SetCell(x, y, _default);
            }
            else if (target == _house)   SetCell(tx, ty, _detroit);
            else if (target == _detroit) SetCell(tx, ty, _bricks);
            else if (target == _tnt)     Explode(tx, ty);
            else return false;
        }
        else if (obj == _tnt) Explode(x, y);
        else return false;

        return true;
    }

    private void Explode(int x, int y, int radius = 1, bool tnt = true)
    {
        SetCell(x, y, tnt ? _boom : _fire);

        var diameter = 1 + 2 * radius;
        for (var i = 0; i < diameter; i++)
        for (var j = 0; j < diameter; j++)
        {
            var tx = (x - radius + i + 4) % 4;
            var ty = (y - radius + j + 4) % 4;
            var target = GetCell(tx, ty);

            if      (target == _house)   SetCell(tx, ty, tnt ? _bricks : _detroit);
            else if (target == _detroit) SetCell(tx, ty, tnt ? _default : _bricks);
            else if (target == _tractor) Explode(tx, ty, 0, tnt: false);
            else if (target == _tnt)     Explode(tx, ty);
        }
    }

    private void DissolveExplosions()
    {
        for (var i = 0; i < 4; i++)
        for (var j = 0; j < 4; j++)
        {
            var cell = GetCell(i, j);
            if (cell == _boom || cell == _fire) SetCell(i, j, _default);
        }
    }

    //

    private static InlineKeyboardButton NewButton
        (string obj)
        => InlineKeyboardButton.WithCallbackData(obj);

    private static InlineKeyboardButton NewButton
        (string obj, int x, int y)
        => InlineKeyboardButton.WithCallbackData(obj, $"{Registry.CallbackKey_Delete} - {obj} - {x}:{y}");

    private string GetCell
        (int x, int y)
        => game[x][y].Text;

    private void SetCell
        (int x, int y, InlineKeyboardButton button)
        => game[x][y] = button;

    private void SetCell
        (int x, int y, string obj)
        => game[x][y] = NewButton(obj);
}