using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Witlesss.Commands
{
    public class MainJunction : Command
    {
        private Command  _command;
        private WitlessCommand _c;
        private readonly MakeMeme _meme = new();
        private readonly Demotivate _demotivate = new();
        private readonly RemoveBitrate _bitrate = new();
        private readonly RemoveAudio _audio = new();
        private readonly ChangeSpeed _speed = new();
        private readonly ToSticker _sticker = new();
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
        private readonly Dictionary<MemeType, ImageProcessor> _mematics;

        public long   LastChat      => Chat;
        public string LastChatTitle => Title;

        public MainJunction() => _mematics = new Dictionary<MemeType, ImageProcessor>()
        {
            { MemeType.Dg, _demotivate }, { MemeType.Meme, _meme }
        };

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
                
                if (Message.Photo?[^1] is { } p && HaveToMeme())
                {
                    GetMemeMaker(p.Width, p.Height).ProcessPhoto(p.FileId);
                }
                else if (Message.Sticker is { IsVideo: false, IsAnimated: false } s && HaveToMemeSticker())
                {
                    GetMemeMaker(s.Width, s.Height).ProcessSticker(s.FileId);
                }
                else if (witless.Ready() && !witless.Banned) WitlessPoop(witless, Chat, Text, Title);

                ImageProcessor GetMemeMaker(int w, int h) => SelectMemeMaker().SetUp(Message, witless, w, h);
                ImageProcessor SelectMemeMaker() => _mematics[witless.MemeType];
                
                bool HaveToMeme() => Extension.Random.Next(100) < witless.MemeChance;
                bool HaveToMemeSticker() => witless.MemeStickers && HaveToMeme();
            }
            else if (Text is not null && TextIsCommand(out var command))
            {
                if (DoSimpleCommands(command) || DoStartCommand(command)) return;

                if (ChatIsPrivate || Text.Contains(BOT_USERNAME)) Bot.SendMessage(Chat, WITLESS_ONLY_COMAND);
            }
        }

        private bool DoSimpleCommands(string command)
        {
            if      (CommandIs( "/fast"   )) _command = _speed.SetMode(SpeedMode.Fast);
            else if (CommandIs( "/slow"   )) _command = _speed.SetMode(SpeedMode.Slow);
            else if (CommandIs( "/cut"    )) _command = _cut;
            else if (CommandIs( "/sus"    )) _command = _sus;
            else if (CommandIs( "/damn"   )) _command = _bitrate;
            else if (command == "/reverse" ) _command = _reverse;
            else if (command == "/chat_id" ) _command = _chatID;
            else if (command == "/sex"     ) _command = _sticker;
            else if (command == "/g"       ) _command = _audio;
            else if (command == "/debug"   ) _command = _debug;
            else                             return false;
            if      (Bot.ChatIsBanned(Chat)) return false;
            
            _command.Pass(Message);
            _command.Run();

            return true;
            
            bool CommandIs(string s) => command.StartsWith(s);
        }

        private bool DoWitlessCommands(string command, Witless witless)
        {
            if      (CommandIs( "/dg"        )) _c = _demotivate.SetUp(DgMode.Square);
            else if (CommandIs( "/dv"        )) _c = _demotivate.SetUp(DgMode.Wide);
            else if (CommandIs( "/meme"      )) _c = _meme;
            else if (CommandIs( "/a"         )) _c = _generate;
            else if (CommandIs( "/zz"        )) _c = _generateB;
            else if (CommandIs( "/b"         )) _c = _buhurt;
            else if (CommandIs( "/set_q"     )) _c = _quality;
            else if (CommandIs( "/set_p"     )) _c = _probability;
            else if (CommandIs( "/set"       )) _c = _frequency;
            else if (CommandIs( "/fuse"      )) _c = _fuse;
            else if (CommandIs( "/move"      )) _c = _move;
            else if (command == "/s_stickers" ) _c = _stickers;
            else if (command == "/chat"       ) _c = _chat;
            else if (command == "/s_admins"   ) _c = _admins;
            else if (command == "/delete"     ) _c = _delete;
            else return true;

            _c.Pass(Message);
            _c.Pass(witless);
            _c.Run();
            
            return true;

            bool CommandIs(string s) => command.StartsWith(s);
        }

        private bool DoStartCommand(string command)
        {
            var success = command == "/start" && Bot.SussyBakas.TryAdd(Chat, new Witless(Chat));
            if (success)
            {
                Bot.SaveChatList();
                Log($"{Title} >> DIC CREATED >> {Chat}", ConsoleColor.Magenta);
                Bot.SendMessage(Chat, START_RESPONSE);
                Bot.PullBanStatus(Chat);
            }
            return success;
        }
        
        private async void WitlessPoop(Witless witless, long chat, string text, string title)
        {
            await Task.Delay(AssumedResponseTime(150, text));
            Bot.SendMessage(chat, witless.TryToGenerate());
            Log($"{title} >> FUNNY");
        }
    }
}