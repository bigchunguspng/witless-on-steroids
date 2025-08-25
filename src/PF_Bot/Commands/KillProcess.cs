
using System.Diagnostics;

namespace PF_Bot.Commands;

public class KillProcess : AsyncCommand
{
    private const    string   NO = "CAACAgIAAxkBAAEEc-dndmzvTuVEQMtDmz7U_q3LfYPIzwACxh0AAuCjggeATxF6DmwtYDYE";
    private const    string  BYE = "CAACAgQAAxkBAAJUGGd2cY6smcUDFpl8WjPfVmaAUnp4AAJYDwACR7e5UyHNO_PeWgkcNgQ";
    private readonly string[] OK =
    [
        "CAACAgQAAxkBAAEEc9Zndmu3qlxcrtF1F3mDa-Nrt375DAACQgADQOKfEqu3FflmJDT8NgQ",
        "CAACAgIAAxkBAAEEc9pndmxazfqs3R7Csa2mv4CBgx0IDwACPwEAAsId5wkO5CMLwftNkTYE",
        "CAACAgIAAxkBAAEEc9xndmx7diny8trp6etYn_Cgv6C16QAC0hkAAoCt4Eu5BK2UlWVS2zYE",
        "CAACAgIAAxkBAAEEc-NndmyzLg5EMR1QRcKa8G2McNLe6AACCSMAAlmMaEgeI5p8MZXe5zYE",
    ];

    protected override async Task Run()
    {
        if (Message.SenderIsBotAdmin() == false) return;

        var name = Args ?? "ffmpeg";
        var process = Process.GetProcessesByName(name).FirstOrDefault();
        if (process != null)
        {
            if (process.Id == Environment.ProcessId)
            {
                Bot.SendSticker(Origin, BYE);
                await Task.Delay(100);
            }

            Log($"{Title} >> KILL {name.ToUpper()}", color: LogColor.Yellow);
            process.Kill();
            Bot.SendSticker(Origin, OK.PickAny());
        }
        else
        {
            Bot.SendSticker(Origin, NO);
        }
    }
}