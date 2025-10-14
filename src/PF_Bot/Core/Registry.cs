using PF_Bot.Commands;
using PF_Bot.Commands.Admin.Fun;
using PF_Bot.Commands.Admin.System;
using PF_Bot.Commands.Debug;
using PF_Bot.Features_Aux.Help.Commands;
using PF_Bot.Features_Aux.Packs.Commands;
using PF_Bot.Features_Aux.Settings.Commands;
using PF_Bot.Features_Main.Edit.Commands.Convert;
using PF_Bot.Features_Main.Edit.Commands.Filter;
using PF_Bot.Features_Main.Edit.Commands.Manual;
using PF_Bot.Features_Main.Media.Commands;
using PF_Bot.Features_Main.Memes.Commands;
using PF_Bot.Features_Main.Text.Commands;
using PF_Bot.Features_Web.Boards.Commands;
using PF_Bot.Features_Web.Piracy.Commands;
using PF_Bot.Features_Web.Reddit.Commands;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Core;

public static class Registry
{
    // CALLBACK

    public const string
        CallbackKey_Manual = "man",
        CallbackKey_Fuse   = "f", // i@!*
        CallbackKey_AliasP = "ap",
        CallbackKey_AliasI = "ai",
        CallbackKey_Boards = "b", // i_
        CallbackKey_Planks = "p", // i_
        CallbackKey_Nukes  = "n", // l
        CallbackKey_Delete = "del";

    public static readonly CommandRegistry<Func<CallbackHandler>> CallbackHandlers
        =               new CommandRegistry<Func<CallbackHandler>>.Builder()
            .Register(CallbackKey_Manual, () => new Help_Callback())
            .Register(CallbackKey_Fuse,   () => new Fuse_Callback())
            .Register(CallbackKey_AliasP, () => new Alias_Callback(AliasContext.FFMpeg))
            .Register(CallbackKey_AliasI, () => new Alias_Callback(AliasContext.Magick))
            .Register(CallbackKey_Boards, () => new ChanEaterCore_Callback(ImageBoardContext.Chan4))
            .Register(CallbackKey_Planks, () => new ChanEaterCore_Callback(ImageBoardContext.Chan2))
            .Register(CallbackKey_Nukes,  () => new Nuke_Callback())
            .Register(CallbackKey_Delete, () => new Delete_Callback())
            .Build();

    // COMMANDS

    private static readonly SendMessage _mail = new();
    private static readonly Htmlizer    _html = new();
    private static readonly CommandHandler
        _start = new Start(),
        _tell  = new Tell(),
        _spam  = new Spam(),
        _help  = new Help(),
        _piece = new Piece(),
        _react = new React(),
        _reply = new Reply(),
        _que   = new QueueMessage(),
        _debug = new DebugMessage(),
        _id    = new SendChatId(),
        _apeg  = new Alias(AliasContext.FFMpeg),
        _aim   = new Alias(AliasContext.Magick),
        _chat  = new ChatInfo(),
        _buggy = new Baguette();

