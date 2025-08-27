using System.Diagnostics;
using PF_Bot.State;
using PF_Bot.State.Generation;
using PF_Bot.Telegram;
using PF_Bot.Tools_Legacy.Technical;
using PF_Tools.Copypaster_Legacy;
using PF_Tools.Copypaster_Legacy.Pack;
using PF_Tools.Copypaster.Extensions;
using PF_Tools.Copypaster.Helpers;

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