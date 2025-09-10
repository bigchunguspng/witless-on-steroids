using System.Globalization;

namespace PF_Bot.Core;

public static class Setup
{
    // todo: maybe make it App.Setup(), and have App.Run() also.
    public static void Run()
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        Config.ReadFromFile();

        EmojiTool.Directory_EmojiPNGs = Dir_Emoji;
    }
}