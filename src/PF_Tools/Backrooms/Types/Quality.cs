namespace PF_Tools.Backrooms.Types;

/// Media file quality (0-100%).
public readonly struct Quality(byte value)
{
    public byte Value { get; } = value.Clamp100();
    
    public static implicit operator byte(Quality quality) => quality.Value;
    public static implicit operator Quality(byte quality) => new (quality);

    /// FFMpeg Constant Rate Factor (for MP4 compression).<br/>
    /// 0 - lossless, 23 - default, 51 - worst possible.<br/>
    /// 0% -> 51, 50% -> 34, 75% -> 25, 80+% -> 23.
    public int GetCRF()
    {
        var x = 51 - (int)(0.35 * Value); // 16 - 51
        return Math.Clamp(x, 23, 51);
    }

    /// FFMpeg quality for JPEG image or MP3 audio.<br/>
    /// 1 - highest, 2-3 - default (JPEG), 31 - lowest.<br/>
    /// 0% -> 31, 50% -> 17, 75% -> 10, 100% -> 3.
    public int GetQscale()
    {
        return 31 - (int)(0.29 * Value); // 2 - 31
    }

    /// FFMpeg quality for WEBP image.<br/>
    /// 0 - lowest, 100 - highest.
    public int GetQscale_WEBP()
    {
        return Value;
    }

    /// ImageSharp quality for image saving.<br/>
    /// 1 - lowest, 100 - highest.
    public int GetImageQuality()
    {
        return Math.Clamp((int)Value, 1, 100);
    }

    /// FFMpeg -b:a Nk value, kbps.<br/>
    /// Returns value relative to the original bitrate if it's big enough.
    /// Otherwise: 0% -> 32k, 50% -> 96k, 75% -> 128k, 100% -> 160k. 
    public int GetAudioBitrate_kbps(int bitrate_OG_bps)
    {
        if (bitrate_OG_bps < 32_000) return (int)(32 + 1.28 * Value);

        var x = (int)(bitrate_OG_bps * Value / 100_000F);
        return Math.Max(x, 32);
    }
}