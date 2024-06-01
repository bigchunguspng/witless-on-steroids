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
        private readonly MakeMeme _meme = new();
        private readonly AddCaption _whenthe = new();
        private readonly Demotivate _demotivate = new();
        private readonly DemotivateProportional _dp = new();
        private readonly RemoveBitrate _bitrate = new();
        private readonly MemeDeepFryer _fryer = new();
        private readonly ToAnimation _audio = new();
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
        private readonly Slice _slice = new();
        private readonly Fuse _fuse = new();
        private readonly Move _move = new();
        private readonly Spam _spam = new();
        private readonly Tell _tell = new();
        private readonly SendMessage _mail = new();
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
        private readonly Stopwatch _watch = new();

        private readonly CommandRegistry<Command> _simpleCommands;
        private readonly CommandRegistry<WitlessCommand> _witlessCommands;

        private long _lastChat;
        private int   _annoyed;

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

            _simpleCommands = new CommandRegistry<Command>()
                .Register("fast"   , () => _speed.SetMode(SpeedMode.Fast))
                .Register("slow"   , () => _speed.SetMode(SpeedMode.Slow))
                .Register("crop"   , () => _crop.UseDefaultMode())
                .Register("shake"  , () => _crop.UseShakeMode())
                .Register("scale"  , () => _scale)
                .Register("slice"  , () => _slice)
                .Register("cut"    , () => _cut)
                .Register("sus"    , () => _sus)
                .Register("damn"   , () => _bitrate)
                .Register("reverse", () => _reverse)
                .Register("sex"    , () => _sticker)
                .Register("song"   , () => _song)
                .Register("eq"     , () => _equalize)
                .Register("vol"    , () => _volume)
                .Register("g"      , () => _audio)
                .Register("note"   , () => _note)
                .Register("vova"   , () => _voice)
                .Register("w"      , () => _reddit)
                .Register("link"   , () => _link)
                .Register("ff"     , () => _edit)
                .Register("piece"  , () => _piece)
                .Register("debug"  , () => _debug)
                .Register("chat_id", () => _mail.WithText(Chat.ToString()))
                .Register("op_top" , () => _mail.WithText(TOP_OPTIONS))
                .Register("op_meme", () => _mail.WithText(MEME_OPTIONS))
                .Register("spam"   , () => _spam)
                .Register("tell"   , () => _tell)
                .Build();

            _witlessCommands = new CommandRegistry<WitlessCommand>()
                .Register("dp"      , () => _dp)
                .Register("dg"      , () => _demotivate.SetUp(DgMode.Square))
                .Register("dv"      , () => _demotivate.SetUp(DgMode.Wide))
                .Register("meme"    , () => _meme)
                .Register("top"     , () => _whenthe)
                .Register("nuke"    , () => _fryer)
                .Register("a"       , () => _generate)
                .Register("zz"      , () => _generateB)
                .Register("b"       , () => _bouhourt)
                .Register("set"     , () => _frequency)
                .Register("pics"    , () => _probability)
                .Register("quality" , () => _quality)
                .Register("stickers", () => _stickers)
                .Register("chat"    , () => _chat)
                .Register("fuse"    , () => _fuse)
                .Register("move"    , () => _move)
                .Register("board"   , () => _boards)
                .Register("xd"      , () => _comments)
                .Register("delete"  , () => _delete)
                .Register("admins"  , () => _admins)
                .Build();
        }

        private static bool TextIsCommand(out string command)
        {
            command = TextWithoutBotUsername;
            return Text.StartsWith('/');
        }

        public override void Run()
        {
            _watch.WriteTime();

            if (Bot.WitlessExist(Chat))
            {
                _witless.Run();
            }
            else if (Text is not null && TextIsCommand(out var command))
            {
                var success = DoSimpleCommands(command) || DoStartCommand(command);

                if (!success && (ChatIsPrivate || Text.Contains(Config.BOT_USERNAME)))
                {
                    Bot.SendMessage(Chat, WITLESS_ONLY_COMAND);
                }
            }

            SuspectForLongHangs(_watch.GetElapsed());
        }

        private void SuspectForLongHangs(TimeSpan time)
        {
            var hang = time.Seconds > 5;
            _annoyed = hang && _lastChat == Chat ? _annoyed + 1 : 1;
            if (hang) Bot.ThorRagnarok.Suspect(Chat, time * _annoyed);
            _lastChat = Chat;
        }

        private bool DoSimpleCommands(string command)
        {
            var func = _simpleCommands.Resolve(command);
            if (func is null) return false;

            func.Invoke().Run();
            return true;
        }

        private bool DoWitlessCommands(string command)
        {
            var func = _witlessCommands.Resolve(command);
            if (func is null) return true;

            func.Invoke().Run();
            return true;
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