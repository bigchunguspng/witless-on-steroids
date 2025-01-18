using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Witlesss.Memes;

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

        var pixelated = FontWizard.FontIsPixelated();

        var opacity = ShadowOpacity / 100F;
        var maxOpacity = (255 * opacity).RoundInt().ClampByte();

        Func<int, int, double, double> getShadowOpacity = pixelated ? SquareShadow : RoundShadow;

        var sw = GetStartedStopwatch();

        if (top.height > 0)
        {
            ShadowImagePart(top.fontSize, new Rectangle(0, 0, _w, GetSafeShadowHeight(top.height)));
        }

        if (bottom.height > 0)
        {
            var height = GetSafeShadowHeight(bottom.height);
            var y = _h - height;
            ShadowImagePart(bottom.fontSize, new Rectangle(0, y, _w, height));
        }

        sw.Log("DrawShadow");

        shadowRealm.Mutate(x => x.DrawImage(textLayer));
        textLayer.Dispose();

        return shadowRealm;

        //

        int GetSafeShadowHeight(float height) => height.RoundInt() + 4 * Math.Max(0, _marginY);

        void ShadowImagePart(float fontSize, Rectangle rectangle)
        {
            var w = Math.Sqrt(fontSize) / (pixelated ? 1.6F : 2F);
            var w2 = (int)Math.Ceiling(w) + 2;

            var width  = textLayer.Width;
            var height = textLayer.Height;

#if DEBUG
            shadowRealm.Mutate(x => x.Draw(Color.Black, 1, rectangle));
#endif
            for (var y = rectangle.Y; y < rectangle.Bottom; y++)
            for (var x = rectangle.X; x < rectangle.Right; x++)
            {
                var textA = textLayer[x, y].A;
                if (textA == 0) continue;

                for (var ky = y - w2; ky <= y + w2; ky++)
                for (var kx = x - w2; kx <= x + w2; kx++)
                {
                    var outsideImage = kx < 0 || kx >= width || ky < 0 || ky >= height;
                    if (outsideImage) continue;

                    var shadowA = shadowRealm[kx, ky].A;
                    if (shadowA == maxOpacity) continue;

                    var shadowOpacity = opacity * getShadowOpacity(kx - x, ky - y, w);
                    if (shadowOpacity == 0) continue;

                    var a = Math.Max(shadowA, shadowOpacity * textA).RoundInt().ClampByte();
                    shadowRealm[kx, ky] = new Rgba32(0, 0, 0, a);
                }
            }
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