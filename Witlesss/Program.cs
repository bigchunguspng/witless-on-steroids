using System;
using System.Collections.Generic;
using Witlesss.Commands.Routing;
using Witlesss.Generation;

namespace Witlesss
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            var paths = new[]
            {
                @"C:\piece_fap_bot\DB\Board\a - 2023-12-12 21.59.json",
                @"C:\piece_fap_bot\DB\Board\a - 2023-11-22 14-24.json",
                @"C:\piece_fap_bot\DB\Board\a - 2024-03-19 14.20.json",
                @"C:\piece_fap_bot\DB\Board\a - 2024-06-01 12.23.json"
            };
            var data = new List<string>();
            
            var sw = Helpers.GetStartedStopwatch();
            foreach (var path in paths)
            {
                data.AddRange(new FileIO<List<string>>(path).LoadData());
            }
            sw.Log($"data loaded: {data.Count} lines");

            var cp1 = new Copypaster();
            var cp2 = new Copypaster2();

            Console.ReadKey();
            sw.Restart();
            foreach (var line in data)
            {
                cp1.Eat(line, out _);
            }
            sw.Log("db1");

            Console.ReadKey();
            sw.Restart();
            foreach (var line in data)
            {
                cp2.Eat(line, out _);
            }
            sw.Log("db2");

            Console.ReadKey();

            Console.WriteLine(cp1.Words.Count);
            Console.WriteLine(cp2.DB.Vocabulary.Count);

            Console.ReadKey();
            while (true)
            {
                Console.ReadKey();
                sw.Restart();
                for (var i = 0; i < 50; i++)
                {
                    try
                    {
                        cp1.Generate(Copypaster.START);
                    }
                    catch
                    {
                        //
                    }
                    
                }
                sw.Log("cp1.gen");
                
                Console.ReadKey();
                sw.Restart();
                for (var i = 0; i < 50; i++)
                {
                    try
                    {
                        cp2.Generate();
                    }
                    catch
                    {
                        //
                    }
                }
                sw.Log("cp2.gen");
            }
            return;
            Config.ReadFromFile();
            Bot.LaunchInstance(args.Length > 0 ? new Skip() : new CommandRouter());
        }
    }
}