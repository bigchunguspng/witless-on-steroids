namespace PF_Bot.Backrooms.Literals;

public static partial class Texts
{
    public const string START_RESPONSE =
        """
        ВИРУСНАЯ БАЗА ОБНОВЛЕНА!

        📖 Помощь: /man
        👀 Настройки чата: /chat
        """;

    // SETTINGS

    public const string SET_X_GUIDE =
        """
        {0}: {1}%

        Изменить: <code>/{2} [число]</code>
        """;

    public const string SET_SPEECH_RESPONSE =
        "я буду отвечать на {0}% сообщений";

    public const string SET_PICS_RESPONSE =
        "{0}% пикч будут ставать мемами";

    public const string SET_QUALITY_RESPONSE =
        "качество картинок будет {0}%";

    public const string STICKERS_RESPONSE =
        "cтикеры {0}будут ставать мемами в случайном порядке";

    public const string ADMINS_RESPONSE =
        "Менять настройки смогут {0}";

    public const string SET_MEMES_RESPONSE =
        "Картинки будут превращаться в {0}";

    public const string SET_MEME_OPS_RESPONSE =
        "Опции команды <b>{0}</b> изменены на <b>{1}</b>";

    public const string SET_AUTO_HANDLER_RESPONSE =
        """
        Активировано режим <b>авто-обработки</b>

        {0}
        """;

    public const string SET_AUTO_HANDLER_OPTIONS_RESPONSE =
        """
        Установлен <b>авто-обработчик</b>:

        <code>{0}</code>
        """;

    public const string SET_AUTO_HANDLER_OPTIONS_CLEAR_RESPONSE =
        "<b>Авто-обработчик</b> сброшен";

    public const string SET_AUTO_HANDLER_EMPTY_TIP =
        """
        ⚠️ Чтобы всё работало, установите авто-обработчик
        📖 Гайд по авто-обработке: /man_341
        """;

    public const string GROUPS_ONLY_COMAND =
        "Эта команда только для бесед 🤓";

    public const string WITLESS_ONLY_COMAND =
        """
        Для использования этой команды нужно прописать

        /start{0}
        """;

    // FUSE

    public const string FUSE_SUCCESS_RESPONSE =
        """
        Cловарь беседы <b>{0}</b> обновлён!

        Теперь он весит {1} (+{2})
        Новых слов: {3}
        """;

    public const string FUSE_SOURCE =
        """

        Источник: <b><a href='{0}'>{1}</a></b>
        """;

    public const string FUSE_CHAT_NOT_FOUND =
        "К сожалению, у меня нет словаря этой беседы";

    public const string FUSE_FAIL_BOARD =
        """
        К сожалению, я не нашёл сохранённых обсуждений с таким названием. Пропишите

        <code>/{0} info</code>

        и выберите один из вариантов, или пропишите

        <code>/{0}</code>

        чтобы найти новый материал
        """;

    public const string FUSE_ONLY_ARRAY_JSON =
        "Годятся только <b>JSON</b>-файлы, в виде <b>списка строк</b>, например:";

    public const string MOVING_DONE =
        """
        ♻️ Словарь очищен! *пусто* {0}

        Содержимое {1}! Вернуть:
        <code>/fuse {2}{3}</code>
        """;

    public const string PUB_DONE =
        """
        {0} <b>"{1}"</b> опубликован!

        Проверить: <code>/fuse {2}info</code>
        """;

    public const string PUB_NOT_FOUND =
        """
        {0} не могу найти {1} с таким названием

        Пропишите <code>/fuse {2}info</code>, чтобы посмотреть весь список.
        """;

    public const string TRACTOR_GAME_RULES =
        """
        🎮 <b>SUPER GAMING BATTLE</b> 🎳

        🫵 Одолей меня чтобы удалить словарь

        🏠 - я, 🚜 - ты
        """;

    public const string DEL_SUCCESS_RESPONSE =
        """
        Поздравляю, чат <b>{0}</b> был удалён из списка чатов!

        Словарь был сохранён и может быть восстановлен командой <code>/fuse ! {1}</code>.

        Если хотите начать заново - пропишите /start{2}
        """;

    // LISTING