    public static readonly CommandRegistry<Func<CommandHandler>> CommandHandlers
        =              new CommandRegistry<Func<CommandHandler>>.Builder()
            // memes
            .Register("dp"      , () => new Demo_Dp())
            .Register("dg"      , () => new Demo_Dg().SetMode(Demo_Dg.Mode.Square))
            .Register("dv"      , () => new Demo_Dg().SetMode(Demo_Dg.Mode.Wide))
            .Register("meme"    , () => new Meme())
            .Register("top"     , () => new Top())
            .Register("snap"    , () => new Snap())
            .Register("nuke"    , () => new Nuke())
            // gena
            .Register("a"       , () => new GenerateByFirstWord())
            .Register("zz"      , () => new GenerateByLastWord())
            .Register("b"       , () => _buggy)
            // pack management
            .Register("fuse"    , () => new Fuse())
            .Register("board"   , () => new EatBoards())
            .Register("plank"   , () => new EatPlanks())
            .Register("xd"      , () => new EatReddit())
            .Register("pub"     , () => new Move().WithMode(ExportMode.Public))
            .Register("move"    , () => new Move().WithMode(ExportMode.Private))
            .Register("delete"  , () => new Delete())
            // settings
            .Register("start"   , () => _start)
            .Register("chat"    , () => _chat)
            .Register("set"     , () => new Set())
            .Register("speech"  , () => new SetSpeech())
            .Register("quality" , () => new SetPics())
            .Register("pics"    , () => new SetQuality())
            .Register("stickers", () => new ToggleStickers())
            .Register("admins"  , () => new ToggleAdmins())
            // other
            .Register("fast"   , () => new Speed().SetMode(Speed.Mode.Fast))
            .Register("slow"   , () => new Speed().SetMode(Speed.Mode.Slow))
            .Register("crop"   , () => new Crop().UseDefaultMode())
            .Register("shake"  , () => new Crop().UseShakeMode())
            .Register("scale"  , () => new Scale())
            .Register("slice"  , () => new Slice())
            .Register("cut"    , () => new Cut())
            .Register("sus"    , () => new Sus())
            .Register("song"   , () => new DownloadMusic())
            .Register("damn"   , () => new Damn())
            .Register("reverse", () => new Reverse())
            .Register("eq"     , () => new Equalize())
            .Register("vol"    , () => new Volume())
            .Register("g"      , () => new ToGIF())
            .Register("sex"    , () => new ToSticker())
            .Register("hex"    , () => new Hex())
            .Register("note"   , () => new ToVideoNote())
            .Register("vova"   , () => new ToVoice())
            .Register("load"   , () => new Load())
            .Register("upload" , () => new UploadFile())
            .Register("im"     , () => new UseMagick())
            .Register("peg"    , () => new UseFFMpeg())
            .Register("pipe"   , () => new Pipe())
            .Register("w"      , () => new BrowseReddit())
            .Register("link"   , () => new GetRedditLink())
            .Register("piece"  , () => _piece)
            .Register("apeg"   , () => _apeg)
            .Register("aim"    , () => _aim)
            .Register("help"   , () => _help)
            .Register("man"    , () => _help)
            .Register("debug"  , () => _debug)
            .Register("id"     , () => _id)
            .Register("html"   , () => _html.WithMode(Htmlizer.Mode.ToHtml))
            .Register("text"   , () => _html.WithMode(Htmlizer.Mode.FromHtml))
            // admin
            .Register("run"    , () => new RunProcess())
            .Register("kill"   , () => new KillProcess())
            .Register("spam"   , () => _spam)
            .Register("tell"   , () => _tell)
            .Register("re"     , () => _react)
            .Register("rep"    , () => _reply)
            .Register("que"    , () => _que)
            // options
            .Register("op_meme", () => _mail.WithText(MEME_OPTIONS))
            .Register("op_top" , () => _mail.WithText(TOP_OPTIONS))
            .Register("op_dp"  , () => _mail.WithText(DP_OPTIONS))
            .Register("op_dg"  , () => _mail.WithText(DG_OPTIONS))
            .Register("op_snap", () => _mail.WithText(SNAP_OPTIONS))
            .Register("op_nuke", () => _mail.WithText(NUKE_OPTIONS))
            .Register("fonts"  , () => _mail.WithText(FONTS_CHEAT_SHEET))
            // manuals
            .Register("man_g"    , () => _mail.WithText(G_MANUAL))
            .Register("man_pipe" , () => _mail.WithText(PIPE_MANUAL))
            .Register("man_crop" , () => _mail.WithText(CROP_MANUAL))
            .Register("man_shake", () => _mail.WithText(SHAKE_MANUAL))
            .Register("man_scale", () => _mail.WithText(SCALE_MANUAL))
            .Register("man_slice", () => _mail.WithText(SLICE_MANUAL))
            .Register("man_cut"  , () => _mail.WithText(CUT_MANUAL))
            .Register("man_sus"  , () => _mail.WithText(SUS_MANUAL))
            .Register("man_eq"   , () => _mail.WithText(EQ_MANUAL))
            .Register("man_vol"  , () => _mail.WithText(VOLUME_MANUAL))
            .Build();
}