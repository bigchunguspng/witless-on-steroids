using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types;
using Witlesss.Commands.Meme;

namespace Witlesss.Commands.Routing;

public class WitlessCommandRouter : WitlessSyncCommand
{
    private readonly ChatInfo _chat = new();
    private readonly GenerateByFirstWord _generate = new();
    private readonly GenerateByLastWord _generateB = new();
    private readonly Bouhourt _bouhourt = new();
    private readonly Move _move = new();
    private readonly Fuse _fuse = new();
    private readonly FuseBoards _boards = new();
    private readonly FuseRedditComments _comments = new();
    private readonly SetFrequency _frequency = new();
    private readonly SetProbability _probability = new();
    private readonly SetQuality _quality = new();
    private readonly ToggleStickers _stickers = new();
    private readonly ToggleAdmins _admins = new();
    private readonly DeleteDictionary _delete = new();

    private readonly CommandRouter _parent;

    private readonly Dictionary<MemeType, Func<ImageProcessor>> _mematics;

    private readonly CommandRegistry<AnyCommand<WitlessContext>> _witlessCommands;

    public WitlessCommandRouter(CommandRouter parent)
    {
        _parent = parent;

        _mematics = new Dictionary<MemeType, Func<ImageProcessor>>
        {
            { MemeType.Dg,   () => new Demotivate() },
            { MemeType.Meme, () => new MakeMeme() },
            { MemeType.Top,  () => new AddCaption() },
            { MemeType.Dp,   () => new DemotivateProportional() },
            { MemeType.Nuke, () => new MemeDeepFryer() }
        };

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
            .Register("quality" , () => _quality)
            .Register("pics"    , () => _probability)
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

    protected override void Run()
    {
        if (Text is not null)
        {
            if (Context is { Command: not null, IsForMe: true })
            {
                if (!_parent.HandleSimpleCommands()) HandleWitlessCommands(Baka);
                return;
            }
            else if (Baka.Eat(Text, out var eaten))
            {
                Log($"{Title} >> {eaten}", ConsoleColor.Blue);
            }
        }

        Baka.Count();

        if (Message.GetPhoto() is { } photo && HaveToMeme())
        {
            GetMemeMaker(photo.Width, photo.Height).ProcessPhoto(photo.FileId);
        }
        else if (Message.GetImageSticker() is { } sticker && HaveToMemeSticker())
        {
            GetMemeMaker(sticker.Width, sticker.Height).ProcessStick(sticker.FileId);
        }
        else if (Baka.Ready() && !Baka.Banned)
        {
            new PoopText().Execute(Context);
        }

        ImageProcessor GetMemeMaker(int w, int h) // todo don't pass size to everything jbc /dg needs it
        {
            var mematic = _mematics[Baka.Meme.Type].Invoke();
            mematic.Pass(Context);
            return mematic.SetUp(w, h);
        }

        bool HaveToMeme       () => Lucky(Baka.Meme.Chance) && !Message.ContainsSpoilers();
        bool HaveToMemeSticker() => Baka.Meme.Stickers && HaveToMeme();
    }

    private void HandleWitlessCommands(Witless baka)
    {
        var func = _witlessCommands.Resolve(Command);
        func?.Invoke().Execute(WitlessContext.From(Context, baka));
    }

    public void OnCallback(CallbackQuery query) // todo take this garbage apart
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
}