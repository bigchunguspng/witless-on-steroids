using Telegram.Bot.Types;
using Witlesss.Services.Internet.YouTube;

namespace Witlesss.Commands.Editing.Core;

public abstract class VideoCommand : FileEditingCommand
{
    protected override string SuportedMedia => "🎬";
    protected override bool MessageContainsFile(Message m) 
        => GetVideoFileID(m);
}

public abstract class PhotoCommand : FileEditingCommand
{
    protected override string SuportedMedia => "📸";
    protected override bool MessageContainsFile(Message m)
        => GetPhotoFileID(m);
}

public abstract class AudioVideoPhotoCommand : FileEditingCommand
{
    protected override string SuportedMedia => "🎬, 📸, 🎧";
    protected override bool MessageContainsFile(Message m)
        => GetVideoFileID(m) || GetPhotoFileID(m) || GetAudioFileID(m);
}

public abstract class VideoPhotoCommand : FileEditingCommand
{
    protected override string SuportedMedia => "🎬, 📸";
    protected override bool MessageContainsFile(Message m) 
        => GetVideoFileID(m) || GetPhotoFileID(m);
}

public abstract class AudioVideoCommand : FileEditingCommand
{
    protected override string SuportedMedia => "🎬, 🎧";
    protected override bool MessageContainsFile(Message m) 
        => GetVideoFileID(m) || GetAudioFileID(m);
}

public abstract class AudioVideoUrlCommand : FileEditingCommand
{
    private string? Url;

    protected override string SuportedMedia => "🎬, 🎧, 📎";
    protected override bool MessageContainsFile(Message m)
        => GetVideoFileID(m) || GetAudioFileID(m) || GetVideoURL(m);

    private bool GetVideoURL(Message m)
    {
        var text = m.GetTextOrCaption();
        if (text is null) return false;

        var entity = m.GetURL();
        if (entity is null) return false;

        Url = text.Substring(entity.Offset, entity.Length);
        Type = MediaType.Video;
        Ext = ".mp4";
        return true;
    }

    protected async Task<(string path, int waitMessage)> DownloadFileSuperCool()
    {
        if (Url != null)
        {
            var waitMessage = Bot.PingChat(Origin, PLS_WAIT[Random.Shared.Next(5)]);

            var task = new DownloadVideoTask(Url, Context).RunAsync();
            await Bot.RunOrThrow(task, Chat, waitMessage);

            Bot.EditMessage(Chat, waitMessage, PROCESSING.PickAny().XDDD());

            return (await task, waitMessage);
        }
        else
        {
            var path = await DownloadFile();

            var waitMessage = path.FileSizeInBytes() > 4_000_000
                ? Bot.PingChat(Origin, PROCESSING.PickAny().XDDD())
                : -1;

            return (path, waitMessage);
        }
    }
}