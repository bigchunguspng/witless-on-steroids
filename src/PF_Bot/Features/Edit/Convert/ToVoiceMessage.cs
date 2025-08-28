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
        if (probe.HasAudio())
        {
            output = EditingHelpers.GetOutputFilePath(input, "voice", ".ogg");
            var args = FFMpeg.Args().Input(input).Out(output, o => o.Options("-vn -c:a libopus -b:a 48k"));
            await EditingHelpers.FFMpeg_Run(args);
        }
        else
            output = File_DefaultVoiceMessage;

        await using var stream = System.IO.File.OpenRead(output);
        Bot.SendVoice(Origin, InputFile.FromStream(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}