using System.Drawing;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands;

public class RemoveAudio : RemoveBitrate
{
    public override void Run()
    {
        if (NoVideo()) return;

        Bot.Download(FileID, Chat, out string path);

        using var stream = File.OpenRead(Bot.MemeService.RemoveAudio(path));
        Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFilename()));
        Log($"{Title} >> GIF [~]");

        string VideoFilename() => "gif_fap_club.mp4";
    }

    private bool NoVideo()
    {
        if (GetMediaFileID(Message.ReplyToMessage) || GetMediaFileID(Message)) return false;

        Bot.SendMessage(Chat, G_MANUAL);
        return true;
    }

    private bool GetMediaFileID(Message mess)
    {
        if (mess == null) return false;

        if      (mess.Video != null)
        {
            FileID = mess.Video.FileId;
            Memes.SourceSize = new Size(mess.Video.Width, mess.Video.Height);
        }
        else if (mess.Animation != null)
        {
            FileID = mess.Animation.FileId;
            Memes.SourceSize = new Size(mess.Animation.Width, mess.Animation.Height);
        }
        else if (mess.Sticker is { IsVideo: true })
        {
            FileID = mess.Sticker.FileId;
            Memes.SourceSize = new Size(mess.Sticker.Width, mess.Sticker.Height);
        }
        else return false;

        return true;
    }
}