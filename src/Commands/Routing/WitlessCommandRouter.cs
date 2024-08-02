using System;
using System.Collections.Generic;
using Telegram.Bot.Types;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Generation;
using Witlesss.Commands.Meme;
using Witlesss.Commands.Packing;
using Witlesss.Commands.Settings;

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
    private readonly SetMemeType _set = new();
    private readonly SetSpeech _speech = new();
    private readonly SetPics _pics = new();
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
            .Register("dg"      , () => new Demotivate().SetMode(DgMode.Square))
            .Register("dv"      , () => new Demotivate().SetMode(DgMode.Wide))
            .Register("meme"    , () => new MakeMeme())
            .Register("top"     , () => new AddCaption())
            .Register("nuke"    , () => new MemeDeepFryer())
            .Register("a"       , () => _generate)
            .Register("zz"      , () => _generateB)
            .Register("b"       , () => _bouhourt)
            .Register("set"     , () => _set)
            .Register("speech"  , () => _speech)
            .Register("quality" , () => _quality)
            .Register("pics"    , () => _pics)
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
                foreach (var line in eaten) Log($"{Title} >> {line}", ConsoleColor.Blue);
            }
        }

        if (Message.GetPhoto() is { } photo && HaveToMeme())
        {
            GetMemeMaker(photo.Width, photo.Height).ProcessPhoto(photo.FileId);
        }
        else if (Message.GetImageSticker() is { } sticker && HaveToMemeSticker())
        {
            GetMemeMaker(sticker.Width, sticker.Height).ProcessStick(sticker.FileId);
        }
        else if (Message.GetAnimation() is { } anime && HaveToMemeSticker())
        {
            GetMemeMaker(anime.Width, anime.Height).ProcessVideo(anime.FileId);
        }
        else if (Lucky(Baka.Speech))
        {
            new PoopText().Execute(Context);
        }

        ImageProcessor GetMemeMaker(int w, int h) // todo don't pass size to everything jbc /dg needs it
        {
            var mematic = _mematics[Baka.Type].Invoke();
            mematic.Pass(Context);
            if (mematic is Demotivate dg) dg.SelectMode(w, h);
            return mematic;
        }

        bool HaveToMeme       () => Lucky(Baka.Pics) && !Message.ContainsSpoilers();
        bool HaveToMemeSticker() => Baka.Stickers && HaveToMeme();
    }

    private void HandleWitlessCommands(Witless baka)
    {
        var func = _witlessCommands.Resolve(Command);
        func?.Invoke().Execute(WitlessContext.From(Context, baka));
    }

    public void OnCallback(CallbackQuery query)
    {
        if (query.Data == null || query.Message == null) return;

        var data = query.GetData();
        if      (data[0].StartsWith('b')) _boards.HandleCallback(query, data);
        else if (data[0].StartsWith('f')) _fuse  .HandleCallback(query, data);
        else if (data[0] == "del")
        {
            query.Message.From = query.From;
            _delete.DoGameStep(query.Message, data[1]);
        }
    }
}