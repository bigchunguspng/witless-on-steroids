using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace PF_Bot.Core.Meme.Generators;

public partial class MemeGenerator // SHADOW (THE HEDGEHOG THE ULTIMATE LIFE FORM)
{
    private Image<Rgba32> DrawShadow
    (
        Image<Rgba32> textLayer,
        (float height, float fontSize) top,
        (float height, float fontSize) bottom
    )
    {
        var shadowRealm = new Image<Rgba32>(textLayer.Width, textLayer.Height);

        var pixelated = FontOption.FontIsPixelated();

        var opacity = ShadowOpacity / 100F;
        var maxOpacity = (255 * opacity).RoundInt().ClampByte();

        Func<int, int, double, double> getShadowOpacity = pixelated ? SquareShadow : RoundShadow;

        var sw = Stopwatch_StartNew();

        if (top.height > 0)
        {
            var area = new Rectangle(0, 0, _w, CustomOffsetMode ? _h : _h / 2);
            ShadowImagePart(top.fontSize, area);
        }

        if (bottom.height > 0)
        {
            var area = new Rectangle(0, _h / 2, _w, _h - _h / 2);
            ShadowImagePart(bottom.fontSize, area);
        }

        sw.Log("/meme -> DrawShadow");

        shadowRealm.Mutate(x => x.DrawImage(textLayer));
        textLayer.Dispose();

        return shadowRealm;

        //

        void ShadowImagePart(float fontSize, Rectangle rectangle)
        {
            var w = Math.Sqrt(fontSize) / (pixelated ? 1.6F : 2F);
            var w2 = (int)Math.Ceiling(w) + 2;

            var width  = textLayer.Width;
            var height = textLayer.Height;
            
            shadowRealm.ProcessPixelRows(accessor =>
            {
                for (var y = rectangle.Y; y < rectangle.Bottom; y++)
                for (var x = rectangle.X; x < rectangle.Right;  x++)
                {
                    var textA = textLayer[x, y].A;
                    if (textA == 0) continue;

                    for (var ky = y - w2; ky <= y + w2; ky++)
                    {
                        var outsideY = ky < 0 || ky >= height;
                        if (outsideY) continue;

                        var row = accessor.GetRowSpan(ky);

                        for (var kx = x - w2; kx <= x + w2; kx++)
                        {
                            var outsideX = kx < 0 || kx >= width;
                            if (outsideX) continue;

                            ref var pixel = ref row[kx];

                            var shadowA = pixel.A;
                            if (shadowA == maxOpacity) continue;

                            var shadowOpacity = opacity * getShadowOpacity(kx - x, ky - y, w);
                            if (shadowOpacity == 0) continue;

                            var a = Math.Max(shadowA, shadowOpacity * textA).RoundInt().ClampByte();
                            pixel = new Rgba32(0, 0, 0, a);
                        }
                    }
                }
            });
        }
    }

    private double RoundShadow(int kx, int ky, double w)
    {
        var r = Math.Sqrt(kx * kx + ky * ky);
        return Math.Clamp(1 - 2 * (r - w), 0, 1);
    }

    private double SquareShadow(int kx, int ky, double w)
    {
        var x = Math.Abs(kx);
        var y = Math.Abs(ky);
        var b = x > 0 && x < w && y > 0 && y < w;
        return b ? 1 : 0;
    }
}