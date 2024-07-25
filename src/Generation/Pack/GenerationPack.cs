using System.Collections.Generic;

namespace Witlesss.Generation.Pack;

/// <summary>
/// Represents a data used for text generation.
/// </summary>
public class GenerationPack
{
    private const string s_REMOVED = "[R]";
    private const int REMOVED = -8;

    public const int START = -5, END = -3, NO_WORD = -1;

    /// <summary>
    /// A list of unique words (and word pairs).
    /// </summary>
    public readonly List<string> Vocabulary = [];

    /// <summary>
    /// A data structure that represents all possible transitions
    /// for each word by it's index in the <see cref="Vocabulary"/>.
    /// </summary>
    public readonly Dictionary<int, TransitionTable> Transitions = new();

    private Dictionary<string, int>? Index;

    private void IndexVocabulary()
    {
        Index = new Dictionary<string, int>();
        for (var i = 0; i < Vocabulary.Count; i++)
        {
            Index.Add(Vocabulary[i], i);
        }
    }

    /// <returns>An ID of existing word or newly added one.</returns>
    public int GetOrAddWord_ReturnID(string word)
    {
        var id = GetID_ByWord(word);
        if (id == NO_WORD)
        {
            id = Vocabulary.Count;
            Vocabulary.Add(word);
            Index?.Add(word, id);
        }

        return id;
    }

    /// <returns>An ID of existing word or <see cref="NO_WORD"/> code.</returns>
    public int GetID_ByWord(string word)
    {
        if (word.Equals(s_REMOVED)) return REMOVED;

        var useIndex = Vocabulary.Count > 16;
        if (useIndex && Index is null)
        {
            IndexVocabulary();
        }

        return useIndex ? Index!.GetValueOrDefault(word, NO_WORD) : Vocabulary.IndexOf(word);
    }

    public string? GetWordByID(int id)
    {
        if (id < 0) return id == REMOVED ? s_REMOVED : null;

        return Vocabulary[id];
    }

    public TransitionTable GetOrAddTable(int id)
    {
        var update = true;

        if      (!Transitions.TryGetValue(id, out var table)) table = new TransitionTableSingle();
        else if (!table.CanAccept(id))                        TransitionTable.Upgrade(ref table);
        else
            update = false;
        if (update) Transitions[id] = table;

        return table;
    }
}