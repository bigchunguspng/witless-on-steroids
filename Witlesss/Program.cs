using Witlesss.Commands;

namespace Witlesss
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Config.ReadFromFile();
            Bot.LaunchInstance(args.Length > 0 ? new Skip() : new MainJunction());
        }
    }
}