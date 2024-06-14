using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Witlesss.Commands.Editing;
using Witlesss.Commands.Meme;
using MemeProcessors = System.Collections.Generic.Dictionary<Witlesss.XD.MemeType, Witlesss.Commands.Meme.ImageProcessor>;

namespace Witlesss.Commands
{
    public class CommandRouter : CommandAndCallbackRouter
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
        private readonly WitlessCommandRouter _witless;
        private readonly MemeProcessors _mematics;

        private readonly CommandRegistry<AnyCommand<CommandContext>> _simpleCommands;
        private readonly CommandRegistry<AnyCommand<WitlessContext>> _witlessCommands;

        public CommandRouter()
        {
            _witless = new WitlessCommandRouter(this);
            _mematics = new MemeProcessors
            {
                { MemeType.Dg, _demotivate },
                { MemeType.Meme, _meme },
                { MemeType.Top, _whenthe },
                { MemeType.Dp, _dp },
                { MemeType.Nuke, _fryer }
            };

            _simpleCommands = new CommandRegistry<AnyCommand<CommandContext>>()
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
                .Register("chat_id", () => _mail.WithText(Context.Chat.ToString()))
                .Register("op_top" , () => _mail.WithText(TOP_OPTIONS))
                .Register("op_meme", () => _mail.WithText(MEME_OPTIONS))
                .Register("spam"   , () => _spam)
                .Register("tell"   , () => _tell)
                .Build();

            _witlessCommands = new CommandRegistry<AnyCommand<WitlessContext>>()
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

        public override void Run()
        {
            if (Bot.WitlessExist(Context.Chat))
            {
                _witless.Pass(Context.Message, Bot.SussyBakas[Context.Chat]);
                _witless.Run();
            }
            else if (Context.Command is not null)
            {
                var success = DoSimpleCommands() || DoStartCommand();

                if (!success && (Context.ChatIsPrivate || Context.Command.Contains(Config.BOT_USERNAME)))
                {
                    Bot.SendMessage(Context.Chat, WITLESS_ONLY_COMAND);
                }
            }
        }

        private bool DoSimpleCommands()
        {
            var func = _simpleCommands.Resolve(Context.Command);
            if (func is null) return false;

            func.Invoke().Execute(Context);
            return true;
        }

        private bool DoWitlessCommands(Witless baka)
        {
            var func = _witlessCommands.Resolve(Context.Command);
            if (func is null) return true;

            func.Invoke().Execute(new WitlessContext(Context.Message, baka));
            return true;
        }

        private bool DoStartCommand()
        {
            var success = Context.Command == "/start" && Bot.SussyBakas.TryAdd(Context.Chat, Witless.AverageBaka(Context.Chat));
            if (success)
            {
                Bot.SaveChatList();
                Log($"{Context.Title} >> DIC CREATED >> {Context.Chat}", ConsoleColor.Magenta);
                Bot.SendMessage(Context.Chat, START_RESPONSE);
                Bot.ThorRagnarok.PullBanStatus(Context.Chat);
            }
            return success;
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

                _delete.DoGameStep(message, data[1]);
            }
        }


        private class WitlessCommandRouter(CommandRouter parent) : AnyCommandRouter<WitlessContext>
        {
            public void Pass(Message message, Witless baka)
            {
                Context = new WitlessContext(message, baka);
            }

            public override void Run()
            {
                if (Context.Text is not null)
                {
                    if (Context.Command is not null)
                    {
                        if (parent.DoSimpleCommands() || parent.DoWitlessCommands(Context.Baka)) return;
                    }
                    else
                    {
                        if (Context.Baka.Eat(Context.Text, out var eaten)) Log($"{Context.Title} >> {eaten}", ConsoleColor.Blue);
                    }
                }
                
                Context.Baka.Count();
                
                if (Context.Message.Photo?[^1] is { } p && HaveToMeme())
                {
                    GetMemeMaker(p.Width, p.Height).ProcessPhoto(p.FileId);
                }
                else if (Context.Message.Sticker is { IsVideo: false, IsAnimated: false } s && HaveToMemeSticker())
                {
                    GetMemeMaker(s.Width, s.Height).ProcessStick(s.FileId);
                }
                else if (Context.Baka.Ready() && !Context.Baka.Banned) WitlessPoopAsync(Context);

                ImageProcessor GetMemeMaker(int w, int h) => SelectMemeMaker().SetUp(w, h);
                ImageProcessor SelectMemeMaker() => parent._mematics[Context.Baka.Meme.Type];

                bool HaveToMeme() => Extension.Random.Next(100) < Context.Baka.Meme.Chance && !BroSpoilers();
                bool HaveToMemeSticker() => Context.Baka.Meme.Stickers && HaveToMeme();

                bool BroSpoilers() => Context.Message.ContainsSpoilers();
            }

            private static async void WitlessPoopAsync(WitlessContext c)
            {
                await Task.Delay(AssumedResponseTime(150, c.Text));
                Bot.SendMessage(c.Chat, c.Baka.Generate());
                Log($"{c.Title} >> FUNNY");
            }

            private static int AssumedResponseTime(int initialTime, string? text)
            {
                return text is null ? initialTime : Math.Min(text.Length, 120) * 25;
            }
        }
    }

    public class Skip : CommandAndCallbackRouter
    {
        public override void Run()
        {
            Log($"{Context.Title} >> {Context.Text}", ConsoleColor.Gray);
        }

        public override void OnCallback(CallbackQuery query) => Log(query.Data ?? "-", ConsoleColor.Yellow);
    }
}