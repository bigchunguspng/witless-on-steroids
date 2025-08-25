using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Services.Sounds;

public class SoundDB : MediaDB<Voice>
{
    public static readonly SoundDB Instance = new SoundDB().LoadDB<SoundDB>();

    protected override string Name { get; } = "SoundDB";
    protected override string What { get; } = "SOUNDS";
    protected override string WhatSingle { get; } = "sound";
    protected override string DB_Path { get; } = File_Sounds;

    protected override async Task<Voice> UploadFile(string path, long channel)
    {
        var temp = Path.Combine(Dir_Temp, $"{Guid.NewGuid()}.ogg");
        var opus = await path.UseFFMpeg((0, null)).ToVoice().OutAs(temp);

        await using var stream = File.OpenRead(opus);
        var message = await Bot.Instance.Client.SendVoice(channel, stream);
        return message.Voice!;
    }
}