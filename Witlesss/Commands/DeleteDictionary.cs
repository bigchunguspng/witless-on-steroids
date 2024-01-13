using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Witlesss.Commands
{
    public class DeleteDictionary : Move
    {
        protected override void ExecuteAuthorized()
        {
            Bot.SendMessage(Chat, TRACTOR_GAME_RULES, GetMinigameKeyboard());
        }

        private void DeleteTheDictionary()
        {
            SetBaka(Bot.SussyBakas[Chat]);
            
            string name = ValidFileName(Title.Split()[0]);
            string result = MoveDictionary(name);

            if (result == "*") result = "*👊 никак*";

            Bot.RemoveChat(Chat);
            Bot.SaveChatList();

            Baka.DeleteForever();

            DropBaka();

            Log($"{Title} >> DIC REMOVED >> {Chat}", ConsoleColor.Magenta);
            Bot.SendMessage(Chat, string.Format(DEL_SUCCESS_RESPONSE, Title, result));
        }


        private const string _default = "🚧", _house = "🏠", _destruction = "🏚", _bricks = "🧱", _tractor = "🚜", _tnt = "🧨", _boom = "💥";

        private readonly Dictionary<long, List<List<InlineKeyboardButton>>> _games = new();

        private InlineKeyboardMarkup GetMinigameKeyboard()
        {
            var cell = InlineKeyboardButton.WithCallbackData(_default);
            var rows = new List<List<InlineKeyboardButton>>() { Line(), Line(), Line(), Line() };

            AddGameObjects(_house, 3);
            AddGameObjects(_tractor, 1);
            AddGameObjects(_tnt, 2);

            _games[Chat] = rows;
            
            return new InlineKeyboardMarkup(rows);


            List<InlineKeyboardButton> Line() => new() { cell, cell, cell, cell };

            void AddGameObjects(string obj, int count)
            {
                for (int i = 0; i < count;)
                {
                    var x = Extension.Random.Next(4);
                    var y = Extension.Random.Next(4);
                    if (rows[x][y].Text == _default)
                    {
                        rows[x][y] = InlineKeyboardButton.WithCallbackData(obj, $"del - {obj} - {x}:{y}");
                        i++;
                    }
                }
            }
        }

        public void DoGameStep(long chat, string data, int message)
        {
            var s = data.Split(" - ");
            var n = s[1].Split(':');
            var o = s[0];
            var x = int.Parse(n[0]);
            var y = int.Parse(n[1]);

            var game = _games[chat];

            if (o == _tractor)
            {
                var move = Extension.Random.Next(2) == 0 ? 1 : -1;
                var vert = Extension.Random.Next(2) == 0;

                var tx = vert ? (x + move + 4) % 4 : x;
                var ty = vert ? y : (y + move + 4) % 4;

                var target = game[tx][ty].Text;
                if (target == _default || target == _bricks)
                {
                    game[tx][ty] = InlineKeyboardButton.WithCallbackData(_tractor, $"del - {_tractor} - {tx}:{ty}");
                    game[x][y] = InlineKeyboardButton.WithCallbackData(_default);
                }
                else if (target == _house)
                {
                    game[tx][ty] = InlineKeyboardButton.WithCallbackData(_destruction);
                }
                else if (target == _destruction)
                {
                    game[tx][ty] = InlineKeyboardButton.WithCallbackData(_bricks);
                }
                else if (target == _tnt)
                {
                    ExplodeTNT(tx, ty);
                }
                else
                    return;
            }
            else if (o == _tnt)
            {
                ExplodeTNT(x, y);
            }
            else
                return;

            void ExplodeTNT(int x, int y)
            {
                game[x][y] = InlineKeyboardButton.WithCallbackData(_boom);

                for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                {
                    var tx = (x - 1 + i + 4) % 4;
                    var ty = (y - 1 + j + 4) % 4;
                    var target = game[tx][ty].Text;
                    
                    if (target == _house || target == _destruction)
                    {
                        game[tx][ty] = InlineKeyboardButton.WithCallbackData(_bricks);
                    }
                    else if (target == _tnt || target == _tractor) ExplodeTNT(tx, ty);
                }
            }

            Bot.EditMessage(chat, message, TRACTOR_GAME_RULES, new InlineKeyboardMarkup(game));

            var objects = game.SelectMany(x => x.ToArray()).Select(x => x.Text).ToArray();

            if (objects.Contains(_boom))
            {
                Task.Delay(500).Wait();
                for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                {
                    if (game[i][j].Text == _boom) 
                        game[i][j] = InlineKeyboardButton.WithCallbackData(_default);
                }
                Bot.EditMessage(chat, message, TRACTOR_GAME_RULES, new InlineKeyboardMarkup(game));
            }

            var noHousing = !objects.Contains(_house);
            var noTractor = !objects.Contains(_tractor);

            if (noHousing || noTractor)
            {
                Task.Delay(1000).Wait();
                _games.Remove(chat);

                if (noHousing && noTractor)
                {
                    Bot.SendSticker(chat, new InputOnlineFile(GG));
                    Bot.SendMessage(chat, "НИЧЬЯ");
                }
                else if (noTractor)
                {
                    Bot.SendSticker(chat, new InputOnlineFile(I_WIN));
                    Bot.SendMessage(chat, "RIP 🤣😭😂👌");
                }
                else
                {
                    Bot.SendSticker(chat, new InputOnlineFile(U_WIN));
                    Bot.SendSticker(chat, new InputOnlineFile(D_100));

                    DeleteTheDictionary();
                }
            }
        }

        private const string U_WIN = "CAACAgIAAx0CW-fiGwABBCd4ZaKqPINFugduA9_nLrLPrMYTNYYAAl4pAAL9lYhLKdDUtdY2q940BA";
        private const string I_WIN = "CAACAgIAAx0CW-fiGwABBCd6ZaKqa6CruL6nMqAhzoZcmZFfE_UAAo8sAAJAVolLq24cb6DdB580BA";
        private const string GG    = "CAACAgIAAxkBAAECbcplosGS9yZvxuT57ScUu7Njh2sujQAC4iAAAqC_WEqbDIcoiRMHFDQE";
        private const string D_100 = "CAACAgIAAxkBAAECbW1lorIXFnR08kG7nlgOrIW1exeKDQACuCEAApyYMEqx8_D2SJDpWzQE";
    }
}