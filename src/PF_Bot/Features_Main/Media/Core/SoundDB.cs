using PF_Bot.Core;
using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Main.Media.Core;

public class SoundDB : MediaDB<Voice>
{
    public static readonly SoundDB Instance = new SoundDB().LoadDB<SoundDB>();

    protected override string Name { get; } = "SoundDB";
    protected override string What { get; } = "SOUNDS";
    protected override string WhatSingle { get; } = "sound";
    protected override FilePath DB_Path { get; } = File_Sounds;

    protected override async Task<Voice> UploadFile(FilePath path, long channel)
    {
        var temp = Path.Combine(Dir_Temp, $"{Guid.NewGuid()}.ogg");

        await FFMpeg.Command(path, temp, FFMpegOptions.Out_VOICE_MESSAGE).FFMpeg_Run();

        await using var stream = File.OpenRead(temp);
        var message = await App.Bot.Client.SendVoice(channel, stream);
        return message.Voice!;
    }
}