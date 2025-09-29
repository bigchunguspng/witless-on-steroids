namespace PF_Tools.NZB;

/// <b>Non-Zero Bytes.</b> Custom compression algorithm.
/// Intended to be used with "arrays" of 32-bit items (<see cref="Int32"/> and <see cref="UInt32"/>).
/// Data is stored binary in chunks. Each chunk cover up to 65 536 items and consists of: <ul>
/// <li><b>Count</b> (16 bit)
/// - number of additional items in chunk (0 = 1 item).</li>
/// <li><b>ZBCs</b> (<i>Zero Bytes Counts</i>, up to 16KB)
/// - array of 2-bit values to store the number of skipped bytes for each item.</li>
/// <li><b>NZBs</b> (<i>Non-Zero Bytes</i>, up to 256KB, usually 95-135KB)
/// - compacted bytes of the items.</li></ul><br/>
/// Use <see cref="NzbReader"/> and <see cref="NzbWriter"/> for respective operations.
public static class NZB
{
    public const int
        CHUNK_ITEMS_MAX    = 256 * 256,
        ZBCS_BUFFER_LENGTH = 256 * 256 / 4, //  16KB
        NZBS_BUFFER_LENGTH = 256 * 256 * 4; // 256KB
}