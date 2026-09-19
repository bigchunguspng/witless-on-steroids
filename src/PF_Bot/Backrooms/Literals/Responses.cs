using Telegram.Bot.Types;

namespace PF_Bot.Backrooms.Literals;

public static class Responses
{
    public static readonly string[]
        FILE_TOO_BIG =
        [
            "пук-среньк...",
            "много весит 🥺",
            "тяжёлая штука 🤔",
            "ого, какой большой 😯",
            "какой тяжёлый 😩",
        ],
        UNKNOWN_CHAT =
        [
            "ты кто?",
            "я тебя не знаю чувак 😤",
            "сними маску," + "я тебя не узнаю",
            "а ты кто 😲",
            "понасоздают каналов... 😒",
        ],
        NOT_A_CHAT_ADMIN =
        [
            "ты не админ 😎",
            "ты не админ чувак 😒",
            "попроси админа",
            "у тебя нет админки 😎",
            "будет админка - приходи",
        ],
        I_FORGOR =
        [
            "Сорян, не помню",
            "Сорян, не помню такого",
            "Забыл уже",
            "Не помню",
            "Я бы скинул, но уже потерял её",
        ],
        PLS_WAIT =
        [
            "жди 😎",
            "загрузка пошла 😈",
            "✋ ща всё будет",
            "принял👌",
            "ваш заказ принят 🥸",
            "еду скачивать музон 🛒",
        ],
        PROCESSING =
        [
            "идёт обработка...", "вжжжжж...", "брррррр...",
        ],
        FORBIDDEN =
        [
            "LOL", "прикол", "bro thinks he's saul 😭😭💀",
        ],
        RANDOM_EMOJI =
        [
            "🔥✍️", "🪵", "😈", "😎", "💯", "📦", "⚙", "🪤", "💡", "🧨", "🫗", "🌭", "☝️",
            "🍒", "🧄", "🍿", "😭", "🪶", "✨", "🍻", "👌", "💀", "🎳", "🗿", "🔧", "🎉", "🎻",
        ],
        FAIL_EMOJI =
        [
            "😵", "😵‍💫", "😧", "😨", "😰", "😮", "😲", "😳", "💀", "😭", "😔", "😤", "😩", "😫",
        ],
        EMPTY_EMOJI    = ["🐾", "💀", "👻", "💯", "💢", "🗑", "🍽"],
        DONE_REACTIONS = ["👍", "🔥", "🏆", "💯", "⚡", "🍌", "🌭", "😈", "🌚"];

    public static string XDDD(this string text) => $"{RANDOM_EMOJI.PickAny()} {text}";

    public static string GetSillyErrorMessage() => $"произошла ашыпка {FAIL_EMOJI.PickAny()}";

    public static string GetRandomASCII() => File.ReadAllText(Dir_ASCII.GetFiles().PickAny());

    public static ReactionTypeEmoji GetRandomReaction_DONE() => new() { Emoji = DONE_REACTIONS.PickAny() };
}