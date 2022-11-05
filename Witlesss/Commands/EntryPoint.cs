using System;
using System.Threading.Tasks;
using static Witlesss.Logger;
using static Witlesss.Strings;

namespace Witlesss.Commands
{
    public class EntryPoint : Command
    {
        private Command  _command;
        private WitlessCommand _c;
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

        private bool TextIsCommand(out string command)
        {
            command = Text.ToLower().Replace(BOT_USERNAME, "");
            return Text.StartsWith('/');
        }

        public override void Run()
        {
            if (Bot.WitlessExist(Chat))
            {
                var witless = Bot.SussyBakas[Chat];

                if (Text is not null)
                {
                    if (TextIsCommand(out var command))
                    {
                        if (DoSimpleCommands(command) || DoWitlessCommands(command, witless)) return;
                    }
                    else
                    {
                        var text = Text.Clone().ToString();
                        if (witless.Eat(text, out string eaten)) Log($"{Title} >> {eaten}", ConsoleColor.Blue);
                    }
                }
                
                witless.Count();
                
                if (Message.Photo?[^1] is { } p && ShouldDemotivate())
                {
                    SetUpDemotivateCommand(p.Width, p.Height);
                    _demotivate.SendDemotivator(Message.Photo[^1].FileId);
                }
                else if (Message.Sticker is { IsVideo: false, IsAnimated: false } s && ShouldDemotivateSticker())
                {
                    SetUpDemotivateCommand(s.Width, s.Height);
                    _demotivate.SendDemotivatedSticker(Message.Sticker.FileId);
                }
                else if (witless.Ready()) WitlessPoop(witless, Chat, Text, Title);

                void SetUpDemotivateCommand(int w, int h)
                {
                    _demotivate.SelectModeAuto(w, h);
                    _demotivate.PassQuality(witless);
                    _demotivate.Pass(Message);
                    _demotivate.Pass(witless);
                }
                bool ShouldDemotivate() => Extension.Random.Next(100) < witless.DgProbability;
                bool ShouldDemotivateSticker() => witless.DemotivateStickers && ShouldDemotivate();
            }
            else if (Text is not null && TextIsCommand(out var command))
            {
                if (DoSimpleCommands(command) || DoStartCommand(command)) return;

                if (Chat > 0 || Text.Contains(BOT_USERNAME)) Bot.SendMessage(Chat, WITLESS_ONLY_COMAND);
            }
        }

        private bool DoSimpleCommands(string command)
        {
            if      (command.StartsWith("/fast"  )) _command = _speed.SetMode(SpeedMode.Fast);
            else if (command.StartsWith("/slow"  )) _command = _speed.SetMode(SpeedMode.Slow);
            else if (command.StartsWith("/cut"   )) _command = _cut;
            else if (command.StartsWith("/sus"   )) _command = _sus;
            else if (command.StartsWith("/damn"  )) _command = _bitrate;
            else if (command ==         "/reverse") _command = _reverse;
            else if (command ==         "/chat_id") _command = _chatID;
            else if (command ==         "/debug"  ) _command = _debug;
            else return false;
            
            _command.Pass(Message);
            _command.Run();

            return true;
        }

        private bool DoWitlessCommands(string command, Witless witless)
        {
            if      (command.StartsWith("/dg"            )) _c = _demotivate.SetUp(witless, DgMode.Square);
            else if (command.StartsWith("/dv"            )) _c = _demotivate.SetUp(witless, DgMode.Wide);
            else if (command.StartsWith("/a"             )) _c = _generate;
            else if (command.StartsWith("/zz"            )) _c = _generateB;
            else if (command.StartsWith("/b"             )) _c = _buhurt;
            else if (command.StartsWith("/set_q"         )) _c = _quality;
            else if (command.StartsWith("/set_p"         )) _c = _probability;
            else if (command.StartsWith("/set"           )) _c = _frequency;
            else if (command.StartsWith("/fuse"          )) _c = _fuse;
            else if (command.StartsWith("/move"          )) _c = _move;
            else if (command ==         "/toggle_stickers") _c = _stickers;
            else if (command ==         "/chat"           ) _c = _chat;
            else if (command ==         "/toggle_admins"  ) _c = _admins;
            else if (command ==         "/delete"         ) _c = _delete;
            else return false;

            _c.Pass(Message);
            _c.Pass(witless);
            _c.Run();
            
            return true;
        }

        private bool DoStartCommand(string command)
        {
            var success = command == "/start" && Bot.SussyBakas.TryAdd(Chat, new Witless(Chat));
            if (success)
            {
                Bot.SaveChatList();
                Log($"{Title} >> DIC CREATED >> {Chat}", ConsoleColor.Magenta);
                Bot.SendMessage(Chat, START_RESPONSE);
            }
            return success;
        }
        
        private async void WitlessPoop(Witless witless, long chat, string text, string title)
        {
            await Task.Delay(Extension.AssumedResponseTime(150, text));
            Bot.SendMessage(chat, witless.TryToGenerate());
            Log($"{title} >> FUNNY");
        }
    }
}