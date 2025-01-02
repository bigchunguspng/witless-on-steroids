using Witlesss.Services.Sounds;

namespace Witlesss.Commands.Editing;

public class UploadSound : AudioCommand
{
    protected override async Task Execute()
    {
        var userText = Args?.ValidFileName();
        var fileName = Message.GetSongNameOr("Неизвестен - Без названия");
        var text = userText ?? Path.GetFileNameWithoutExtension(fileName);
        var name = $"{text}{Ext}";
        await SoundDB.Instance.UploadSingle(File.FileId, name, Origin);
        Bot.SendMessage(Origin, string.Format(FILE_UPLOADED, text));
        Log($"{Title} >> SOUND UPLOADED [{text}]");
    }
}