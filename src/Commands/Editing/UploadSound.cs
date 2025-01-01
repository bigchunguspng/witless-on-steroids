using Witlesss.Services.Sounds;

namespace Witlesss.Commands.Editing;

public class UploadSound : AudioCommand
{
    protected override async Task Execute()
    {
        var fileName = Message.GetSongNameOr("Неизвестен - Без названия");
        var text = Path.GetFileNameWithoutExtension(fileName);
        await SoundDB.Instance.UploadSingle(File.FileId, fileName, Origin);
        Bot.SendMessage(Origin, $"Файл был сохранён как:\n\n<code>{text}</code>".XDDD());
        Log($"{Title} >> SOUND UPLOADED [{text}]");
    }
}