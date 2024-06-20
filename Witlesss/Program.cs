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
            sw.Log("EAT: db1");

            Console.ReadKey();
            sw.Restart();
            foreach (var line in data)
            {
                cp2.Eat(line, out _);
            }
            sw.Log("EAT: db2");

            Console.ReadKey();
            Console.WriteLine(cp1.Words.Count);
            Console.WriteLine(cp2.DB.Vocabulary.Count);

            Console.ReadKey();
            sw.Restart();
            new FileIO<WitlessDB>(@"D:\Desktop\db-1.json").SaveData(cp1.Words);
            sw.Log("SAVE: db-1.json");

            Console.ReadKey();
            sw.Restart();
            new FileIO<GenerationPack>(@"D:\Desktop\db-2.json").SaveData(cp2.DB);
            sw.Log("SAVE: db-2.json");

            Console.ReadKey();
            sw.Restart();
            var db1B = new FileIO<WitlessDB>(@"D:\Desktop\db-1.json").LoadData();
            sw.Log("LOAD: db-1.json");

            Console.ReadKey();
            sw.Restart();
            var db2B = new FileIO<GenerationPack>(@"D:\Desktop\db-2.json").LoadData();
            sw.Log("LOAD: db-2.json");

            Console.ReadKey();
            Console.WriteLine(db1B.Count);
            Console.WriteLine(db2B.Vocabulary.Count);

            while (true)
            {
                Console.ReadKey();
                sw.Restart();
                for (var i = 0; i < 5; i++)
                {
                    try
                    {
                        Console.Write(i);
                        Console.Write(". ");
                        Console.WriteLine(cp1.Generate(Copypaster.START));
                    }
                    catch
                    {
                        //
                    }
                    
                }
                sw.Log("cp1.gen");
                
                Console.ReadKey();
                sw.Restart();
                for (var i = 0; i < 5; i++)
                {
                    try
                    {
                        Console.Write(i);
                        Console.Write(". ");
                        Console.WriteLine(cp2.Generate());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
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

/*

"24635": [
    "45": 0.6,
    "17025": 1.0,
    "38411": 1.0,
    "410": 0.5,
    "38869": 1.0
],
 */