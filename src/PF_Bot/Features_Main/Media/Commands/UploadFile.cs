using PF_Bot.Features_Main.Edit.Commands;
using PF_Bot.Features_Main.Media.Core;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Features_Main.Media.Commands;

public class UploadFile : AudioVideoCommand
{
    protected override string SyntaxManual => "/man_7";

    protected override async Task Execute()
    {
        if (Type is MediaType.Audio)
        {
            var userText = Args?.ValidFileName();
            var fileName = Message.GetSongNameOr("Неизвестен - Без названия");
            var text = userText ?? Path.GetFileNameWithoutExtension(fileName);
            var name = $"{text}{Ext}";
            await SoundDB.Instance.UploadSingle(File.FileId, name, Origin);
            Bot.SendMessage(Origin, string.Format(SOUND_UPLOADED, text));
            Log($"{Title} >> SOUND UPLOADED [{text}]");
        }
        else
        {
            // todo: crop videonotes ?
            
            var userText = Args?.ValidFileName();
            var fileName = Message.GetAnimationNameOr("Без названия");
            var text = userText ?? Path.GetFileNameWithoutExtension(fileName);
            var name = $"{text}{Ext}";
            await GIF_DB.Instance.UploadSingle(File.FileId, name, Origin);
            Bot.SendMessage(Origin, string.Format(GIF_UPLOADED, text));
            Log($"{Title} >> GIF UPLOADED [{text}]");
        }
    }
}