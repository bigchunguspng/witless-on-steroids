using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Witlesss.Commands.Packing
{
    public class Delete : Move
    {
        protected override void RunAuthorized()
        {
            Bot.SendMessage(Chat, TRACTOR_GAME_RULES, GetMinigameKeyboard());
        }

        private void DeleteTheDictionary()
        {
            var name = ValidFileName(Title.Replace(' ', '-'));
            var result = MoveDictionary(name, Chat);

            if (result == "*") result = "*👊 никак*";

            ChatsDealer.RemoveChat(Chat);
            ChatsDealer.SaveChatList();

            Baka.DeleteForever();

            Log($"{Title} >> DIC REMOVED >> {Chat}", ConsoleColor.Magenta);
            Bot.SendMessage(Chat, string.Format(DEL_SUCCESS_RESPONSE, Title, result, Bot.Username));
        }


        private const string _default = "🚧", _house = "🏠", _detroit = "🏚", _bricks = "🧱"; 
        private const string _tractor = "🚜", _tnt = "🧨", _boom = "💥", _fire = "🔥";

        private readonly Dictionary<long, List<List<InlineKeyboardButton>>> _games = new();

        private List<List<InlineKeyboardButton>> _game = null!;

        private InlineKeyboardMarkup GetMinigameKeyboard()
        {
            var cell = InlineKeyboardButton.WithCallbackData(_default);
            var rows = new List<List<InlineKeyboardButton>> { Line(), Line(), Line(), Line() };

            AddGameObjects(_house, 3, data: false);
            AddGameObjects(_tractor, 1);
            AddGameObjects(_tnt, 2);

            _games[Chat] = rows;
            
            return new InlineKeyboardMarkup(rows);


            List<InlineKeyboardButton> Line() => [cell, cell, cell, cell];

            void AddGameObjects(string obj, int count, bool data = true)
            {
                for (int i = 0; i < count;)
                {
                    var x = Random.Shared.Next(4);
                    var y = Random.Shared.Next(4);
                    if (rows[x][y].Text == _default)
                    {
                        rows[x][y] = data ? GetCallbackButton(x, y, obj) : InlineKeyboardButton.WithCallbackData(obj);
                        i++;
                    }
                }
            }
        }

        public void DoGameStep(Message message, string data)
        {
            Context = WitlessContext.FromMessage(message, ChatsDealer.SussyBakas[message.Chat.Id]);

            var split = data.Split(" - ");
            var num = split[1].Split(':');
            var obj = split[0];
            var x = int.Parse(num[0]);
            var y = int.Parse(num[1]);

            _game = _games[Chat];

            // GAME LOGIC

            if (obj == _tractor)
            {
                var move = Random.Shared.Next(2) == 0 ? 1 : -1;
                var vert = Random.Shared.Next(2) == 0;

                var tx = vert ? x : (x + move + 4) % 4;
                var ty = vert ? (y + move + 4) % 4 : y;
                var target = GetCell(tx, ty);

                if (target == _default || target == _bricks)
                {
                    SetCell(tx, ty, GetCallbackButton(tx, ty, _tractor));
                    SetCell(x, y, _default);
                }
                else if (target == _house)   SetCell(tx, ty, _detroit);
                else if (target == _detroit) SetCell(tx, ty, _bricks);
                else if (target == _tnt)     Explode(tx, ty);
                else
                    return;
            }
            else if (obj == _tnt) Explode(x, y);
            else
                return;

            Bot.EditMessage(Chat, message.MessageId, TRACTOR_GAME_RULES, new InlineKeyboardMarkup(_game));

            // EXPLOSION ANIMATION (IF ANY)

            var objects = _game.SelectMany(row => row.ToArray()).Select(cell => cell.Text).ToArray();

            if (objects.Contains(_boom) || objects.Contains(_fire))
            {
                Task.Delay(500).Wait();
                for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                {
                    var cell = GetCell(i, j);
                    if (cell == _boom || cell == _fire) SetCell(i, j, _default);
                }
                Bot.EditMessage(Chat, message.MessageId, TRACTOR_GAME_RULES, new InlineKeyboardMarkup(_game));
            }

            // MATCH RESULTS

            var noHousing = !objects.Contains(_house);
            var noTractor = !objects.Contains(_tractor);

            if (noHousing || noTractor)
            {
                Task.Delay(1000).Wait();
                _games.Remove(Chat);

                if (noHousing && noTractor)
                {
                    Bot.SendSticker(Chat, new InputOnlineFile(GG));
                    Bot.SendMessage(Chat, "НИЧЬЯ");
                }
                else if (noTractor)
                {
                    Bot.SendSticker(Chat, new InputOnlineFile(I_WIN));
                    Bot.SendMessage(Chat, "RIP 🤣😭😂👌");
                }
                else
                {
                    Bot.SendSticker(Chat, new InputOnlineFile(U_WIN));
                    Bot.SendSticker(Chat, new InputOnlineFile(D_100));

                    DeleteTheDictionary();
                }
            }
        }

        private InlineKeyboardButton GetCallbackButton(int x, int y, string obj)
        {
            return InlineKeyboardButton.WithCallbackData(obj, $"del - {obj} - {x}:{y}");
        }

        private string GetCell(int x, int y) => _game[x][y].Text;

        private void SetCell(int x, int y, InlineKeyboardButton button)
        {
            _game[x][y] = button;
        }

        private void SetCell(int x, int y, string obj)
        {
            _game[x][y] = InlineKeyboardButton.WithCallbackData(obj);
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

        private const string U_WIN = "CAACAgIAAx0CW-fiGwABBCd4ZaKqPINFugduA9_nLrLPrMYTNYYAAl4pAAL9lYhLKdDUtdY2q940BA";
        private const string I_WIN = "CAACAgIAAx0CW-fiGwABBCd6ZaKqa6CruL6nMqAhzoZcmZFfE_UAAo8sAAJAVolLq24cb6DdB580BA";
        private const string GG    = "CAACAgIAAxkBAAECbcplosGS9yZvxuT57ScUu7Njh2sujQAC4iAAAqC_WEqbDIcoiRMHFDQE";
        private const string D_100 = "CAACAgIAAxkBAAECbW1lorIXFnR08kG7nlgOrIW1exeKDQACuCEAApyYMEqx8_D2SJDpWzQE";
    }
}