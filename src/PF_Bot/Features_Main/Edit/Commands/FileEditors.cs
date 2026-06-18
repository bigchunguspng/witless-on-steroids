namespace PF_Bot.Features_Main.Edit.Commands;

public abstract class FileEditor_Video : FileEditor_Core
{
    protected override SupportedFileTypes SupportedTypes
        => SupportedFileTypes.Video;
}

public abstract class FileEditor_Photo : FileEditor_Core
{
    protected override SupportedFileTypes SupportedTypes
        => SupportedFileTypes.Photo;
}

public abstract class FileEditor_Audio : FileEditor_Core
{
    protected override SupportedFileTypes SupportedTypes
        => SupportedFileTypes.Audio;
}

public abstract class FileEditor_AudioVideoPhoto : FileEditor_Core
{
    protected override SupportedFileTypes SupportedTypes
        => SupportedFileTypes.Video
         | SupportedFileTypes.Photo
         | SupportedFileTypes.Audio;
}

public abstract class FileEditor_VideoPhoto : FileEditor_Core
{
    protected override SupportedFileTypes SupportedTypes
        => SupportedFileTypes.Video
         | SupportedFileTypes.Photo;
}

public abstract class FileEditor_AudioVideo : FileEditor_Core
{
    protected override SupportedFileTypes SupportedTypes
        => SupportedFileTypes.Video
         | SupportedFileTypes.Audio;
}

public abstract class FileEditor_AudioVideoUrl : FileEditor_Core
{
    protected override SupportedFileTypes SupportedTypes
        => SupportedFileTypes.Video
         | SupportedFileTypes.Audio
         | SupportedFileTypes.URL;
}