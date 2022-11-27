using System.Drawing;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands;

public class ToSticker : Command
{
    private string _id;
    public override void Run()
    {
        if (NoPicture()) return;

        var path = UniquePath($@"{PICTURES_FOLDER}\{ShortID(_id)}{ExtensionFromID(_id)}");
        Bot.DownloadFile(_id, path, Chat).Wait();
        
        Bot.SendSticker(Chat, new InputOnlineFile(File.OpenRead(Bot.MemeService.Stickerize(path))));
        Log($"{Title} >> STICK [!]");
    }
    
    private bool NoPicture()
    {
        if (GetPicID(Message.ReplyToMessage) || GetPicID(Message)) return false;

        Bot.SendMessage(Chat, STICK_MANUAL);
        return true;
    }
    
    private bool GetPicID(Message mess)
    {
        if (mess == null) return false;
        
        if (mess.Photo != null)
        {
            _id = mess.Photo[^1].FileId;
            Memes.SourceSize = new Size(mess.Photo[^1].Width, mess.Photo[^1].Height);
        }
        else if (mess.Sticker is { IsVideo: false, IsAnimated: false })
        {
            _id = mess.Sticker.FileId;
            Memes.SourceSize = new Size(mess.Sticker.Width, mess.Sticker.Height);
        }
        else return false;

        return true;
    }
}