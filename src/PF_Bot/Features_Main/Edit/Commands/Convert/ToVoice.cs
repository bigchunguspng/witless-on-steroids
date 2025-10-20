using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Convert;

public class ToVoice : FileEditor_AudioVideo
{
    protected override async Task Execute()
    {
        var input = await GetFile();

        var output = File_DefaultVoiceMessage;

        var probe = await FFProbe.Analyze(input);
        if (probe.HasAudio)
        {
            output = input.GetOutputFilePath("voice", ".ogg");

            await FFMpeg.Command(input, output, FFMpegOptions.Out_VOICE_MESSAGE).FFMpeg_Run();
        }

        SendFile(output, MediaType.Voice, "balls.ogg");
        Log($"{Title} >> VOICE ~|||~");
    }
}