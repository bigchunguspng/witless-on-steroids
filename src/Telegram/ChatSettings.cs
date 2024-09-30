using Newtonsoft.Json;
using Witlesss.Commands.Meme.Core;

namespace Witlesss.Telegram;

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

    public MemeOptions GetMemeOptions() => Options ??= new MemeOptions();
}

public class MemeOptions
{
    [JsonProperty] public string? Meme { get; set; }
    [JsonProperty] public string? Top  { get; set; }
    [JsonProperty] public string? Dp   { get; set; }
    [JsonProperty] public string? Dg   { get; set; }
    [JsonProperty] public string? Nuke { get; set; }

    public string? this [MemeType type]
    {
        get => type switch
        {
            MemeType.Meme => Meme,
            MemeType.Top  => Top,
            MemeType.Dg   => Dg,
            MemeType.Dp   => Dp,
            _             => Nuke,
        };
        set
        {
            if      (type is MemeType.Meme) Meme = value;
            else if (type is MemeType.Top)  Top  = value;
            else if (type is MemeType.Dp )  Dp   = value;
            else if (type is MemeType.Dg )  Dg   = value;
            else                            Nuke = value;
        }
    }

    public bool IsEmpty() => Meme is null && Top is null && Dp is null && Dg is null && Nuke is null;
}