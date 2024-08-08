using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Witlesss.Commands.Editing.Core;

public abstract class VideoCommand : FileEditingCommand
{
    protected override string SuportedMedia => "🎬";
    protected override bool MessageContainsFile(Message m) 
        => GetVideoFileID(m);
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
    protected override string SuportedMedia => "🎬, 🎧, 📎";
    protected override bool MessageContainsFile(Message m)
        => GetVideoFileID(m) || GetAudioFileID(m) || GetVideoURL(m);

    private bool GetVideoURL(Message m)
    {
        var text = m.GetTextOrCaption();
        if (text is null) return false;

        var entity = m.GetURL();
        if (entity is null) return false;

        FileID = text.Substring(entity.Offset, entity.Length);
        return true;
    }

    protected async Task<(string path, MediaType type, int waitMessage)> DownloadFileSuperCool()
    {
        if (FileID.StartsWith("http"))
        {
            var waitMessage = Bot.PingChat(Chat, Responses.PLS_WAIT[Random.Shared.Next(5)]);

            var task = new DownloadVideoTask(FileID, Context).RunAsync();
            await Bot.RunOrThrow(task, Chat, waitMessage);

            Bot.EditMessage(Chat, waitMessage, Responses.PROCESSING.PickAny().XDDD());

            return (await task, MediaType.Video, waitMessage);
        }
        else
        {
            var (path, type) = await Bot.Download(FileID, Chat);

            var waitMessage = SizeInBytes(path) > 4_000_000
                ? Bot.PingChat(Chat, Responses.PROCESSING.PickAny().XDDD())
                : -1;

            return (path, type, waitMessage);
        }
    }
}