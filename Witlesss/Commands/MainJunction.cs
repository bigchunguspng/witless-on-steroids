using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Witlesss.Commands.Editing;
using Witlesss.Commands.Meme;
using MemeProcessors = System.Collections.Generic.Dictionary<Witlesss.XD.MemeType, Witlesss.Commands.Meme.ImageProcessor>;

namespace Witlesss.Commands
{
    public class MainJunction : CallBackHandlingCommand
    {
        private Command        _sc;
        private WitlessCommand _wc;
        private readonly MakeMeme _meme = new();
        private readonly AddCaption _whenthe = new();
        private readonly Demotivate _demotivate = new();
        private readonly DemotivateProportional _dp = new();
        private readonly RemoveBitrate _bitrate = new();
        private readonly MemeDeepFryer _fryer = new();
        private readonly RemoveAudio _audio = new();
        private readonly ChangeSpeed _speed = new();
        private readonly ChangeVolume _volume = new();
        private readonly Equalize _equalize = new();
        private readonly AdvancedEdit _edit = new();
        private readonly ToVideoNote _note = new();
        private readonly ToVoiceMessage _voice = new();
        private readonly ToSticker _sticker = new();
        private readonly Reverse _reverse = new();
        private readonly Sus _sus = new();
        private readonly Cut _cut = new();
        private readonly Crop _crop = new();
        private readonly Scale _scale = new();
        private readonly Fuse _fuse = new();
        private readonly Move _move = new();
        private readonly GetChatID _chatID = new();
        private readonly ChatInfo _chat = new();
        private readonly Piece _piece = new();
        private readonly DebugMessage _debug = new();
        private readonly DownloadMusic _song = new();
        private readonly GenerateByFirstWord _generate = new();
        private readonly GenerateByLastWord _generateB = new();
        private readonly Bouhourt _bouhourt = new();
        private readonly FuseBoards _boards = new();
        private readonly FuseRedditComments _comments = new();
        private readonly BrowseReddit _reddit = new();
        private readonly GetRedditLink _link = new();
        private readonly SetFrequency _frequency = new();
        private readonly SetProbability _probability = new();
        private readonly SetQuality _quality = new();
        private readonly ToggleStickers _stickers = new();
        private readonly ToggleAdmins _admins = new();
        private readonly DeleteDictionary _delete = new();
        private readonly WitlessMainJunction _witless;
        private readonly MemeProcessors _mematics;

        public MainJunction()
        {
            _witless = new WitlessMainJunction(this);
            _mematics = new MemeProcessors
            {
                { MemeType.Dg, _demotivate },
                { MemeType.Meme, _meme },
                { MemeType.Top, _whenthe },
                { MemeType.Dp, _dp },
                { MemeType.Nuke, _fryer }
            };
        }

        private static bool TextIsCommand(out string command)
        {
            command = TextWithoutBotUsername;
            return Text.StartsWith('/');
        }

        public override void Run()
        {
            if (Bot.WitlessExist(Chat))
            {
                _witless.Run();
            }
            else if (Text is not null && TextIsCommand(out var command))
            {
                if (DoSimpleCommands(command) || DoStartCommand(command)) return;

                if (ChatIsPrivate || Text.Contains(Config.BOT_USERNAME)) Bot.SendMessage(Chat, WITLESS_ONLY_COMAND);
            }
        }

        private bool DoSimpleCommands(string command)
        {
            if      (CommandIs( "/fast"   )) _sc = _speed.SetMode(SpeedMode.Fast);
            else if (CommandIs( "/slow"   )) _sc = _speed.SetMode(SpeedMode.Slow);
            else if (CommandIs( "/crop"   )) _sc = _crop.UseDefaultMode();
            else if (CommandIs( "/shake"  )) _sc = _crop.UseShakeMode();
            else if (CommandIs( "/cut"    )) _sc = _cut;
            else if (CommandIs( "/sus"    )) _sc = _sus;
            else if (CommandIs( "/damn"   )) _sc = _bitrate;
            else if (CommandIs( "/scale"  )) _sc = _scale;
            else if (command == "/reverse" ) _sc = _reverse;
            else if (command == "/chat_id" ) _sc = _chatID;
            else if (CommandIs( "/sex"    )) _sc = _sticker;
            else if (CommandIs( "/song"   )) _sc = _song;
            else if (CommandIs( "/eq"     )) _sc = _equalize;
            else if (CommandIs( "/vol"    )) _sc = _volume;
            else if (command == "/g"       ) _sc = _audio;
            else if (command == "/note"    ) _sc = _note;
            else if (command == "/vova"    ) _sc = _voice;
            else if (command == "/debug"   ) _sc = _debug;
            else if (CommandIs( "/w"      )) _sc = _reddit;
            else if (CommandIs( "/link"   )) _sc = _link;
            else if (CommandIs( "/piece"  )) _sc = _piece;
            else if (CommandIs( "/ff"     )) _sc = _edit;
            else                                          return false;
            if      (Bot.ThorRagnarok.ChatIsBanned(Chat)) return false;

            _sc.Run();

            return true;
            
            bool CommandIs(string s) => command.StartsWith(s);
        }

