using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Messaging;
using Witlesss.Services.Internet.Reddit;

namespace Witlesss
{
    public class ConsoleUI
    {
        private long   _active;
        private string? _input;

        private Bot Bot => Bot.Instance;

        private BanHammer Thor => Bot.ThorRagnarok;
        private ChatList SussyBakas => ChatsDealer.SussyBakas;
        private Witless Active => SussyBakas[_active];

        private IEnumerable<Witless> Bakas => SussyBakas.Values;

        public static bool LoggedIntoReddit = false;


        public void EnterConsoleLoop()
        {
            do
            {
                _input = Console.ReadLine();
                try
                {
                    if (_input != null && !_input.EndsWith("_"))
                    {
                        if      (_input.StartsWith("+") && _input.Length > 1) SetActiveChat();
                        else if (_input.StartsWith("/")                     ) DoConsoleCommands();
                    }
                }
                catch
                {
                    Log(">:^< u/stupid >:^<", ConsoleColor.Yellow);
                }
            } while (_input != "s");
            ChatsDealer.SaveBakasBeforeExit();
            if (LoggedIntoReddit) RedditTool.Instance.SaveExcluded();
        }

        private void DoConsoleCommands()
        {
            if (_input is null) return;

            if      (BotWannaSpeak()) BreakFourthWall();
            else if (_input == "/"  ) Log(CONSOLE_MANUAL, ConsoleColor.Yellow);
            else if (_input == "/s" ) ChatsDealer.SaveBakas();
            else if (_input == "/db") DeleteBlockers();
            else if (_input == "/DB") DeleteBlocker();
            else if (_input == "/ds") DeleteBySize();
            else if (_input == "/cc") ClearTempFiles();
            else if (_input == "/cp") ClearDic(Active);
            else if (_input == "/l" ) ActivateLastChat();
            else if (_input == "/b" ) Thor.  BanChat(_active);
            else if (_input == "/ub") Thor.UnbanChat(_active);
            else if (_input.StartsWith("/ds") && _input.HasIntArgument(out var a1)) DeleteBySize(a1);
            else if (_input.StartsWith("/b" ) && _input.HasIntArgument(out var a2)) Thor.BanChat(_active, a2);
        }

        private bool BotWannaSpeak() => Regex.IsMatch(_input!, @"^\/[aw] ");

        private void SetActiveChat()
        {
            string shit = _input![1..];
            foreach (long chat in SussyBakas.Keys)
            {
                if (chat.ToString().EndsWith(shit))
                {
                    _active = chat;
                    Log($"ACTIVE CHAT >> {_active}");
                    break;
                }
            }
        }

        private void BreakFourthWall()
        {
            string text = _input!.Split (' ', 2)[1];
            if (!ChatsDealer.WitlessExist(_active)) return;

            if      (_input.StartsWith("/a ") && Active.Eat(text, out var eaten)) // add
            {
                foreach (var line in eaten) Log($"{_active} << {line}", ConsoleColor.Yellow);
            }
            else if (_input.StartsWith("/w "))                                  // write
            {
                Bot.SendMessage(_active, text, preview: true);
                Active.Eat(text);
                Log($"{_active} >> {text}", ConsoleColor.Yellow);
            }
        }

        private void ActivateLastChat()
        {
            var context = Bot.Router.Context;
            _active = context.Chat;
            Log($"ACTIVE CHAT >> {_active} ({context.Title})");
        }

        private static void ClearDic(Witless witless)
        {
            witless.BackupAndDelete();
        }

        private void DeleteBlockers()
        {
            foreach (var w in Bakas) if (DeleteBlocker(w) == -1) ChatsDealer.RemoveChat(w.Chat);
            ChatsDealer.SaveChatList();
        }
        private void DeleteBlocker()
        {
            if (DeleteBlocker(Active) == -1)
            {
                ChatsDealer.RemoveChat(_active);
                ChatsDealer.SaveChatList();
            }
        }
        private int DeleteBlocker(Witless witless)
        {
            var x = Bot.PingChat(witless.Chat, notify: false);
            if (x == -1) witless.BackupAndDelete();
            else Bot.Client.DeleteMessageAsync(witless.Chat, x);

            return x;
        }

        private void DeleteBySize(int size = 2)
        {
            foreach (var witless in Bakas)
            {
                if (SizeInBytes(witless.FilePath) > size) continue;

                witless.BackupAndDelete();
                ChatsDealer.RemoveChat(witless.Chat);
            }
            ChatsDealer.SaveChatList();
        }
    }
}