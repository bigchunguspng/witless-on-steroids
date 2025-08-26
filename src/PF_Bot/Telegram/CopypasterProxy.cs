using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PF_Bot.Generation;
using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Extensions;

namespace PF_Bot.Telegram
{
    /// <summary>
    /// Thread safe <see cref="GenerationPack"/> wrapper attached to a Telegram chat.
    /// </summary>
    public class CopypasterProxy // 40 (34) bytes
    {
        private const byte MAX_USELESSNESS_BEFORE_UNLOAD = 10;

        public CopypasterProxy(long chat)
        {
            Chat = chat;
            Baka = GenerationPackIO.Load(FilePath);
        }

        public  int    WordCount => Baka.VocabularyCount;
        private string FilePath  => ChatService.GetPath(Chat);

        public GenerationPack Baka { get; set; }
        public long           Chat { get; set; }

        private bool _dirty;
        private byte _uselessness;


        // EAT / GENERATE

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text)
            => _dirty = Baka.Eat_Advanced(text, out _);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text, [NotNullWhen(true)] out string[]? eaten)
            => _dirty = Baka.Eat_Advanced(text, out eaten);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string Generate
            () => TextOrBust(() => Baka.RenderText(Baka.Generate()));

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateBackwards
            () => TextOrBust(() => Baka.RenderText(Baka.GenerateBackwards()));

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
                var response = IsOneIn(8) ? null : DefaultTextProvider.GetRandomResponse();
                return (response ?? Bot.Instance.Me.FirstName).ToRandomLetterCase();
            }
        }

        // FUSE

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Fuse(GenerationPack pack) => Baka.Fuse(pack);


        // SAVE

        public void SaveChanges()
        {
            if (_dirty) Save();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Save()
        {
            GenerationPackIO.Save(Baka, FilePath);
            ResetState();
            Log($"DIC SAVE << {Chat}", LogLevel.Info, LogColor.Lime);
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