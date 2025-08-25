using Newtonsoft.Json;
using PF_Bot.Commands.Meme.Core;

namespace PF_Bot.Telegram;

[JsonObject(MemberSerialization.OptIn)]
public class ChatSettings // 32 (28) bytes
{
    private byte _speech, _pics, _quality;

    [JsonProperty] public byte Speech  { get => _speech;  set => _speech  = value.Clamp100(); }
    [JsonProperty] public byte Pics    { get => _pics;    set => _pics    = value.Clamp(150); }
    [JsonProperty] public byte Quality { get => _quality; set => _quality = value.Clamp100(); }

    [JsonProperty] public MemeType Type { get; set; }

    [JsonProperty] public bool Stickers   { get; set; }
    [JsonProperty] public bool AdminsOnly { get; set; }

    [JsonProperty] public MemeOptions? Options { get; set; }

    public MemeOptions GetOrCreateMemeOptions() => Options ??= new MemeOptions();
}

public class MemeOptions
{
    [JsonProperty] public string? Meme { get; set; }
    [JsonProperty] public string? Top  { get; set; }
    [JsonProperty] public string? Dp   { get; set; }
    [JsonProperty] public string? Dg   { get; set; }
    [JsonProperty] public string? Snap { get; set; }
    [JsonProperty] public string? Nuke { get; set; }
    [JsonProperty] public string? Auto { get; set; }

    public string? this [MemeType type]
    {
        get => type switch
        {
            MemeType.Meme => Meme,
            MemeType.Top  => Top,
            MemeType.Dg   => Dg,
            MemeType.Dp   => Dp,
            MemeType.Snap => Snap,
            MemeType.Nuke => Nuke,
            _             => Auto,
        };
        set
        {
            if      (type is MemeType.Meme) Meme = value;
            else if (type is MemeType.Top ) Top  = value;
            else if (type is MemeType.Dp  ) Dp   = value;
            else if (type is MemeType.Dg  ) Dg   = value;
            else if (type is MemeType.Snap) Snap = value;
            else if (type is MemeType.Nuke) Nuke = value;
            else                            Auto = value;
        }
    }

    public bool IsEmpty() =>
        Meme is null
     && Top  is null
     && Dp   is null
     && Dg   is null
     && Snap is null
     && Nuke is null
     && Auto is null;
}