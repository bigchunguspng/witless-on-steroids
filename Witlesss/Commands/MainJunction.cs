using System;
using System.Threading.Tasks;
using MemeProcessors = System.Collections.Generic.Dictionary<Witlesss.XD.MemeType, Witlesss.Commands.ImageProcessor>;

namespace Witlesss.Commands
{
    public class MainJunction : WitlessCommand
    {
        private Command        _sc;
        private WitlessCommand _wc;
        private readonly MakeMeme _meme = new();
        private readonly AddCaption _whenthe = new();
        private readonly Demotivate _demotivate = new();
        private readonly RemoveBitrate _bitrate = new();
        private readonly RemoveAudio _audio = new();
        private readonly ChangeSpeed _speed = new();
        private readonly ToVideoNote _note = new();
        private readonly ToSticker _sticker = new();
        private readonly Reverse _reverse = new();
        private readonly Sus _sus = new();
        private readonly Cut _cut = new();
        private readonly Fuse _fuse = new();
        private readonly Move _move = new();
        private readonly GetChatID _chatID = new();
        private readonly ChatInfo _chat = new();
        private readonly Piece _piece = new();
        private readonly DebugMessage _debug = new();
        private readonly GenerateByFirstWord _generate = new();
        private readonly GenerateByLastWord _generateB = new();
        private readonly Buhurt _buhurt = new();
        private readonly FuseRedditComments _comments = new();
        private readonly CheckReddit _reddit = new();
        private readonly GetRedditLink _link = new();
        private readonly SetFrequency _frequency = new();
        private readonly SetProbability _probability = new();
        private readonly SetQuality _quality = new();
        private readonly ToggleStickers _stickers = new();
        private readonly ToggleColors _colors = new();
        private readonly ToggleAdmins _admins = new();
        private readonly DeleteDictionary _delete = new();
        private readonly MemeProcessors _mematics;

        public MainJunction() => _mematics = new MemeProcessors
        {
            { MemeType.Dg, _demotivate }, { MemeType.Meme, _meme }, { MemeType.Top, _whenthe }
        };

        private static bool TextIsCommand(out string command)
        {
            command = RemoveBotMention();
            return Text.StartsWith('/');
        }

        public override void Run()
        {
            if (Bot.WitlessExist(Chat))
            {
                SetBaka(Bot.SussyBakas[Chat]);

                if (Text is not null)
                {
                    if (TextIsCommand(out var command))
                    {
                        if (DoSimpleCommands(command) || DoWitlessCommands(command)) return;
                    }
                    else
                    {
                        var text = Text.Clone().ToString();
                        if (Baka.Eat(text, out string eaten)) Log($"{Title} >> {eaten}", ConsoleColor.Blue);
                    }
                }
                
                Baka.Count();
                
                if (Message.Photo?[^1] is { } p && HaveToMemePhoto())
                {
                    GetMemeMaker(p.Width, p.Height).ProcessPhoto(p.FileId);
                }
                else if (Message.Sticker is { IsVideo: false, IsAnimated: false } s && HaveToMemeSticker())
                {
                    GetMemeMaker(s.Width, s.Height).ProcessStick(s.FileId);
                }
                else if (Baka.Ready() && !Baka.Banned) WitlessPoopAsync(SnapshotMessageData());

                ImageProcessor GetMemeMaker(int w, int h) => SelectMemeMaker().SetUp(w, h);
                ImageProcessor SelectMemeMaker() => _mematics[Baka.Meme.Type];

                bool HaveToMemeSticker() => Baka.Meme.Stickers && HaveToMeme();
                bool HaveToMemePhoto() => !BetterSkip() && HaveToMeme(TextIsMemable() ? 4 : 1);
                bool HaveToMeme(int x = 1) => Extension.Random.Next(100 / x) < Baka.Meme.Chance;
                bool BetterSkip() => Baka.Meme.Chance < 50 && PhotoHasShortCaption();
                bool PhotoHasShortCaption() => Text is { } t && !t.Contains('\n');
                bool TextIsMemable()        => Text is { } t &&  t.Contains('\n');
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
            else if (CommandIs( "/cut"    )) _sc = _cut;
            else if (CommandIs( "/sus"    )) _sc = _sus;
            else if (CommandIs( "/damn"   )) _sc = _bitrate;
            else if (command == "/reverse" ) _sc = _reverse;
            else if (command == "/chat_id" ) _sc = _chatID;
            else if (CommandIs( "/sex"    )) _sc = _sticker;
            else if (command == "/g"       ) _sc = _audio;
            else if (command == "/note"    ) _sc = _note;
            else if (command == "/debug"   ) _sc = _debug;
            else if (CommandIs( "/w"      )) _sc = _reddit;
            else if (CommandIs( "/link"   )) _sc = _link;
            else if (CommandIs( "/piece"  )) _sc = _piece;
            else                                          return false;
            if      (Bot.ThorRagnarok.ChatIsBanned(Chat)) return false;

            _sc.Run();

            return true;
            
            bool CommandIs(string s) => command.StartsWith(s);
        }

        private bool DoWitlessCommands(string command)
        {
            if      (CommandIs( "/dg"        )) _wc = _demotivate.SetUp(DgMode.Square);
            else if (CommandIs( "/dv"        )) _wc = _demotivate.SetUp(DgMode.Wide);
            else if (CommandIs( "/meme"      )) _wc = _meme;
            else if (CommandIs( "/top"       )) _wc = _whenthe;
            else if (CommandIs( "/a"         )) _wc = _generate;
            else if (CommandIs( "/zz"        )) _wc = _generateB;
            else if (CommandIs( "/b"         )) _wc = _buhurt;
            else if (CommandIs( "/quality"   )) _wc = _quality;
            else if (CommandIs( "/pics"      )) _wc = _probability;
            else if (CommandIs( "/set"       )) _wc = _frequency;
            else if (CommandIs( "/fuse"      )) _wc = _fuse;
            else if (CommandIs( "/move"      )) _wc = _move;
            else if (CommandIs( "/xd"        )) _wc = _comments;
            else if (command == "/stickers"   ) _wc = _stickers;
            else if (command == "/colors"     ) _wc = _colors;
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
        
        private static async void WitlessPoopAsync(WitlessCommandParams data)
        {
            await Task.Delay(AssumedResponseTime(150, data.Text));
            Bot.SendMessage(data.Chat, data.Baka.Generate());
            Log($"{data.Title} >> FUNNY");
        }
    }

    public class Skip : Command
    {
        public override void Run() => Log($"{Title} >> {Text}", ConsoleColor.Gray);
    }
}