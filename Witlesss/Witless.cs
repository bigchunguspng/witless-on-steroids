using System;
using System.IO;
using Newtonsoft.Json;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        private bool _admins;

        public Witless(long chat, int interval = 7)
        {
            Chat = chat;
            Interval = interval;
            
            Meme   = new MemeSettings();
            Baka   = new Copypaster();
            FileIO = new FileIO<WitlessDB>(Path);

            Saves.Interval = 10;
        }

        public static Witless AverageBaka(CommandContext context)
        {
            var witless = new Witless(context.Chat);

            if (context.ChatIsPrivate)
            {
                witless.Interval = 1;
                witless.Meme.Chance = 100;
                witless.Meme.Stickers = true;
            }
            witless.Meme.Type = (MemeType)Random.Shared.Next(4);

            witless.Load();
            return witless;
        }

        [JsonProperty] public long Chat { get; set; }
        [JsonProperty] public int Interval // todo -> frequency
        {
            get => Generation.Interval;
            set => Generation.Interval = value;
        }
        [JsonProperty] public bool AdminsOnly
        {
            get => _admins;
            set
            {
                if (Chat < 0) _admins = value;
            }
        }
        [JsonProperty] public MemeSettings Meme { get; set; }

        private FileIO<WitlessDB> FileIO { get; }

        private Counter Saves      { get; } = new();
        private Counter Generation { get; } = new();

        public bool Banned, Loaded, HasUnsavedStuff;

        public Copypaster Baka { get; set; }
        public WitlessDB Words
        {
            get => Baka.Words;
            set => Baka.Words = value;
        }

        public bool Eat(string text)                    => HasUnsavedStuff = Baka.Eat(text, out _);
        public bool Eat(string text, out string? eaten) => HasUnsavedStuff = Baka.Eat(text, out eaten);

        public string Generate(string word = Copypaster.START) => TextOrBust(Baka.Generate, word);

        public string GenerateByWord(string word) => TextOrBust(Baka.GenerateByWord, word);
        public string GenerateByLast(string word) => TextOrBust(Baka.GenerateByLast, word);

        private string TextOrBust(Func<string, string> genetare, string word)
        {
            try
            {
                return genetare(word);
            }
            catch
            {
                //LogError($"{Command.LastChat.Title} >> NO TEXT!?");
                LogError("NO TEXT!?");
                var response = IsOneIn(3) ? null : DefaultTextProvider.GetRandomResponse();
                return (response ?? Bot.Instance.Me.FirstName).ToRandomLetterCase();
            }
        }

        public string Path => System.IO.Path.Combine(Paths.Dir_Chat, $"{Paths.Prefix_Pack}-{Chat}.json");

        public void Count() => Generation.Count();
        public bool Ready() => Generation.Ready();

        public void SaveAndCount()
        {
            if (HasUnsavedStuff)
            {
                SaveNoMatterWhat();
                Saves.Reset();
            }
            else if (Loaded)
            {
                Saves.Count();
                if (Saves.Ready()) Unload();
            }
        }
        public void Save()
        {
            if (HasUnsavedStuff) SaveNoMatterWhat();
        }

        public void SaveNoMatterWhat()
        {
            lock (Words.Sync) FileIO.SaveData(Words);
            HasUnsavedStuff = false;
            Log($"DIC SAVED << {Chat}", ConsoleColor.Green);
        }

        public void LoadUnlessLoaded()
        {
            if (!Loaded) Load();
        }
        public void Load()
        {
            Words = FileIO.LoadData();
            Loaded = true;
            Saves.Reset();
            HasUnsavedStuff = false;
            Log($"DIC LOADED >> {Chat}", ConsoleColor.Magenta);
        }

        public void Unload()
        {
            Words = null!;
            Loaded = false;
            Log($"DIC UNLOAD << {Chat}", ConsoleColor.Yellow);
        }

        public void Backup()
        {
            Save();
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var name = $"{Paths.Prefix_Pack}-{Chat}.json";
            var file = new FileInfo(Path);
            file.CopyTo(UniquePath(System.IO.Path.Combine(Paths.Dir_Backup, date), name));
        }

        public void Delete()
        {
            Backup();
            DeleteForever();
        }

        public void DeleteForever() => File.Delete(Path);
    }

    public class MemeSettings
    {
        private int _quality, _chance;

        public MemeSettings(int pics = 20, int jpg = 75)
        {
            Chance = pics;
            Quality = jpg;
        }
        
        [JsonProperty] public int Chance
        {
            get => _chance;
            set => _chance = Math.Clamp(value, 0, 100);
        }
        [JsonProperty] public int Quality
        {
            get => _quality;
            set => _quality = Math.Clamp(value, 0, 100);
        }
        [JsonProperty] public MemeType Type { get; set; }
        [JsonProperty] public bool Stickers { get; set; }

        [JsonProperty] public string? OptionsM { get; set; }
        [JsonProperty] public string? OptionsT { get; set; }
        [JsonProperty] public string? OptionsD { get; set; }
        [JsonProperty] public string? OptionsG { get; set; }
        [JsonProperty] public string? OptionsN { get; set; }
    }
}