        private bool DoWitlessCommands(string command)
        {
            if      (CommandIs( "/dp"        )) _wc = _dp;
            else if (CommandIs( "/dg"        )) _wc = _demotivate.SetUp(DgMode.Square);
            else if (CommandIs( "/dv"        )) _wc = _demotivate.SetUp(DgMode.Wide);
            else if (CommandIs( "/meme"      )) _wc = _meme;
            else if (CommandIs( "/top"       )) _wc = _whenthe;
            else if (CommandIs( "/a"         )) _wc = _generate;
            else if (CommandIs( "/zz"        )) _wc = _generateB;
            else if (CommandIs( "/board"     )) _wc = _boards;
            else if (CommandIs( "/b"         )) _wc = _bouhourt;
            else if (CommandIs( "/quality"   )) _wc = _quality;
            else if (CommandIs( "/nuke"      )) _wc = _fryer;
            else if (CommandIs( "/pics"      )) _wc = _probability;
            else if (CommandIs( "/set"       )) _wc = _frequency;
            else if (CommandIs( "/fuse"      )) _wc = _fuse;
            else if (CommandIs( "/move"      )) _wc = _move;
            else if (CommandIs( "/xd"        )) _wc = _comments;
            else if (command == "/stickers"   ) _wc = _stickers;
            else if (command == "/chat"       ) _wc = _chat;
            else if (command == "/s_admins"   ) _wc = _admins;
            else if (command == "/delete"     ) _wc = _delete;
            else return true;

            _wc.Run();
            
            return true;

            bool CommandIs(string s) => command.StartsWith(s);
        }

        private static bool DoStartCommand(string command)
        {
            var success = command == "/start" && Bot.SussyBakas.TryAdd(Chat, Witless.AverageBaka(Chat));
            if (success)
            {
                Bot.SaveChatList();
                Log($"{Title} >> DIC CREATED >> {Chat}", ConsoleColor.Magenta);
                Bot.SendMessage(Chat, START_RESPONSE);
                Bot.ThorRagnarok.PullBanStatus(Chat);
            }
            return success;
        }
        
        private static async void WitlessPoopAsync(WitlessMessageData message)
        {
            await Task.Delay(AssumedResponseTime(150, message.Text));
            Bot.SendMessage(message.Chat, message.Baka.Generate());
            Log($"{message.Title} >> FUNNY");
        }


        public override void OnCallback(CallbackQuery query)
        {
            if (query.Data == null || query.Message == null) return;

            var data = query.Data.Split(" - ", 2);
            if (data[0].StartsWith('b'))
            {
                var numbers = data[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                var chat = query.Message.Chat.Id;

                if (data[0] == "b")
                    _boards.SendBoardList(chat, numbers[0], numbers[1], query.Message.MessageId);
                else
                    _boards.SendSavedList(chat, numbers[0], numbers[1], query.Message.MessageId);
            }
            else if (data[0].StartsWith('f'))
            {
                var numbers = data[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                var chat = query.Message.Chat.Id;

                if (data[0] == "fi")
                    _fuse.SendFuseList     (chat, numbers[0], numbers[1], query.Message.MessageId);
                else
                    _fuse.SendFusionHistory(chat, numbers[0], numbers[1], query.Message.MessageId);
            }
            else if (data[0] == "del")
            {
                var message = query.Message;
                message.From = query.From;

                _delete.Pass(message);
                _delete.DoGameStep(message.Chat.Id, data[1], message.MessageId);
            }
        }


        private class WitlessMainJunction : WitlessCommand
        {
            private readonly MainJunction _parent;

            public WitlessMainJunction(MainJunction parent) => _parent = parent;

            public override void Run()
            {
                SetBaka(Bot.SussyBakas[Chat]);

                if (Text is not null)
                {
                    if (TextIsCommand(out var command))
                    {
                        if (_parent.DoSimpleCommands(command) || _parent.DoWitlessCommands(command)) return;
                    }
                    else
                    {
                        var text = Text.Clone().ToString();
                        if (Baka.Eat(text, out var eaten)) Log($"{Title} >> {eaten}", ConsoleColor.Blue);
                    }
                }
                
                Baka.Count();
                
                if (Message.Photo?[^1] is { } p && HaveToMeme())
                {
                    GetMemeMaker(p.Width, p.Height).ProcessPhoto(p.FileId);
                }
                else if (Message.Sticker is { IsVideo: false, IsAnimated: false } s && HaveToMemeSticker())
                {
                    GetMemeMaker(s.Width, s.Height).ProcessStick(s.FileId);
                }
                else if (Baka.Ready() && !Baka.Banned) WitlessPoopAsync(SnapshotMessageData());

                ImageProcessor GetMemeMaker(int w, int h) => SelectMemeMaker().SetUp(w, h);
                ImageProcessor SelectMemeMaker() => _parent._mematics[Baka.Meme.Type];

                bool HaveToMeme() => Extension.Random.Next(100) < Baka.Meme.Chance && !BroSpoilers();
                bool HaveToMemeSticker() => Baka.Meme.Stickers && HaveToMeme();

                bool BroSpoilers() => Message.CaptionEntities is { } c && c.Any(x => x.Type == MessageEntityType.Spoiler);
            }
        }
    }

    public class Skip : CallBackHandlingCommand
    {
        public override void Run() => Log($"{Title} >> {Text}", ConsoleColor.Gray);

        public override void OnCallback(CallbackQuery query) => Log(query.Data ?? "-", ConsoleColor.Yellow);
    }
}