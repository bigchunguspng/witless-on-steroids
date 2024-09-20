using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Witlesss.Backrooms.Types;
using Witlesss.Commands.Meme.Core;
using Witlesss.Generation;
using Witlesss.Generation.Pack;

namespace Witlesss
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Witless
    {
        private const byte MAX_USELESSNESS_BEFORE_UNLOAD = 10;

        public Witless(long chat, byte speech = 15, byte pics = 20, byte jpg = 75)
        {
            Chat = chat;
            Speech = speech;
            Pics = pics;
            Quality = jpg;
        }

        public static Witless GetAverageBaka(CommandContext context)
        {
            var chance = Random.Shared.Next(4);
            var type = chance.IsEven()
                ? MemeType.Meme
                : (chance >> 1).IsEven()
                    ? MemeType.Dg
                    : MemeType.Dp;

            var witless = new Witless(context.Chat) { Type = type };
            if (context.ChatIsPrivate)
            {
                witless.Speech = 100;
                witless.Pics = 150;
                witless.Stickers = true;
            }

            return witless;
        }


        // FILE

        public string FilePath => GetPath();

        public string GetPath() => Path.Combine(Dir_Chat, $"{Prefix_Pack}-{Chat}.json");

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

        public  bool Loaded => _baka is not null;

        private bool Dirty  { get => _flags[6]; set => _flags[6] = value; }
        public  bool Banned { get => _flags[7]; set => _flags[7] = value; }


        // DATA

        [JsonProperty] public long Chat { get; set; }

        private ByteFlags _flags;
        private byte _uselessness;
        private byte _speech, _pics, _quality;

        [JsonProperty] public bool AdminsOnly
        {
            get => _flags[0];
            set => _flags[0] = Chat.ChatIsNotPrivate() && value;
        }

        [JsonProperty] public byte Speech  { get => _speech;  set => _speech  = value.Clamp100(); }
        [JsonProperty] public byte Pics    { get => _pics;    set => _pics    = value.Clamp(150); }
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
        public string Generate
            () => TextOrBust(() => Baka.Generate());

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateBackwards
            () => TextOrBust(() => Baka.GenerateBackwards());

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateByWord(string word) => TextOrBust(() => Baka.GenerateByWord(word));

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateByLast(string word) => TextOrBust(() => Baka.GenerateByLast(word));

        private string TextOrBust(Func<string> generate)
        {
            try
            {
                _uselessness = 0;
                return generate();
            }
            catch
            {
                LogError("NO TEXT!?");
                var response = IsOneIn(5) ? null : DefaultTextProvider.GetRandomResponse();
                return (response ?? Bot.Instance.Me.FirstName).ToRandomLetterCase();
            }
        }


        // SAVE / LOAD

        public void SaveChangesOrUnloadIfUseless()
        {
            if (Loaded)
            {
                if (Dirty) Save();
                else if (IsUselessEnough()) Unload();
            }
        }

        public void SaveChanges()
        {
            if (Loaded && Dirty) Save();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            JsonIO.SaveData(Baka.DB, GetPath());
            ResetState();
            Log($"DIC SAVED << {Chat}", ConsoleColor.Green);
        }

        private void Load()
        {
            Baka = new Copypaster { DB = JsonIO.LoadData<GenerationPack>(GetPath()) };
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
            _uselessness = 0;
        }

        private bool IsUselessEnough()
        {
            var limitReached = _uselessness + 1 >= MAX_USELESSNESS_BEFORE_UNLOAD;
            if (limitReached)  _uselessness = 0;

            return limitReached;
        }

        // FUSE / DELETE

        [MethodImpl(MethodImplOptions.Synchronized)]
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
            if (Baka.DB.Vocabulary.Count == 0) return; // don't backup empty ones

            SaveChanges();
            var file = new FileInfo(FilePath);
            if (file.Length >= 4_000_000) return; // don't backup big ones

            var date = DateTime.Now.ToString("yyyy-MM-dd");
            var name = $"{Prefix_Pack}-{Chat}.json";
            file.CopyTo(UniquePath(Path.Combine(Dir_Backup, date), name));
        }
    }

    public class MemeOptions
    {
        [JsonProperty] public string? Meme { get; set; }
        [JsonProperty] public string? Top  { get; set; }
        [JsonProperty] public string? Dp   { get; set; }
        [JsonProperty] public string? Dg   { get; set; }
        [JsonProperty] public string? Nuke { get; set; }

        public string? this [MemeType type]
        {
            get => type switch
            {
                MemeType.Meme => Meme,
                MemeType.Top  => Top,
                MemeType.Dg   => Dg,
                MemeType.Dp   => Dp,
                _             => Nuke,
            };
            set
            {
                if      (type is MemeType.Meme) Meme = value;
                else if (type is MemeType.Top)  Top  = value;
                else if (type is MemeType.Dp )  Dp   = value;
                else if (type is MemeType.Dg )  Dg   = value;
                else                            Nuke = value;
            }
        }

        public bool IsEmpty() => Meme is null && Top is null && Dp is null && Dg is null && Nuke is null;
    }
}