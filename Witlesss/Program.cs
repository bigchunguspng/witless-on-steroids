namespace Witlesss
{
    public static class Program
    {
        private static void Main()
        {
            Config.ReadFromFile();
            Bot .LaunchInstance();
        }
    }
}