using System;

namespace Witlesss.XD;

public static class Responses
{
    public static readonly string[] FILE_TOO_BIG =
    [
        "пук-среньк...", "много весит 🥺", "тяжёлая штука 🤔", "ого, какой большой 😯", "какой тяжёлый 😩"
    ];
    public static readonly string[] UNKNOWN_CHAT =
    [
        "ты кто?", "я тебя не знаю чувак 😤", "сними маску, я тебя не узнаю", "а ты кто 😲", "понасоздают каналов... 😒"
    ];
    public static readonly string[] NOT_ADMIN =
    [
        "ты не админ 😎", "ты не админ чувак 😒", "попроси админа", "у тебя нет админки 😎", "будет админка - приходи"
    ];
    public static readonly string[] I_FORGOR =
    [
        "Сорян, не помню", "Сорян, не помню такого", "Забыл уже", "Не помню", "Я бы скинул, но уже потерял её"
    ];
    public static readonly string[] PLS_WAIT =
    [
        "жди 😎", "загрузка пошла 😮", "✋ ща всё будет", "принял👌", "ваш заказ принят 🥸", "еду скачивать музон 🛒"
    ];
    public static readonly string[] PROCESSING =
    [
        "идёт обработка...", "вжжжжж...", "брррррр..."
    ];

    public static readonly string[] RANDOM_EMOJI =
    [
        "🔥✍️", "🪵", "😈", "😎", "💯", "📦", "⚙", "🪤", "💡", "🧨", "🫗", "🌭", "☝️",
        "🍒", "🧄", "🍿", "😭", "🪶", "✨", "🍻", "👌", "💀", "🎳", "🗿", "🔧", "🎉", "🎻"
    ];
    public static readonly string[] FAIL_EMOJI_1 = ["😭", "😎", "😙", "☺️", "💀", "😤", "😩"];
    public static readonly string[] FAIL_EMOJI_2 = ["😵", "😧", "😨", "😰", "😮", "😲", "💀"];
    public static readonly string[] EMPTY_EMOJI  = ["🐾", "💀", "👻", "💯", "💢", "🗑", "🍽"];

    public static T PickAny<T>(this T[] options) => options[Random.Shared.Next(options.Length)];

    public static string XDDD(this string s) => $"{PickAny(RANDOM_EMOJI)} {s}";

    public static string GetRandomASCII()
    {
        return File.ReadAllText(GetFiles(Dir_ASCII).PickAny());
    }
}