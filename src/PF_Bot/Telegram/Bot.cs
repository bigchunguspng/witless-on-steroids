using PF_Bot.Core;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace PF_Bot.Telegram;

public partial class Bot
{
    public readonly ITelegramBotClient Client;
    public readonly User Me;

    /// Lowercase bot username with "@" symbol.
    public readonly string Username;

    public static async Task<Bot> Create(IMessageRouter rM, ICallbackRouter rC)
    {
        var client = Config.TelegramLocalServer
            ? CreateTelegramBotClient_LOCAL()
            : CreateTelegramBotClient_NORMAL();

        var me = await client.GetMe_AtAllCost();
        return new Bot(client, me, rM, rC);
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

    private Bot(ITelegramBotClient client, User me, IMessageRouter rM, ICallbackRouter rC)
    {
        Client   = client;
        Me       =     me;
        Username = $"@{me.Username!.ToLower()}";
        Router_Message  = rM;
        Router_Callback = rC;
    }
}