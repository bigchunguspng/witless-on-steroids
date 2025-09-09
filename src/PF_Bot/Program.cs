using System.Diagnostics;
using System.Globalization;
using PF_Bot.Core;
using PF_Bot.Core.Text;
using PF_Bot.Telegram;

namespace PF_Bot
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Config.ReadFromFile();
            Bot.LaunchInstance(args.FirstOrDefault());
        }
    }
}