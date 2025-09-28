using PF_Tools.Copypaster.TransitionTables;

namespace PF_Tools.Copypaster;

/// Text generation data.
/// Existing words and transitions chances between them.
public class GenerationPack
{
    public  const string S_REMOVED = "[R]";
    private const int      REMOVED = -8;

    public const int START = -5, END = -3, NO_WORD = -1;

    // DATA

    private readonly List                    <string> _vocabulary         = [];    // list index is word id
    private readonly List           <TransitionTable> _transitionsOrdinal = [];    // list index is word id
    private readonly Dictionary<int, TransitionTable> _transitionsSpecial = new(); //        key is word id

    private Dictionary<string, int>? _wordIds; // index of word ids by the word itself // todo have this instead of vocab?

    public GenerationPack() { }
    public GenerationPack
    (
        List<string> vocabulary,
        List           <TransitionTable> transitionsOrdinal,
        Dictionary<int, TransitionTable> transitionsSpecial
    )
    {
        _vocabulary = vocabulary;
        _transitionsOrdinal = transitionsOrdinal;
        _transitionsSpecial = transitionsSpecial;
    }

    // READS (simple)

    public int  VocabularyCount => _vocabulary.Count;
    public int TransitionsCount => _transitionsSpecial.Count + _transitionsOrdinal.Count;
    public int     SpecialCount => _transitionsSpecial.Count;
    public int     OrdinalCount => _transitionsOrdinal.Count; // should be equal to VocabularyCount

    public IEnumerable<string>
        Vocabulary => _vocabulary.AsEnumerable();

    public IEnumerable<KeyValuePair<int, TransitionTable>>
        Transitions => _transitionsSpecial.Concat
        (_transitionsOrdinal.Select((table, i) => new KeyValuePair<int, TransitionTable>(i, table)));

    public IEnumerable                  <TransitionTable>  TransitionsOrdinal => _transitionsOrdinal;
    public IEnumerable<KeyValuePair<int, TransitionTable>> TransitionsSpecial => _transitionsSpecial;

    // READS
    
    /// Use with existing ids!
    /// Returns null if word is invisible, e.g. start or end markers.
    public string? GetWord(int id)
    {
        return id < 0
            ? id == REMOVED
                ? S_REMOVED
                : null
            : _vocabulary[id];
    }

    /// Use with existing ids!
    public TransitionTable GetTransitionTable(int id)
    {
        return id < 0
            ? _transitionsSpecial[id]
            : _transitionsOrdinal[id];
    }

    /// Returns <see cref="NO_WORD"/> if the word doesn't exist.
    public int GetWordId(string word)
    {
        if (word.Equals(S_REMOVED)) return REMOVED;

        var useIndex = _vocabulary.Count > 16;
        if (useIndex && _wordIds is null)
        {
            IndexWordIds();
        }

        return useIndex
            ? _wordIds!.GetValueOrDefault(word, NO_WORD)
            : _vocabulary.IndexOf(word);
    }

    private void IndexWordIds()
    {
        _wordIds = _vocabulary.Select((word, i) => new KeyValuePair<string, int>(word, i)).ToDictionary();
    }

    // UPDATES

    /// Adds word to vocabulary if nessesary.
    /// Returns word id.
    public int TryAddWord_GetWordId(string word)
    {
        var id = GetWordId(word);
        if (id == NO_WORD)
        {
            id = _vocabulary.Count;
            _vocabulary.Add(word);
            _wordIds?.Add(word, id);
        }

        return id;
    }

    /// Increases transition between two words by a given chance. 
    public void PutTransition(int fromId, int toId, int chance)
    {
        var table = GetTransitionTableOrNull(fromId);
        if (table is null)
        {
            table = new TransitionTableC1();
            AssignTransitionTable(fromId, table); // add
        }
        else if (table.ShouldBeUpgradedToPut(toId))
        {
            table = table.Upgrade();
            AssignTransitionTable(fromId, table); // set
        }

        table.Put(toId, chance);
    }

    private TransitionTable? GetTransitionTableOrNull(int id)
    {
        return id < 0
            ? _transitionsSpecial.GetValueOrDefault(id)
            : id < _transitionsOrdinal.Count
                ?  _transitionsOrdinal[id]
                : null;
    }

    private void AssignTransitionTable(int id, TransitionTable table)
    {
        if      (id < 0)
            _transitionsSpecial[id] = table;
        else if (id < _transitionsOrdinal.Count)
            _transitionsOrdinal[id] = table;
        else
            _transitionsOrdinal.Add(table);
    }
}