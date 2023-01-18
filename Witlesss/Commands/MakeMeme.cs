using System;
using System.Drawing;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.JpegCoder;

namespace Witlesss.Commands;

public class MakeMeme : WitlessCommand, ImageProcessor
{
    private readonly Regex _meme = new(@"^\/meme\S* *", RegexOptions.IgnoreCase);

    public ImageProcessor SetUp(Message message, Witless witless, int w, int h)
    {
        Pass(message);
        Pass(witless);
        PassQuality(witless);
        
        return this;
    }
    
    public override void Run()
    {
        PassQuality(Baka);
        
        var x = Message.ReplyToMessage;
        if (ProcessMessage(Message) || ProcessMessage(x)) return;

        Bot.SendMessage(Chat, MEME_MANUAL);
    }
    
    private bool ProcessMessage(Message mess)
    {
        if (mess == null) return false;
            
        if      (mess.Photo != null)
            ProcessPhoto(mess.Photo[^1].FileId);
        else if (mess.Animation is { })
        {
            Memes.SourceSize = new Size(mess.Animation.Width, mess.Animation.Height);
            ProcessVideo(mess.Animation.FileId);
        }
        else if (mess.Sticker is { IsVideo: true })
        {
            Memes.SourceSize = new Size(mess.Sticker.Width, mess.Sticker.Height);
            ProcessVideo(mess.Sticker.FileId);
        }
        else if (mess.Video is { })
        {
            Memes.SourceSize = new Size(mess.Video.Width, mess.Video.Height);
            ProcessVideo(mess.Video.FileId);
        }
        else if (mess.Sticker is { IsAnimated: false})
            ProcessSticker(mess.Sticker.FileId);
        else return false;
            
        return true;
    }

    public void ProcessPhoto(string fileID)
    {
        Bot.Download(fileID, Chat, out string path);
        
        using var stream = File.OpenRead(Bot.MemeService.MakeMeme(path, Texts()));
        Bot.SendPhoto(Chat, new InputOnlineFile(stream));
        Log($"{Title} >> MEME [M]");
    }

    public void ProcessSticker(string fileID)
    {
        Bot.Download(fileID, Chat, out string path);
        
        var extension = ".png";
        if (Text != null && Text.Contains('x'))
            extension = ".jpg";
        using var stream = File.OpenRead(Bot.MemeService.MakeMemeFromSticker(path, Texts(), extension));
        Bot.SendPhoto(Chat, new InputOnlineFile(stream));
        Log($"{Title} >> MEME [M] STICKER");
    }

    private void ProcessVideo(string fileID)
    {
        if (Bot.ChatIsBanned(Baka)) return;
            
        var time = DateTime.Now;
        Bot.Download(fileID, Chat, out string path);
        using var stream = File.OpenRead(Bot.MemeService.MakeVideoMeme(path, Texts()));
        Bot.SendAnimation(Chat, new InputOnlineFile(stream, "piece_fap_club.mp4"));
        Log($@"{Title} >> MEME [M] VID >> TIME: {DateTime.Now - time:s\.fff}");
    }

    private DgText Texts() => GetMemeText(RemoveCommand(Text));
    
    private string RemoveCommand(string text) => text == null ? null : _meme.Replace(text, "");

    private DgText GetMemeText(string text)
    {
        string a, b;
        if (string.IsNullOrEmpty(text))
        {
            (a, b) = (Baka.TryToGenerate(), Baka.TryToGenerate());
                
            var c = Extension.Random.Next(10);
            if (c == 0) a = "";
            if (a.Length > 25)
            {
                if (c > 5) (a, b) = ("", a);
                else b = "";
            }
        }
        else
        {
            if (text.Contains('\n'))
            {
                var s = text.Split('\n', 2);
                (a, b) = (s[0], s[1]);
            }
            else
            {
                a = text;
                b = AddBottomText() ? Baka.TryToGenerate() : "";
            }
        }
        return new DgText(a, b);
    }
    
    private bool AddBottomText() => Text != null && Text.Split()[0].Contains('s');
}

public interface ImageProcessor
{
    ImageProcessor SetUp(Message message, Witless witless, int w, int h);
    
    void ProcessPhoto  (string fileID);
    void ProcessSticker(string fileID);
}