using Telegram.Bot;
using Telegram.Bot.Types;

namespace Witlesss.Services.Sounds;

public class GIF_DB : MediaDB<Animation>
{
    public static readonly GIF_DB Instance = new GIF_DB().LoadDB<GIF_DB>();

    protected override string Name { get; } = "GIF_DB";
    protected override string What { get; } = "GIFS";
    protected override string WhatSingle { get; } = "GIF";
    protected override string DB_Path { get; } = File_GIFs;

    protected override async Task<Animation> UploadFile(string path, long channel)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var temp = Path.Combine(Dir_Temp, $"{name}.mp4");
        var gif = await path.UseFFMpeg((0, null)).RemoveAudio().OutAs(temp);

        await using var stream = File.OpenRead(gif);
        var message = await Bot.Instance.Client.SendAnimation(channel, stream);
        return message.Animation!;
    }
}