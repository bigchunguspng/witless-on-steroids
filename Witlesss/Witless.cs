﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Witlesss.Generation;
using Witlesss.Generation.Pack;

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
            FileIO = new FileIO<GenerationPack>(Path);

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

        private FileIO<GenerationPack> FileIO { get; }

        private Counter Saves      { get; } = new();
        private Counter Generation { get; } = new();

        public bool Banned, Loaded, HasUnsavedStuff;

        public Copypaster Baka { get; set; }

        public GenerationPack Pack
        {
            get => Baka.DB;
            set => Baka.DB = value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text)
            => HasUnsavedStuff = Baka.Eat(text, out _);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text, [NotNullWhen(true)] out string[]? eaten)
            => HasUnsavedStuff = Baka.Eat(text, out eaten);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string Generate() => TextOrBust(() => Baka.Generate());

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateByWord(string word) => TextOrBust(() => Baka.GenerateByWord(word));

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateByLast(string word) => TextOrBust(() => Baka.GenerateByLast(word));

        private string TextOrBust(Func<string> generate)
        {
            try
            {
                return generate();
            }
            catch
            {
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

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SaveNoMatterWhat()
        {
            FileIO.SaveData(Pack);
            HasUnsavedStuff = false;
            Log($"DIC SAVED << {Chat}", ConsoleColor.Green);
        }

        public void LoadUnlessLoaded()
        {
            if (!Loaded) Load();
        }
        public void Load()
        {
            Pack = FileIO.LoadData();
            Loaded = true;
            Saves.Reset();
            HasUnsavedStuff = false;
            Log($"DIC LOADED >> {Chat}", ConsoleColor.Magenta);
        }

        public void Unload()
        {
            Pack = null!;
            Loaded = false;
            Log($"DIC UNLOAD << {Chat}", ConsoleColor.Yellow);
        }

        public void Fuse(GenerationPack pack)
        {
            Backup();
            Baka.Fuse(pack);
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

        [JsonProperty] public MemeOptions? Options { get; set; }

        public MemeOptions GetMemeOptions() => Options ??= new MemeOptions();
    }

    // todo load from json as null if empty
    public class MemeOptions
    {
        [JsonProperty] public string? Meme { get; set; }
        [JsonProperty] public string? Top  { get; set; }
        [JsonProperty] public string? Dp   { get; set; }
        [JsonProperty] public string? Dg   { get; set; }
        [JsonProperty] public string? Nuke { get; set; }
    }
}
