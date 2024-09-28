using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Witlesss.Generation;
using Witlesss.Generation.Pack;

namespace Witlesss
{
    /// <summary>
    /// Thread safe <see cref="Generation.Copypaster"/> wrapper attached to a Telegram chat.
    /// </summary>
    public class CopypasterProxy // 40 (34) bytes
    {
        private const byte MAX_USELESSNESS_BEFORE_UNLOAD = 10;

        public CopypasterProxy(long chat)
        {
            Chat = chat;
            Baka = new Copypaster { DB = JsonIO.LoadData<GenerationPack>(FilePath) };
        }

        public  int    WordCount => Baka.DB.Vocabulary.Count;
        private string FilePath  => ChatService.GetPath(Chat);

        public Copypaster Baka { get; set; }
        public long       Chat { get; set; }

        private bool _dirty;
        private byte _uselessness;


        // EAT / GENERATE

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text)
            => _dirty = Baka.Eat(text, out _);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text, [NotNullWhen(true)] out string[]? eaten)
            => _dirty = Baka.Eat(text, out eaten);

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

        // FUSE

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Fuse(GenerationPack pack)
        {
            ChatService.BackupPack(Chat);
            Baka.Fuse(pack);
        }


        // SAVE

        public void SaveChanges()
        {
            if (_dirty) Save();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            JsonIO.SaveData(Baka.DB, FilePath);
            ResetState();
            Log($"DIC SAVED << {Chat}", ConsoleColor.Green);
        }

        private void ResetState()
        {
            _dirty = false;
            _uselessness = 0;
        }

        public bool IsUselessEnough()
        {
            var yes = ++_uselessness >= MAX_USELESSNESS_BEFORE_UNLOAD;
            if (yes)    _uselessness = 0;

            return yes;
        }
    }
}