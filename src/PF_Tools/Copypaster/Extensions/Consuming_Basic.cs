using PF_Tools.Copypaster.Tokens;

namespace PF_Tools.Copypaster.Extensions;

public static class Consuming_Basic
{
    public static void Eat(this GenerationPack pack, IEnumerable<string> words, int chance)
    {
        // update vocabulary
        var tokens = new LinkedList<IConsumableToken>();
        foreach (var word in words)
        {
            // words: A, B, [R], C
            if (word.Contains(' '))
            {
                var index = word.IndexOf(' ');
                var id1 = pack.TryAddWord_GetWordId(word.Remove(index));
                var id2 = pack.TryAddWord_GetWordId(word.Substring(index + 1));
                var idC = pack.TryAddWord_GetWordId(word);
                tokens.AddLast(new TokenDouble(id1, id2, idC));
            }
            else
                tokens.AddLast(new TokenSingle(pack.TryAddWord_GetWordId(word)));
        }

        // update transitions
        tokens.AddFirst(new TokenSingle(GenerationPack.START));
        tokens.AddLast (new TokenSingle(GenerationPack.END));

        var node = tokens.First!;
        while (node.Next is { } next)
        {
            // ids: -5, A, B, -8, C, -3
            node.Value.RememberTransition(pack, next.Value, chance);
            node = next;
        }
    }

    public static void Fuse(this GenerationPack target, GenerationPack source)
    {
        // update vocabulary
        var ids = source.Vocabulary.Select(target.TryAddWord_GetWordId).ToList();

        // update transitions
        foreach (var table in source.Transitions)
        {
            var fromId = GetNewId(table.Key);
            foreach (var transition in table.Value.AsIEnumerable())
            {
                var toId = GetNewId(transition.WordId);
                target.PutTransition(fromId, toId, transition.Chance);
            }
        }

        int GetNewId(int id) => id < 0 ? id : ids[id];
    }
}