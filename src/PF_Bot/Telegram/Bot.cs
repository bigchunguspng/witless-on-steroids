using PF_Bot.Core;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Telegram;

public partial class Bot
{
    public required ITelegramBotClient Client;
    public required ITelegramBotClient Client_Public; // <- to work with public file ids
    public required User Me;

    /// Lowercase bot username with "@" symbol.
    public required string Username;

    public static async Task<Bot> Create(IMessageRouter rM, ICallbackRouter rC)
    {
        Router_Message  = rM;
        Router_Callback = rC;

        var                 local = Config.TelegramLocalServer;
        var client        = local ? CreateTelegramBotClient_LOCAL () : CreateTelegramBotClient_NORMAL();
        var client_public = local ? CreateTelegramBotClient_NORMAL() : client;

        var me = await client.GetMe_AtAllCost();
        return new Bot
        {
            Client          = client,
            Client_Public   = client_public,
            Me              =     me,
            Username        = $"@{me.Username!.ToLower()}",
        };
    }

    private static ITelegramBotClient CreateTelegramBotClient_NORMAL()
    {
        var options = new TelegramBotClientOptions(Config.TelegramToken)
        {
            RetryThreshold = 300,
            RetryCount = 5,
        };

        return new TelegramBotClient(options)
        {
            Timeout = TimeSpan.FromMinutes(5),
        };
    }

    private static ITelegramBotClient CreateTelegramBotClient_LOCAL()
    {
        WTelegram.Helpers.Log = (_, _) => { };

        var db_con  = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=bot.db");
        var options = new WTelegramBotClientOptions
            (Config.TelegramToken, Config.TelegramApiId, Config.TelegramApiHash, db_con)
        {
            RetryThreshold = 300,
            RetryCount = 5,
        };

        return new WTelegramBotClient(options)
        {
            Timeout = TimeSpan.FromMinutes(10),
        };
    }
}