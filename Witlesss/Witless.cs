using System;
using System.IO;
using Newtonsoft.Json;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        private int _quality, _chance;
        private bool _admins;

        private FileIO<WitlessDB> FileIO { get; }

        private Counter Generation { get; } = new();
        private Counter Saves      { get; } = new();

        public Witless(long chat, int interval = 7, int pics = 20, int jpg = 75)
        {
            Chat = chat;
            Interval = interval;
            MemeChance = pics;
            MemeQuality = jpg;

            Baka   = new Copypaster(this);
            FileIO = new FileIO<WitlessDB>(Path);

            Saves.Interval = 10;
        }

        public static Witless AverageBaka(long chat)
        {
            var witless = new Witless(chat);

            witless.Load();
            return witless;
        }

        [JsonProperty] public long Chat { get; set; }
        [JsonProperty] public int Interval
        {
            get => Generation.Interval;
            set => Generation.Interval = value;
        }
        [JsonProperty] public int MemeChance
        {
            get => _chance;
            set => _chance = Math.Clamp(value, 0, 100);
        }
        [JsonProperty] public int MemeQuality
        {
            get => _quality;
            set => _quality = Math.Clamp(value, 0, 100);
        }
        [JsonProperty] public MemeType MemeType { get; set; }
        [JsonProperty] public bool MemeStickers { get; set; } // todo move all those meme_props to another class
        [JsonProperty] public bool AdminsOnly
        {
            get => _admins;
            set { if (Chat < 0) _admins = value; }
        }

        public WitlessDB Words { get; set; }
        public Copypaster Baka { get; set; }

        public string Path => $@"{DBS_FOLDER}\{DB_FILE_PREFIX}-{Chat}.json";

        public bool Banned, Loaded, HasUnsavedStuff;

        public void Eat(string text)                   => Baka.Eat(text, out _);
        public bool Eat(string text, out string eaten) => Baka.Eat(text, out eaten);

        public string Generate(string word = Copypaster.START)
        {
            try
            {
                return Baka.Generate(word);
            }
            catch (Exception e)
            {
                LogError(e.Message);
                return "";
            }
        }

        public string GenerateByWord    (string word) => Baka.GenerateByWord    (word);
        public string GenerateByLastWord(string word) => Baka.GenerateByLastWord(word);

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
            FileIO.SaveData(Words);
            HasUnsavedStuff = false;
            Log($"DIC SAVED << {Chat}", ConsoleColor.Green);
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
            Words = null;
            Loaded = false;
            Log($"DIC UNLOAD << {Chat}", ConsoleColor.Yellow);
        }

        public void Backup()
        {
            Save();
            var path = $@"{BACKUP_FOLDER}\{DateTime.Now:yyyy-MM-dd}\{DB_FILE_PREFIX}-{Chat}.json";
            var file = new FileInfo(Path);
            file.CopyTo(UniquePath(path));
        }

        public void Delete()
        {
            Backup();
            File.Delete(Path);
        }
    }
}