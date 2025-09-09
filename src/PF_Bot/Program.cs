using System.Diagnostics;
using PF_Bot.Core;
using PF_Bot.Core.Text;
using PF_Bot.Telegram;

namespace PF_Bot
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Config.ReadFromFile();
            Bot.LaunchInstance(args.FirstOrDefault());
        }
    }
}