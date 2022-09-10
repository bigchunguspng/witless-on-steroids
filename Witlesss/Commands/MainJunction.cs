using System;
using System.Threading;
using Witlesss.Also;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class MainJunction : Command
    {
        private Command _command;
        private readonly Demotivate _demotivate = new Demotivate();
        private readonly RemoveBitrate _bitrate = new RemoveBitrate();
        private readonly ChangeSpeed _speed = new ChangeSpeed();
        private readonly Reverse _reverse = new Reverse();
        private readonly Cut _cut = new Cut();
        private readonly Fuse _fuse = new Fuse();
        private readonly Move _move = new Move();
        private readonly GetChatID _chatID = new GetChatID();
        private readonly ChatInfo _chat = new ChatInfo();
        private readonly DebugMessage _debug = new DebugMessage();
        private readonly GenerateByFirstWord _generate = new GenerateByFirstWord();
        private readonly GenerateByLastWord _generateB = new GenerateByLastWord();
        private readonly Buhurt _buhurt = new Buhurt();
        private readonly SetFrequency _frequency = new SetFrequency();
        private readonly SetProbability _probability = new SetProbability();
        private readonly ToggleStickers _stickers = new ToggleStickers();
        private readonly DeleteDictionary _delete = new DeleteDictionary();

        private string TextAsCommand() => Text.ToLower().Replace(Strings.BOT_USERNAME, "");
        public override void Run()
        {
            if (Bot.WitlessExist(Chat))
            {
                var witless = Bot.SussyBakas[Chat];

                if (Text != null)
                {
                    if (Text.StartsWith('/'))
                    {
                        if      (TextAsCommand().StartsWith("/dg"))
                        {
                            _command = _demotivate;
                            _demotivate.SetMode();
                        }
                        else if (TextAsCommand().StartsWith("/a"))
                        {
                            _command = _generate;
                        }
                        else if (TextAsCommand().StartsWith("/zz"))
                        {
                            _command = _generateB;
                        }
                        else if (TextAsCommand().StartsWith("/fast"))
                        {
                            _command = _speed;
                            _speed.Mode = SpeedMode.Fast;
                        }
                        else if (TextAsCommand().StartsWith("/slow"))
                        {
                            _command = _speed;
                            _speed.Mode = SpeedMode.Slow;
                        }
                        else if (TextAsCommand().StartsWith("/damn"))
                        {
                            _command = _bitrate;
                        }
                        else if (TextAsCommand().StartsWith("/cut"))
                        {
                            _command = _cut;
                        }
                        else if (TextAsCommand().StartsWith("/reverse"))
                        {
                            _command = _reverse;
                        }
                        else if (TextAsCommand().StartsWith("/dv"))
                        {
                            _command = _demotivate;
                            _demotivate.SetMode(DgMode.Wide);
                        }
                        else if (TextAsCommand().StartsWith("/b"))
                        {
                            _command = _buhurt;
                        }
                        else if (TextAsCommand().StartsWith("/set_p"))
                        {
                            _command = _probability;
                        }
                        else if (TextAsCommand().StartsWith("/set"))
                        {
                            _command = _frequency;
                        }
                        else if (TextAsCommand().StartsWith("/toggle_stickers"))
                        {
                            _command = _stickers;
                        }
                        else if (TextAsCommand() == "/chat_id")
                        {
                            _command = _chatID;
                        }
                        else if (TextAsCommand() == "/chat")
                        {
                            _command = _chat;
                        }
                        else if (TextAsCommand().StartsWith("/fuse"))
                        {
                            _command = _fuse;
                        }
                        else if (TextAsCommand().StartsWith("/move"))
                        {
                            _command = _move;
                        }
                        else if (TextAsCommand() == "/debug")
                        {
                            _command = _debug;
                        }
                        else if (TextAsCommand() == "/delete")
                        {
                            _command = _delete;
                        }
                        else return;

                        _command.Pass(Message);
                        if (_command is WitlessCommand command) command.Pass(witless);
                        _command.Run();
                        return;
                    }
                    else
                    {
                        var sentence = Text.Clone().ToString();
                        if (witless.ReceiveSentence(ref sentence))
                            Log($"{Title} >> {sentence}", ConsoleColor.Blue);
                    }
                }
                
                witless.Count();
                
                if (Message.Photo != null && ShouldDemotivate())
                {
                    SetUpDemotivateCommand();
                    _demotivate.SendDemotivator(Message.Photo[^1].FileId);
                }
                else if (witless.DemotivateStickers && Message.Sticker != null && !Message.Sticker.IsVideo && !Message.Sticker.IsAnimated && ShouldDemotivate())
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
                    Bot.MemeService.Mode = DgMode.Square;
                    _demotivate.Pass(Message);
                    _demotivate.Pass(witless);
                }
                
                bool ShouldDemotivate() => Extension.Random.Next(100) < witless.DgProbability;
            }
            else if (Text != null && TextAsCommand() == "/start")
            {
                ChatStart();
            }

            void ChatStart()
            {
                if (!Bot.SussyBakas.TryAdd(Chat, new Witless(Chat)))
                    return;
                Bot.SaveChatList();
                Log($"{Title} >> DIC CREATED >> {Chat}", ConsoleColor.Magenta);
                Bot.SendMessage(Chat, Strings.START_RESPONSE);
            }
        }
    }
}