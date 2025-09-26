namespace PF_Bot.Terminal;

public static class Texts
{
    public const string BUENOS_DIAS =
        """
        This is the certified {0} classic!
        =======
        {1} на связи!
        """;

    public const string CONSOLE_MANUAL =
        """
        Console Commands:

        s   - save and exit

        /s  - save packs
        /p  - packs info
        /pp - packs info (full)
        /cc - clear temp files

        /ups - upload sounds [path]
        /upg - upload GIFs   [path]

        +55 - select active chat

        /mg - JSON -> TGP migration (temporary)
        /xp - save pack JSON copy [chat]

        /db - delete blockers
        /DB - delete active chat if blocked
        /ds - delete by size [max size, bytes]
        """;
}