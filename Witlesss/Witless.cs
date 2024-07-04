﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Witlesss.Backrooms.Types;
using Witlesss.Generation;
using Witlesss.Generation.Pack;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        private const byte MAX_SAVES_SKIP_BEFORE_UNLOAD = 1;

        public Witless(long chat, byte speech = 15, byte pics = 20, byte jpg = 75)
        {
            Chat = chat;
            Speech = speech;
            Pics = pics;
            Quality = jpg;

            FileIO = new FileIO<GenerationPack>(GetPath());
        }

        public static Witless GetAverageBaka(CommandContext context)
        {
            var witless = new Witless(context.Chat)
            {
                Type = (MemeType)Random.Shared.Next(4)
            };

            if (context.ChatIsPrivate)
            {
                witless.Speech = 100;
                witless.Pics = 100;
                witless.Stickers = true;
            }

            return witless;
        }


        // FILE

        private FileIO<GenerationPack> FileIO { get; }

        public string FilePath => FileIO.Path;

        public string GetPath() => Path.Combine(Paths.Dir_Chat, $"{Paths.Prefix_Pack}-{Chat}.json");

        // BAKA

        private Copypaster? _baka;

        public Copypaster Baka
        {
            get
            {
                if (!Loaded) Load();
                return _baka!;
            }
            set => _baka = value;
        }

        // STATE

        private bool Loaded => _baka is not null;

        private bool Dirty  { get => _flags[6]; set => _flags[6] = value; }
        public  bool Banned { get => _flags[7]; set => _flags[7] = value; }


        // DATA

        [JsonProperty] public long Chat { get; set; }

        private ByteFlags _flags;
        private byte _savesSkipped;
        private byte _speech, _pics, _quality;

        [JsonProperty] public bool AdminsOnly
        {
            get => _flags[0];
            set => _flags[0] = Chat.ChatIsNotPrivate() && value;
        }

        [JsonProperty] public byte Speech  { get => _speech;  set => _speech  = value.Clamp100(); }
        [JsonProperty] public byte Pics    { get => _pics;    set => _pics    = value.Clamp100(); }
        [JsonProperty] public byte Quality { get => _quality; set => _quality = value.Clamp100(); }

        [JsonProperty] public MemeType Type { get; set; }

        [JsonProperty] public bool Stickers { get => _flags[1]; set => _flags[1] = value; }

        [JsonProperty] public MemeOptions? Options { get; set; }

        public MemeOptions GetMemeOptions() => Options ??= new MemeOptions();


        // EAT / GENERATE

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text)
            => Dirty = Baka.Eat(text, out _);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text, [NotNullWhen(true)] out string[]? eaten)
            => Dirty = Baka.Eat(text, out eaten);

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


        // SAVE / LOAD

        public void SaveChangesOrUnloadInactive()
        {
            if (!Loaded) return;
            if (Dirty) Save();
            else if (EnoughSavesSkipped()) Unload();
        }

        public void SaveChanges()
        {
            if (Loaded && Dirty) Save();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            FileIO.SaveData(Baka.DB);
            ResetState();
            Log($"DIC SAVED << {Chat}", ConsoleColor.Green);
        }

        private void Load()
        {
            Baka = new Copypaster { DB = FileIO.LoadData() };
            ResetState();
            Log($"DIC LOADED >> {Chat}", ConsoleColor.Magenta);
        }

        private void Unload()
        {
            _baka = null;
            Log($"DIC UNLOAD << {Chat}", ConsoleColor.Yellow);
        }

        private void ResetState()
        {
            Dirty = false;
            _savesSkipped = 0;
        }

        private bool EnoughSavesSkipped()
        {
            _savesSkipped = ((_savesSkipped + 1) % MAX_SAVES_SKIP_BEFORE_UNLOAD).ClampByte();
            return _savesSkipped == 0;
        }

        // FUSE / DELETE

        public void Fuse(GenerationPack pack)
        {
            Backup();
            Baka.Fuse(pack);
        }

        public void BackupAndDelete()
        {
            Backup();
            DeleteForever();
        }

        public void DeleteForever()
        {
            Unload();
            File.Delete(FilePath);
        }

        private void Backup()
        {
            SaveChanges();
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var name = $"{Paths.Prefix_Pack}-{Chat}.json";
            var file = new FileInfo(FilePath);
            file.CopyTo(UniquePath(Path.Combine(Paths.Dir_Backup, date), name));
        }
    }

    public class MemeOptions // todo load from json as null if empty
    {
        [JsonProperty] public string? Meme { get; set; }
        [JsonProperty] public string? Top  { get; set; }
        [JsonProperty] public string? Dp   { get; set; }
        [JsonProperty] public string? Dg   { get; set; }
        [JsonProperty] public string? Nuke { get; set; }
    }
}