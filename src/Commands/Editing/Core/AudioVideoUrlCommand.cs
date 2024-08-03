using Telegram.Bot.Types;

namespace Witlesss.Commands.Editing.Core;

public abstract class AudioVideoUrlCommand : FileEditingCommand
{
    protected override string Manual { get; } = SLICE_MANUAL;

    protected override bool MessageContainsFile(Message m)
    {
        return GetVideoFileID(m) || GetAudioFileID(m) || GetVideoURL(m);
    }

    private bool GetVideoURL(Message m)
    {
        var text = m.GetTextOrCaption();
        if (text is null) return false;

        var entity = m.GetURL();
        if (entity is null) return false;

        FileID = text.Substring(entity.Offset, entity.Length);
        return true;
    }
}