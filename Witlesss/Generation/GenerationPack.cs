using System;
using System.Collections.Generic;

namespace Witlesss.Generation;

/// <summary>
/// Represents a data used for text generation.
/// </summary>
public class GenerationPack
{
    public const int START = -5, END = -3, REMOVED = -8, NO_WORD = -1;

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
        if (word.Equals("[R]")) return REMOVED;

        var useIndex = Vocabulary.Count > 16;
        if (useIndex && Index is null)
        {
            IndexVocabulary();
        }

        return useIndex ? Index!.GetValueOrDefault(word, NO_WORD) : Vocabulary.IndexOf(word);
    }

    public string? GetWordByID(int id)
    {
        if (id < 0) return id == REMOVED ? Copypaster.LINK : null;

        return Vocabulary[id];
    }

    public TransitionTable GetTableByID(int id)
    {
        if (Transitions.TryGetValue(id, out var table) == false)
        {
            table = new TransitionTable();
            Transitions[id] = table;
        }

        return table;
    }
}

public class TransitionTable(int capacity = 1) : List<Transition>(capacity)
{
    public float TotalChance { get; private set; }

    private void IncreaseTotalChanceBy(float value)
        => TotalChance = TotalChance.CombineRound(value);

    public void Put(int id, float chance)
    {
        var index = FindIndexByID(id);
        if (index < 0)
        {
            if (Capacity == Count)
                Capacity = Math.Max(Capacity * 5 >> 2, Capacity + 1);
            Add(new Transition(id, chance));
        }
        else
        {
            var transition = this[index];
            transition.IncreaseChanceBy(chance);
            this[index] = transition;
            IncreaseTotalChanceBy(chance);
        }
    }

    public new void Add(Transition transition)
    {
        base.Add(transition);
        IncreaseTotalChanceBy(transition.Chance);
    }

    private int FindIndexByID(int id)
    {
        var i = 0;
        foreach (var transition in this)
        {
            if (transition.WordID == id) return i;
            i++;
        }

        return -1;
    }
}

public struct Transition(int wordID, float chance)
{
    public int   WordID { get; }              = wordID;
    public float Chance { get; private set; } = chance;

    public void IncreaseChanceBy(float value) => Chance = Chance.CombineRound(value);
}