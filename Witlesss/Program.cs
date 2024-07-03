using System;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using Witlesss.Backrooms.Helpers;
using Witlesss.Commands.Routing;
using Witlesss.Memes.Shared;

namespace Witlesss
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            /*var txtasd = """
                         /dp 𝕮𝖔𝖔𝖑 𝓕𝓸𝓷𝓽𝓼 🔥 Generate Online (っ◔◡◔)っ ♥️ ℭ𝔬𝔬𝔩 ♥️ and ⓢⓣⓨⓛⓘⓢⓗ Text Fonts for Instagram with symbols & em🥰jis ☆➀ copy & paste ✓.
                         𝕮𝖔𝖔𝖑 𝓕𝓸𝓷𝓽𝓼 🔥 Generate Online (っ◔◡◔)っ ♥️ ℭ𝔬𝔬𝔩 ♥️ and ⓢⓣⓨⓛⓘⓢⓗ Text Fonts for Instagram with symbols & em🥰jis ☆➀ copy & paste ✓.!!!!!!!!
                         """;
            var text1337 = """
                           Железный закон элит гласит - всякая элита в своем костяке возникает не сама по себе, не путем отбора из низов, а исключительно как продукт другой элиты. Если есть та или иная элита, локальная или глобальная, специализированная или универсальная, обязательно существует другая элита, которая ее создала.
                           """;
            var text = "Анархизм - это такая форма человеческого сосуществования,\nв котором взято всё самое лучшее.";
            var text2 = "Анархизм - это такая форма👌 человеческого сосуществования \ud83d\ude2d\ud83d\ude0e\ud83d\udc4c\nв котором👌взято всё 👌👌 самое лучшее. #\ufe0f\u20e31\ufe0f\u20e3\ud83d\udc4c\ud83c\udfff";
            var textSupaCuuul = EmojiTool.ReplaceEmoji(text, "👌");
            var options = new RichTextOptions(SystemFonts.Get("Arial").CreateFont(36))
            {
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                LineSpacing = 1.2F,
                WordBreaking = WordBreaking.Standard,
                KerningMode = KerningMode.Standard,
                FallbackFontFamilies = ExtraFonts.FallbackFamilies,
            };
            for (var i = 2; i <= 10; i++)
            {
                var chunks = TextMeasuring.MeasureTextSuperCool(textSupaCuuul, options, 36);
                TextMeasuring.DistributeText(chunks, i);
                Console.WriteLine(chunks.FillWith(text));
                Console.WriteLine();
            }

            return;*/
            Config.ReadFromFile();
            Bot.LaunchInstance(args.Length > 0 ? new Skip() : new CommandRouter());
        }
    }
}