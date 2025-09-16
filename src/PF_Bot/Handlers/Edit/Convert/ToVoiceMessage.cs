using PF_Bot.Core.Editing;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Handlers.Edit.Convert;

public class ToVoiceMessage : AudioVideoCommand
{
    protected override async Task Execute()
    {
        var input = await DownloadFile();

        var output = File_DefaultVoiceMessage;

        var probe = await FFProbe.Analyze(input);
        if (probe.HasAudio)
        {
            output = input.GetOutputFilePath("voice", ".ogg");

            await FFMpeg.Command(input, output, FFMpegOptions.Out_VOICE_MESSAGE).FFMpeg_Run();
        }

        await using var stream = System.IO.File.OpenRead(output);
        Bot.SendVoice(Origin, InputFile.FromStream(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}