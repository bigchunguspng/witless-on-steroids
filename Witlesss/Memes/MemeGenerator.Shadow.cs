using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Witlesss.Memes;

public partial class MemeGenerator // SHADOW (THE HEDGEHOG THE ULTIMATE LIFE FORM)
{
    private Image<Rgba32> DrawShadow(Image<Rgba32> image, Size? top, Size? bottom, float avgTextHeight)
    {
        var shadowRealm = new Image<Rgba32>(image.Width, image.Height);

        var nokia = GetFontFamily().Name.Contains("Nokia");

        var w = avgTextHeight / (nokia ? 12D : 15D);
        var w2 = (int)Math.Ceiling(w) + 2;

        var opacity = ShadowOpacity / 100F;
        var maxOpacity = (255 * opacity).RoundInt().ClampByte();

        Func<int, int, double, double> getShadowOpacity = nokia ? SquareShadow : RoundShadow;

        var sw = Helpers.GetStartedStopwatch();

        if (top.HasValue)
        {
            var x = (_w - top.Value.Width) / 2;
            ShadowImagePart(new Rectangle(new Point(x, 0), top.Value));
        }

        if (bottom.HasValue)
        {
            var x = (_w - bottom.Value.Width) / 2;
            var y = _h - bottom.Value.Height;
            ShadowImagePart(new Rectangle(new Point(x, y), bottom.Value));
        }

        sw.Log("DrawShadow");

        shadowRealm.Mutate(x => x.DrawImage(image));
        image.Dispose();
        return shadowRealm;

        //

        void ShadowImagePart(Rectangle rectangle)
        {
            for (var y = rectangle.Y; y < rectangle.Bottom; y++)
            for (var x = rectangle.X; x < rectangle.Right; x++)
            {
                var textA = image[x, y].A;
                if (textA == 0) continue;

                for (var ky = y - w2; ky <= y + w2; ky++)
                for (var kx = x - w2; kx <= x + w2; kx++)
                {
                    var sx = kx - x;
                    var sy = ky - y;

                    var outsideImage = kx < 0 || kx >= image.Width || ky < 0 || ky >= image.Height;
                    if (outsideImage) continue;

                    var shadowA = shadowRealm[kx, ky].A;
                    if (shadowA == maxOpacity) continue;

                    var shadowOpacity = opacity * getShadowOpacity(sx, sy, w);
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