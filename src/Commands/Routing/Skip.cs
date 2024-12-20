﻿using Telegram.Bot.Types;

namespace Witlesss.Commands.Routing;

public class Skip : CommandAndCallbackRouter
{
    protected override void Run()
    {
        Print($"{Context.Title} >> {Context.Text}", ConsoleColor.Gray);
    }

    public override void OnCallback(CallbackQuery query)
    {
        Print(query.Data ?? "-", ConsoleColor.Yellow);
    }
}