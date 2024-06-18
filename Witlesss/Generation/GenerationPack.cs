using System;
using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;

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
    //public Dictionary<string, int> Vocabulary;

    /// <summary>
    /// A data structure that represents all possible transitions
    /// for each word by it's index in the <see cref="Vocabulary"/>.
    /// </summary>
    public readonly Dictionary<int, TransitionTable> Transitions = new();
    // should be changed from list to dic on 16 elements list.Count == 16

    private Dictionary<string, int>? Index;

    private void IndexVocabulary()
    {
        Index = new Dictionary<string, int>();
        for (var i = 0; i < Vocabulary.Count; i++)
        {
            Index.Add(Vocabulary[i], i);
        }
    }

    public int GetWordID(string word)
    {
        var useIndex = Vocabulary.Count > 16;
        if (useIndex && Index is null)
        {
            IndexVocabulary();
        }

        var id = useIndex ? Index!.GetValueOrDefault(word, -1) : Vocabulary.IndexOf(word);
        if (id < 0) // ADD WORD
        {
            id = Vocabulary.Count;
            Vocabulary.Add(word);
            Index?.Add(word, id);
        }

        return id;
    }

    public string GetWordByID(int id)
    {
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

public class TransitionTable() : List<Transition>(1) // TransitionTable 4.70 MB (42.6K objects => )
{
    //public List<Transition> Transitions { get; } = new(1); // Transition[] 2.10 MB, List<Transition> 3.40 MB

    public float TotalChance { get; private set; }

    public void Put(int id, float chance)
    {
        var index = FindIndex(x => x.WordID == id);
        if (index < 0)
        {
            if (Capacity == Count)
                Capacity = Math.Max(Capacity * 5 >> 2, Capacity + 1);
            Add(new Transition(id, chance));
        }
        else
        {
            this[index].IncreaseChanceBy(chance);
        }

        TotalChance = TotalChance.CombineRound(chance);
    }
}

public struct Transition(int wordID, float chance)
{
    public int   WordID { get; }              = wordID;
    public float Chance { get; private set; } = chance;

    public void IncreaseChanceBy(float value) => Chance = Chance.CombineRound(value);
}
/*public interface ITransitionTable
{
    public IEnumerable<float> Weights { get; }

    public float WeightSum { get; }

    public int GetWordID(int index);

    public void AddOrUpdate(int wordID, float weight);
}

public class DictionaryTransitionTable : Dictionary<int, float>, ITransitionTable
{
    public float WeightSum { get; private set; }
    public IEnumerable<float> Weights => Values;

    public void AddOrUpdate(int wordID, float weight)
    {
        if (!TryAdd(wordID, weight)) this[wordID] = this[wordID].CombineRound(weight);
        WeightSum = WeightSum.CombineRound(weight);
    }

    public int GetWordID(int index) => this.ElementAt(index).Key;
}

public class ListTransitionTable : List<int>, ITransitionTable
{
    private readonly List<float> _weights = [];

    public float WeightSum { get; private set; }

    public IEnumerable<float> Weights => _weights;

    public void AddOrUpdate(int wordID, float weight)
    {
        var index = IndexOf(wordID);
        if (index > 0)
        {
            _weights[index] = _weights[wordID].CombineRound(weight);
        }
        else
        {
            Add(wordID);
            _weights.Add(weight);
        }

        WeightSum = WeightSum.CombineRound(weight);
    }

    public int GetWordID(int index) => this[index];
}*/