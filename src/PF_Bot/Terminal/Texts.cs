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

        +55 - select active chat

        /a  -  add [text] to active chat pack
        /w  - send [text] to active chat + /a

        /s  - save packs
        /p  - packs info
        /pp - packs info (full)
        /cc - clear temp files

        /mg - JSON -> TGP migration (temporary)
        /xp - [chat/+] export pack to JSON

        /US - [path] upload sounds
        /UG - [path] upload GIFs

        /db - delete blockers
        /DB - delete active chat if blocked

        End input with '_' to discard it.
        """;
}