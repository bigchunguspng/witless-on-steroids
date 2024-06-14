using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Witlesss.Commands.Editing;
using Witlesss.Commands.Meme;

namespace Witlesss.Commands
{
    public class CommandRouter : CommandAndCallbackRouter
    {
        private readonly Fuse _fuse = new();
        private readonly Move _move = new();
        private readonly Spam _spam = new();
        private readonly Tell _tell = new();
        private readonly SendMessage _mail = new();
        private readonly ChatInfo _chat = new();
        private readonly Piece _piece = new();
        private readonly DebugMessage _debug = new();
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

        private readonly Dictionary<MemeType, Func<ImageProcessor>> _mematics;

        private readonly CommandRegistry<AnyCommand<CommandContext>> _simpleCommands;
        private readonly CommandRegistry<AnyCommand<WitlessContext>> _witlessCommands;

        public CommandRouter()
        {
            _witless = new WitlessCommandRouter(this);
            _mematics = new Dictionary<MemeType, Func<ImageProcessor>>
            {
                { MemeType.Dg,   () => new Demotivate() },
                { MemeType.Meme, () => new MakeMeme() },
                { MemeType.Top,  () => new AddCaption() },
                { MemeType.Dp,   () => new DemotivateProportional() },
                { MemeType.Nuke, () => new MemeDeepFryer() }
            };

            _simpleCommands = new CommandRegistry<AnyCommand<CommandContext>>()
                .Register("fast"   , () => new ChangeSpeed().SetMode(SpeedMode.Fast))
                .Register("slow"   , () => new ChangeSpeed().SetMode(SpeedMode.Slow))
                .Register("crop"   , () => new Crop().UseDefaultMode())
                .Register("shake"  , () => new Crop().UseShakeMode())
                .Register("scale"  , () => new Scale())
                .Register("slice"  , () => new Slice())
                .Register("cut"    , () => new Cut())
                .Register("sus"    , () => new Sus())
                .Register("song"   , () => new DownloadMusic())
                .Register("damn"   , () => new RemoveBitrate())
                .Register("reverse", () => new Reverse())
                .Register("eq"     , () => new Equalize())
                .Register("vol"    , () => new ChangeVolume())
                .Register("g"      , () => new ToAnimation())
                .Register("sex"    , () => new ToSticker())
                .Register("note"   , () => new ToVideoNote())
                .Register("vova"   , () => new ToVoiceMessage())
                .Register("w"      , () => _reddit)
                .Register("link"   , () => _link)
                .Register("ff"     , () => new AdvancedEdit())
                .Register("piece"  , () => _piece)
                .Register("debug"  , () => _debug)
                .Register("chat_id", () => _mail.WithText(Context.Chat.ToString()))
                .Register("op_top" , () => _mail.WithText(TOP_OPTIONS))
                .Register("op_meme", () => _mail.WithText(MEME_OPTIONS))
                .Register("spam"   , () => _spam)
                .Register("tell"   , () => _tell)
                .Build();

            _witlessCommands = new CommandRegistry<AnyCommand<WitlessContext>>()
                .Register("dp"      , () => new DemotivateProportional())
                .Register("dg"      , () => new Demotivate().SetUp(DgMode.Square))
                .Register("dv"      , () => new Demotivate().SetUp(DgMode.Wide))
                .Register("meme"    , () => new MakeMeme())
                .Register("top"     , () => new AddCaption())
                .Register("nuke"    , () => new MemeDeepFryer())
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
                _witless.Pass(Context, Bot.SussyBakas[Context.Chat]);
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

            func.Invoke().Execute(new WitlessContext(Context, baka));
            return true;
        }

        private bool DoStartCommand()
        {
            var success = Context.Command == "/start" && Bot.SussyBakas.TryAdd(Context.Chat, Witless.AverageBaka(Context));
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
            public void Pass(CommandContext context, Witless baka)
            {
                Context = new WitlessContext(context, baka);
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
                ImageProcessor SelectMemeMaker()
                {
                    var mematic = parent._mematics[Context.Baka.Meme.Type].Invoke();
                    mematic.Pass(Context);
                    return mematic;
                }

                bool HaveToMeme() => Random.Shared.Next(100) < Context.Baka.Meme.Chance && !BroSpoilers();
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