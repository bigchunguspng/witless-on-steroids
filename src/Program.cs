﻿namespace Witlesss
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