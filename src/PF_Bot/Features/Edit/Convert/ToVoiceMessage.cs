using PF_Bot.Features.Edit.Core;
using PF_Bot.Features.Edit.Shared;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Edit.Convert;

public class ToVoiceMessage : AudioVideoCommand
{
    protected override async Task Execute()
    {
        var input = await DownloadFile();

        string output;
        var probe = await FFProbe.Analyze(input);
        if (probe.HasAudio)
        {
            output = EditingHelpers.GetOutputFilePath(input, "voice", ".ogg");

            await FFMpeg.Command(input, output, FFMpegOptions.Out_VOICE_MESSAGE).FFMpeg_Run();
        }
        else
            output = File_DefaultVoiceMessage;

        await using var stream = System.IO.File.OpenRead(output);
        Bot.SendVoice(Origin, InputFile.FromStream(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}