    public const string USE_ARROWS =
        "\n\nНавигация👇";

    // EDITING

    public const string PROCESS_ERROR_REPORT =
        """
        Ошибка произошла во время выполнения следующей команды:

        {0} {1}

        Если хотите чтоб её пофиксили - скиньте этот файл разрабу вместе с обрабатываемым файлом (づ｡◕‿‿◕｡)づ

        {2}

        Более детальный отчёт (для шарящих юзеров):

        {3}
        """;

    public const string AUTO_FAIL_TYPE =
        "Не удалось найти обработчик для сообщения данного типа {0}";

    public const string PIPE_FAIL_RESOLVE =
        "Не удалось распознать команду: <code>{0}</code>";

    public const string PEG_EXTENSION_MISSING =
        """
        последним аргументом должно быть расширение файла

        лайк если не знал, посмотрим сколько нас 👍
        """;

    public const string NUKE_LOG_EXPLANATION =
        """
        <i>*пусто*</i>

        Тут будет список использованных фильтров для всех вызовов <code>/nuke</code> в этом чате с момента запуска бота 🫢.
        """;

    // EATING INTERNET

    public const string REDDIT_COMMENTS_START =
        """
        НАЧИНАЕМ ПРИЗЫВ СОТОНЫ!!!
        {0}
        """;

    public const string BOARD_START =
        "Начинаю поглощение интернета 😈";

    public const string BOARD_START_EDIT =
        """
        Начинаю поглощение интернета 😈

        Тредов найдено: <b>{0}</b>
        """;

    public const string BOARDS_4CHAN =
        """🍀 <b><a href="https://www.4chan.org/">4CHAN</a> BOARDS</b> 🍀""";

    public const string BOARDS_2CHAN =
        """⚡️ <b>ДОСКИ <a href="https://2ch.org/">ДВАЧА</a></b> ⚡️""";

    public const string UNKNOWN_LINK_4CHAN =
        "Dude, wrong URL 👉😄";

    public const string UNKNOWN_LINK_2CHAN =
        "Это куда ссылка? 👉😄";

    public const string MAY_TAKE_A_WHILE =
        """

        (может занять до пары минут 😵)
        """;

    // ALIASES

    public const string ALIAS_INFO =
        "📖 Популярные фильтры: ";

    public const string ALIAS_SYNTAX =
        """
        👁 Список ярлыков: /a{0}_info
        ✍️ Создать ярлык (синтаксис):
        <blockquote><code>/a{0} [имя] [аргументы {1}]</code></blockquote>
        """;

    public const string ALIAS_EXIST_RESPONSE =
        """
        Код "<code>{0}</code>" уже используется для:
        <blockquote>{1}</blockquote>
        {2} Придумайте другой …✍️
        """;

    public const string ALIAS_SAVED_RESPONSE =
        """
        Ярлык "<code>{0}</code>" успешно сохранён 🥂
        """;

    public const string ALIAS_DELETED_RESPONSE =
        """
        Ярлык "<code>{0}</code>" успешно удалён ♻️
        """;

    public const string ALIAS_NOT_FOUND =
        """
        Не могу найти ярлык "<code>{0}</code>" {1}
        """;

    public const string ALIAS_FORMAT_FAIL =
        """
        Не удалось подставить аргументы {0}

        Ваши аргументы: {1}
        Ярлык: <code>{2}</code> ({3} аргумент{4}):
        <blockquote>{5}</blockquote>
        """;

    // MEDIA

    public const string SOUND_UPLOADED =
        """
        🎙 Файл сохранён как:

        <code>{0}</code>
        """;

    public const string GIF_UPLOADED =
        """
        📹 Файл сохранён как:

        <code>{0}</code>
        """;

    // PIECE

    public const string PIECE_MANGA_NOT_FOUND =
        """
        {0} Не удалось найти тайтл "{1}"

        Пропишите <code>/piece info</code> чтобы посмотреть список тайтлов.
        """;

    public const string PIECE_CHAPTER_NOT_FOUND =
        """
        {0} Не удалось найти главу "{1}"

        Пропишите <code>/piece {2}</code> чтобы посмотреть список глав.
        """;
}