using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using PF_Bot.Telegram;
using PF_Tools.Backrooms.Helpers;
using PF_Tools.Copypaster;
using PF_Tools.Copypaster.Extensions;

namespace PF_Bot.State.Generation
{
    /// Thread safe <see cref="GenerationPack"/> wrapper.
    /// Tracks changes and usage.
    public class Copypaster(GenerationPack pack) // 32 (26) bytes
    {
        public GenerationPack Pack { get; private set; } = pack;

        /// True if <see cref="Pack"/> content was modified.
        public bool IsDirty { get; private set; }

        /// Resets to 0 after every usage (read or write).
        public byte Idle    { get; private set; }

        public int  VocabularyCount => Pack.VocabularyCount;

        /// Replaces wrapped <see cref="Pack"/> with a new empty one.
        public void ClearPack()
        {
            Pack = new GenerationPack();
            IsDirty = true;
        }

        public void ResetDirty() => IsDirty = false;
        public void BumpIdle  () => Idle++;

        // EAT

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text)
            => EatIfTasty(() => Pack.Eat_Advanced(text), out _);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Eat(string text, [NotNullWhen(true)] out string[]? eaten)
            => EatIfTasty(() => Pack.Eat_Advanced(text), out eaten);

        private bool EatIfTasty(Func<string[]?> eat, out string[]? eaten)
        {
            eaten = eat();
            var success = eaten != null;
            if (success)
            {
                IsDirty = true;
                Idle = 0;
            }
            return success;
        }

        // GENERATE

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string Generate
            () => TextOrBust(() => Pack.RenderText(Pack.Generate()));

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateBackwards
            () => TextOrBust(() => Pack.RenderText(Pack.GenerateBackwards()));

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateByWord(string word) => TextOrBust(() => Pack.GenerateByWord(word));

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GenerateByLast(string word) => TextOrBust(() => Pack.GenerateByLast(word));

        private string TextOrBust(Func<string> generate)
        {
            try
            {
                Idle = 0;
                return generate();
            }
            catch // todo move calls to DefaultTextProvider elsewhere?
            {
                LogError("NO TEXT!?");
                var response = IsOneIn(8) ? null : DefaultTextProvider.GetRandomResponse();
                return (response ?? Bot.Instance.Me.FirstName).ToRandomLetterCase();
            }
        }

        // FUSE

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Fuse(GenerationPack pack)
        {
            Pack.Fuse(pack);
            IsDirty = true;
        }
    }
}