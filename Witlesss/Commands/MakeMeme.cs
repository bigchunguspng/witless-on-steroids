using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands;

public class MakeMeme : WitlessCommand
{
    private readonly Regex _dg = new(@"^\/meme\S* *", RegexOptions.IgnoreCase);
    
    public override void Run()
    {
        var x = Message.ReplyToMessage;
        if (ProcessMessage(x) || ProcessMessage(Message)) return;

        Bot.SendMessage(Chat, DG_MANUAL);
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
        var path = GetSource(fileID, ".jpg");
        
        using var stream = File.OpenRead(Bot.MemeService.MakeMeme(path, Texts()));
        Bot.SendPhoto(Chat, new InputOnlineFile(stream));
        Log($"{Title} >> MEME [$]");
    }

    private void SendMemeFromSticker(string fileID)
    {
        var path = GetSource(fileID, ".webp");
        var extension = ".png";
        if (Text != null && Text.Contains('x'))
            extension = ".jpg";
        using var stream = File.OpenRead(Bot.MemeService.MakeMemeFromSticker(path, Texts(), extension));
        Bot.SendPhoto(Chat, new InputOnlineFile(stream));
        Log($"{Title} >> MEME [$] STICKER");
    }
    
    private string GetSource(string fileID, string extension)
    {
        var path = UniquePath($@"{PICTURES_FOLDER}\{ShortID(fileID)}{extension}");
        Bot.DownloadFile(fileID, path, Chat).Wait();
        return path;
    }
    
    private DgText Texts() => GetDemotivatorText(Baka, RemoveDg(Text));
    
    private string RemoveDg(string text) => text == null ? null : _dg.Replace(text, "");
}