using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands;

public class MakeMeme : WitlessCommand
{
    private readonly Regex _meme = new(@"^\/meme\S* *", RegexOptions.IgnoreCase);
    
    public override void Run()
    {
        var x = Message.ReplyToMessage;
        if (ProcessMessage(x) || ProcessMessage(Message)) return;

        Bot.SendMessage(Chat, MEME_MANUAL);
    }
    
    private bool ProcessMessage(Message mess)
    {
        if (mess == null) return false;
            
        if      (mess.Photo != null)
            SendMeme(mess.Photo[^1].FileId);
        /*else if (mess.Animation is { })
            SendDemotivatedVideo(mess.Animation.FileId);
        else if (mess.Sticker is { IsVideo: true })
            SendDemotivatedVideo(mess.Sticker.FileId, ".webm");     // да я копипащу код вопросы?
        else if (mess.Video is { })
            SendDemotivatedVideo(mess.Video.FileId);*/
        else if (mess.Sticker is { IsAnimated: false, IsVideo: false })
            SendMemeFromSticker(mess.Sticker.FileId);
        else return false;
            
        return true;
    }

    private void SendMeme(string fileID)
    {
        Bot.Download(fileID, Chat, out string path);
        
        using var stream = File.OpenRead(Bot.MemeService.MakeMeme(path, Texts()));
        Bot.SendPhoto(Chat, new InputOnlineFile(stream));
        Log($"{Title} >> MEME [$]");
    }

    private void SendMemeFromSticker(string fileID)
    {
        Bot.Download(fileID, Chat, out string path);
        
        var extension = ".png";
        if (Text != null && Text.Contains('x'))
            extension = ".jpg";
        using var stream = File.OpenRead(Bot.MemeService.MakeMemeFromSticker(path, Texts(), extension));
        Bot.SendPhoto(Chat, new InputOnlineFile(stream));
        Log($"{Title} >> MEME [$] STICKER");
    }

    private DgText Texts() => GetMemeText(RemoveCommand(Text));
    
    private string RemoveCommand(string text) => text == null ? null : _meme.Replace(text, "");

    private DgText GetMemeText(string text)
    {
        string a, b;
        if (string.IsNullOrEmpty(text))
        {
            (a, b) = (Baka.TryToGenerate(), Baka.TryToGenerate());
                
            var c = Random.Next(10);
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