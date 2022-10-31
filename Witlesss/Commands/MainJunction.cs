using System;
using System.Threading;
using static Witlesss.Logger;
using static Witlesss.Strings;

namespace Witlesss.Commands
{
    public class MainJunction : Command
    {
        private Command _command;
        private readonly Demotivate _demotivate = new();
        private readonly RemoveBitrate _bitrate = new();
        private readonly ChangeSpeed _speed = new();
        private readonly Reverse _reverse = new();
        private readonly Sus _sus = new();
        private readonly Cut _cut = new();
        private readonly Fuse _fuse = new();
        private readonly Move _move = new();
        private readonly GetChatID _chatID = new();
        private readonly ChatInfo _chat = new();
        private readonly DebugMessage _debug = new();
        private readonly GenerateByFirstWord _generate = new();
        private readonly GenerateByLastWord _generateB = new();
        private readonly Buhurt _buhurt = new();
        private readonly SetFrequency _frequency = new();
        private readonly SetProbability _probability = new();
        private readonly SetQuality _quality = new();
        private readonly ToggleStickers _stickers = new();
        private readonly ToggleAdmins _admins = new();
        private readonly DeleteDictionary _delete = new();

        private string TextAsCommand() => Text.ToLower().Replace(BOT_USERNAME, "");

        private bool TextMayBeCommand(out string command)
        {
            command = TextAsCommand();
            return Text.StartsWith('/');
        }

        public override void Run()
        {
            if (Bot.WitlessExist(Chat))
            {
                var witless = Bot.SussyBakas[Chat];

                if (Text != null)
                {
                    if (TextMayBeCommand(out var command))
                    {
                        if      (command.StartsWith("/dg"))
                        {
                            _command = _demotivate;
                            _demotivate.SetMode();
                            _demotivate.PassQuality(witless);
                        }
                        else if (command.StartsWith("/dv"))
                        {
                            _command = _demotivate;
                            _demotivate.SetMode(DgMode.Wide);
                            _demotivate.PassQuality(witless);
                        }
                        else if (command.StartsWith("/a"))
                        {
                            _command = _generate;
                        }
                        else if (command.StartsWith("/fast"))
                        {
                            _command = _speed;
                            _speed.Mode = SpeedMode.Fast;
                        }
                        else if (command.StartsWith("/slow"))
                        {
                            _command = _speed;
                            _speed.Mode = SpeedMode.Slow;
                        }
                        else if (command.StartsWith("/cut"))
                        {
                            _command = _cut;
                        }
                        else if (command.StartsWith("/sus"))
                        {
                            _command = _sus;
                        }
                        else if (command == "/reverse")
                        {
                            _command = _reverse;
                        }
                        else if (command.StartsWith("/damn"))
                        {
                            _command = _bitrate;
                        }
                        else if (command.StartsWith("/zz"))
                        {
                            _command = _generateB;
                        }
                        else if (command.StartsWith("/b"))
                        {
                            _command = _buhurt;
                        }
                        else if (command.StartsWith("/set_jpg"))
                        {
                            _command = _quality;
                        }
                        else if (command.StartsWith("/set_p"))
                        {
                            _command = _probability;
                        }
                        else if (command.StartsWith("/set"))
                        {
                            _command = _frequency;
                        }
                        else if (command == "/toggle_stickers")
                        {
                            _command = _stickers;
                        }
                        else if (command == "/chat_id")
                        {
                            _command = _chatID;
                        }
                        else if (command == "/chat")
                        {
                            _command = _chat;
                        }
                        else if (command.StartsWith("/fuse"))
                        {
                            _command = _fuse;
                        }
                        else if (command.StartsWith("/move"))
                        {
                            _command = _move;
                        }
                        else if (command == "/toggle_admins")
                        {
                            _command = _admins;
                        }
                        else if (command == "/debug")
                        {
                            _command = _debug;
                        }
                        else if (command == "/delete")
                        {
                            _command = _delete;
                        }
                        else return;

                        _command.Pass(Message);
                        if (_command is WitlessCommand c) c.Pass(witless);
                        _command.Run();
                        return;
                    }
                    else
                    {
                        var sentence = Text.Clone().ToString();
                        if (witless.Eat(sentence, out string text)) Log($"{Title} >> {text}", ConsoleColor.Blue);
                    }
                }
                
                witless.Count();
                
                if (Message.Photo != null && ShouldDemotivate())
                {
                    SetUpDemotivateCommand();
                    _demotivate.SendDemotivator(Message.Photo[^1].FileId);
                }
                else if (witless.DemotivateStickers && Message.Sticker is { IsVideo: false, IsAnimated: false } && ShouldDemotivate())
                {
                    SetUpDemotivateCommand();
                    _demotivate.SendDemotivatedSticker(Message.Sticker.FileId);
                }
                else if (witless.ReadyToGen())
                {
                    Thread.Sleep(Extension.AssumedResponseTime(150, Text));
                    Bot.SendMessage(Chat, witless.TryToGenerate());
                    Log($"{Title} >> FUNNY");
                }

                void SetUpDemotivateCommand()
                {
                    _demotivate.SetMode();
                    _demotivate.PassQuality(witless);
                    _demotivate.Pass(Message);
                    _demotivate.Pass(witless);
                }
                
                bool ShouldDemotivate() => Extension.Random.Next(100) < witless.DgProbability;
            }
            else if (Text != null && TextAsCommand() == "/start")
            {
                if (!Bot.SussyBakas.TryAdd(Chat, new Witless(Chat))) return;

                Bot.SaveChatList();
                Log($"{Title} >> DIC CREATED >> {Chat}", ConsoleColor.Magenta);
                Bot.SendMessage(Chat, START_RESPONSE);
            }
        }
    }
}