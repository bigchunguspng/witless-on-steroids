﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Witlesss.X {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Witlesss.X.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Отправь эту команду вместе со словом, и я сгенерю чё-нибудь, начиная с него (если оно есть в словаре).
        /// </summary>
        internal static string A_MANUAL {
            get {
                return ResourceManager.GetString("A_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Параметры генерации смогут менять {0}.
        /// </summary>
        internal static string ADMINS_RESPONSE {
            get {
                return ResourceManager.GetString("ADMINS_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-Backup.
        /// </summary>
        internal static string BACKUP_FOLDER {
            get {
                return ResourceManager.GetString("BACKUP_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to @piece_fap_bot.
        /// </summary>
        internal static string BOT_USERNAME {
            get {
                return ResourceManager.GetString("BOT_USERNAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Chat-History.
        /// </summary>
        internal static string CH_HISTORY_FILE_PREFIX {
            get {
                return ResourceManager.GetString("CH_HISTORY_FILE_PREFIX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-HistoryDBs.
        /// </summary>
        internal static string CH_HISTORY_FOLDER {
            get {
                return ResourceManager.GetString("CH_HISTORY_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;{0}&lt;/b&gt;
        ///
        ///Вес словаря: {1}
        ///Интервал генерации: {2}
        ///Мемами стают: {3}% пикч
        ///Качество картинок: {4}%
        ///Мемы из стикеров: {5}
        ///Текст в мемах: {6}
        ///Пикчи стают: {7}
        ///Менять настройки могут: {8}.
        /// </summary>
        internal static string CHAT_INFO {
            get {
                return ResourceManager.GetString("CHAT_INFO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-ChatsDB.
        /// </summary>
        internal static string CHATLIST_FILENAME {
            get {
                return ResourceManager.GetString("CHATLIST_FILENAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to текст в мемах будет {0}.
        /// </summary>
        internal static string COLORS_RESPONSE {
            get {
                return ResourceManager.GetString("COLORS_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Console Commands:
        ///
        ///s   - save and exit
        ///+55 - select active chat
        ///
        /// /s  - save dics
        /// /sd - sync dics
        /// /sp - spam (min size)
        ///
        /// /db - delete blockers
        /// /DB - delete active if blocked
        /// /ds - delete by size (max size)
        ///
        /// /cc - clear temp files
        /// /oo - clear dics
        /// /Oo - clear active dic
        ///
        /// /xx - fix DBs
        /// /Xx - fix active chat DB
        ///
        /// /l  - activate last chat
        /// /b  - ban active chat (hours)
        /// /ub - unban active chat.
        /// </summary>
        internal static string CONSOLE_MANUAL {
            get {
                return ResourceManager.GetString("CONSOLE_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-CopyDBs.
        /// </summary>
        internal static string COPIES_FOLDER {
            get {
                return ResourceManager.GetString("COPIES_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Укажите &lt;b&gt;таймкод начала&lt;/b&gt; фрагмента и его &lt;b&gt;длину&lt;/b&gt;, например:
        ///
        ///&lt;b&gt;/cut@piece_fap_bot 0:10&lt;/b&gt;
        ///
        ///выдаст первые 10 секунд
        ///
        ///&lt;b&gt;/cut@piece_fap_bot 0:10 хд&lt;/b&gt;
        ///
        ///оставит всё, что идёт после 10-й секунды
        ///
        ///&lt;b&gt;/cut@piece_fap_bot 0:10 0:15&lt;/b&gt;
        ///
        ///вырежет 15-секундный фрагмент, начиная с 10-й секунды
        ///
        ///&lt;b&gt;/cut@piece_fap_bot 0:10 - 0:15&lt;/b&gt;
        ///
        ///вырежет 5-секундный фрагмент, с 10-й секунды по 15-ю.
        /// </summary>
        internal static string CUT_MANUAL {
            get {
                return ResourceManager.GetString("CUT_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;u&gt;Команда работает с&lt;/u&gt;:
        ///
        ///• видео 🎬, гифками🪣
        ///• видеосообщениями 📹, видео-стикерами🍿
        ///• музыкой 🎧, wav-файлами🥭, голосовыми🎙
        ///
        ///Отправь её вместе с таким файлом 🗞 или в ответ на него✍️.
        /// </summary>
        internal static string DAMN_MANUAL {
            get {
                return ResourceManager.GetString("DAMN_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-WitlessDB.
        /// </summary>
        internal static string DB_FILE_PREFIX {
            get {
                return ResourceManager.GetString("DB_FILE_PREFIX", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-WitlessDBs.
        /// </summary>
        internal static string DBS_FOLDER {
            get {
                return ResourceManager.GetString("DBS_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Поздравляю, чат &lt;b&gt;{0}&lt;/b&gt; был удалён из списка чатов, а словарь сохранён как &lt;b&gt;{1}&lt;/b&gt;!
        ///
        ///Если хотите начать заново - пропишите /start@piece_fap_bot.
        /// </summary>
        internal static string DEL_SUCCESS_RESPONSE {
            get {
                return ResourceManager.GetString("DEL_SUCCESS_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Comic Sans MS.
        /// </summary>
        internal static string DEMOTIVATOR_LOWER_FONT {
            get {
                return ResourceManager.GetString("DEMOTIVATOR_LOWER_FONT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Times New Roman.
        /// </summary>
        internal static string DEMOTIVATOR_UPPER_FONT {
            get {
                return ResourceManager.GetString("DEMOTIVATOR_UPPER_FONT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Emoji.
        /// </summary>
        internal static string EMOJI_FOLDER {
            get {
                return ResourceManager.GetString("EMOJI_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-ExtraDBs.
        /// </summary>
        internal static string EXTRA_DBS_FOLDER {
            get {
                return ResourceManager.GetString("EXTRA_DBS_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\ffmpeg\bin\ffmpeg.exe.
        /// </summary>
        internal static string FFMPEG_PATH {
            get {
                return ResourceManager.GetString("FFMPEG_PATH", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to К сожалению, я не нашёл словаря с таким названием.
        /// </summary>
        internal static string FUSE_FAIL_BASE {
            get {
                return ResourceManager.GetString("FUSE_FAIL_BASE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to К сожалению, у меня нет словаря этой беседы.
        /// </summary>
        internal static string FUSE_FAIL_CHAT {
            get {
                return ResourceManager.GetString("FUSE_FAIL_CHAT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to К сожалению, я не нашёл переписки за этот диапазон дат, пропишите
        ///
        ///&lt;code&gt;/fuse@piece_fap_club his&lt;/code&gt;
        ///
        ///и выберите один из вариантов, или пропишите
        ///
        ///&lt;code&gt;/fuse@piece_fap_club his all&lt;/code&gt;
        ///
        ///чтобы скормить всё сразу.
        /// </summary>
        internal static string FUSE_FAIL_DATES {
            get {
                return ResourceManager.GetString("FUSE_FAIL_DATES", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ёкарный бабай)0.
        /// </summary>
        internal static string FUSE_FAIL_SELF {
            get {
                return ResourceManager.GetString("FUSE_FAIL_SELF", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;&lt;i&gt;Kоманда /fuse позволяет быстро пополнять ваш словарь уже готовым материалом, а именно:&lt;/i&gt;&lt;/b&gt;
        ///
        ///1. &lt;u&gt;Словарь другой беседы&lt;/u&gt;. Пропишите &lt;b&gt;в другой беседе&lt;/b&gt;
        ///
        /// /chat_id@piece_fap_bot
        ///
        ///скопируйте &lt;b&gt;полученное число&lt;/b&gt; и пропишите &lt;b&gt;здесь&lt;/b&gt;
        ///
        /// /fuse@piece_fap_bot [полученное число]
        ///
        ///Пример: &lt;i&gt;/fuse -1001541923355&lt;/i&gt;
        ///
        ///Слияние разово обновит словарь &lt;b&gt;этой беседы&lt;/b&gt;
        ///
        ///2. &lt;u&gt;Словари созданные командой&lt;/u&gt; /move. Список таковых можно посмотреть прописав &lt;code&gt;/fuse@piece_fap_bot info&lt; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string FUSE_MANUAL {
            get {
                return ResourceManager.GetString("FUSE_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cловарь беседы &quot;{0}&quot; обновлён!
        ///Теперь он весит {1} (+{2}).
        /// </summary>
        internal static string FUSE_SUCCESS_RESPONSE {
            get {
                return ResourceManager.GetString("FUSE_SUCCESS_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;u&gt;Команда работает с&lt;/u&gt;:
        ///
        ///• видео 🎬, гифками🪣, видео-стикерами🍿.
        /// </summary>
        internal static string G_MANUAL {
            get {
                return ResourceManager.GetString("G_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Эта команда только для бесед 🤓.
        /// </summary>
        internal static string GROUPS_ONLY_COMAND {
            get {
                return ResourceManager.GetString("GROUPS_ONLY_COMAND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CHATLIST SAVED.
        /// </summary>
        internal static string LOG_CHATLIST_SAVED {
            get {
                return ResourceManager.GetString("LOG_CHATLIST_SAVED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to FUSION DONE.
        /// </summary>
        internal static string LOG_FUSION_DONE {
            get {
                return ResourceManager.GetString("LOG_FUSION_DONE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;u&gt;{0} я делаю из&lt;/u&gt;:
        ///
        ///• картинок 📸, стикеров 🎟
        ///• видео 🎬, гифок🪣
        ///• видеосообщений 📹 видео-стикеров🍿
        ///
        ///Напиши это вместе с таким файлом 🗞 или в ответ на него✍️.
        /// </summary>
        internal static string MEME_MANUAL {
            get {
                return ResourceManager.GetString("MEME_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Если вы хотите очистить словарь беседы, пропишите
        ///
        /// /move@piece_fap_bot [имя]
        ///
        ///Эта команда сохранит &lt;b&gt;копию словаря&lt;/b&gt; под указанным именем и очистит &lt;b&gt;сам словарь&lt;/b&gt;. Зная имя, вы можете в любой момент &lt;b&gt;влить&lt;/b&gt; сохранённую копию в словарь беседы, прописав
        ///
        /// /fuse@piece_fap_bot [имя].
        /// </summary>
        internal static string MOVE_MANUAL {
            get {
                return ResourceManager.GetString("MOVE_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Словарь очищен! *пусто*
        ///
        ///Копия словаря сохранена как &lt;b&gt;&quot;{0}&quot;&lt;/b&gt;.
        /// </summary>
        internal static string MOVING_DONE {
            get {
                return ResourceManager.GetString("MOVING_DONE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-Pictures.
        /// </summary>
        internal static string PICTURES_FOLDER {
            get {
                return ResourceManager.GetString("PICTURES_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Данная команда☝️поможет вам создать словарь 📔 влив который 😰 бот сможет накидывать вам 👾 рандомных постов ✍️ из определённого тг-канала
        ///
        ///&lt;b&gt;Синтаксис:&lt;/b&gt;
        ///
        /// /piece@piece_fap_bot [ссылка на последний пост] [название_без_пробелов]
        ///
        ///&lt;b&gt;Например:&lt;/b&gt;
        ///
        /// /piece@piece_fap_bot t.me/xd_bruh_asd/1337 BLACK_OSAKA.
        /// </summary>
        internal static string PIECE_MANUAL {
            get {
                return ResourceManager.GetString("PIECE_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;Готово!&lt;/b&gt; 🥂
        ///
        ///Чтобы влить словарь, пропишите:
        ///
        ///&lt;code&gt;/fuse@piece_fap_bot {0}&lt;/code&gt;.
        /// </summary>
        internal static string PIECE_RESPONSE {
            get {
                return ResourceManager.GetString("PIECE_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ((\u00a9|\u00ae|\u203c|\u2049|\u2122|[\u2139-\u21aa]|\u3297|\u3299)\ufe0f|([\u231a-\u303d]|(\ud83c|\ud83d|\ud83e)[\ud000-\udfff])\ufe0f*\u200d*|[\d*#]\ufe0f\u20e3)+.
        /// </summary>
        internal static string REGEX_EMOJI {
            get {
                return ResourceManager.GetString("REGEX_EMOJI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;&lt;i&gt;Инструкция в самолёте:&lt;/i&gt;&lt;/b&gt;
        ///
        ///&lt;u&gt;Изменить интервал отправки текста&lt;/u&gt;:
        ///
        /// /set@piece_fap_bot 3 (число сообщений)
        ///
        ///&lt;u&gt;Изменить тип авто-мемов&lt;/u&gt;:
        ///
        ///&lt;code&gt;/set@piece_fap_bot M&lt;/code&gt; - мемы
        ///&lt;code&gt;/set@piece_fap_bot D&lt;/code&gt; - демотиваторы
        ///
        ///&lt;u&gt;Изменить частоту появления мемов&lt;/u&gt;:
        ///
        /// /pics@piece_fap_bot [число] (%).
        /// </summary>
        internal static string SET_FREQUENCY_MANUAL {
            get {
                return ResourceManager.GetString("SET_FREQUENCY_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to я буду отвечать на.
        /// </summary>
        internal static string SET_FREQUENCY_RESPONSE {
            get {
                return ResourceManager.GetString("SET_FREQUENCY_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Выбрать тип авто-мемов:
        ///
        ///&lt;code&gt;/set@piece_fap_bot M&lt;/code&gt;
        ///&lt;code&gt;/set@piece_fap_bot D&lt;/code&gt;
        ///
        ///(&lt;u&gt;м&lt;/u&gt;акросы / &lt;u&gt;д&lt;/u&gt;емотиваторы).
        /// </summary>
        internal static string SET_MEMES_MANUAL {
            get {
                return ResourceManager.GetString("SET_MEMES_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Картинки будут превращаться в {0}.
        /// </summary>
        internal static string SET_MEMES_RESPONSE {
            get {
                return ResourceManager.GetString("SET_MEMES_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to пикчи будут ставать мемами с вероятностью {0}%.
        /// </summary>
        internal static string SET_P_RESPONSE {
            get {
                return ResourceManager.GetString("SET_P_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to качество картинок будет {0}%.
        /// </summary>
        internal static string SET_Q_RESPONSE {
            get {
                return ResourceManager.GetString("SET_Q_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Если че правильно вот так:
        ///
        /// /{0}@piece_fap_bot 75.
        /// </summary>
        internal static string SET_X_MANUAL {
            get {
                return ResourceManager.GetString("SET_X_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ВИРУСНАЯ БАЗА ОБНОВЛЕНА!.
        /// </summary>
        internal static string START_RESPONSE {
            get {
                return ResourceManager.GetString("START_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;u&gt;Команда работает с&lt;/u&gt;:
        ///
        ///• картинками 📸 и стикерами 🎟
        ///• картинками 📸 без сжатия.
        /// </summary>
        internal static string STICK_MANUAL {
            get {
                return ResourceManager.GetString("STICK_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to cтикеры {0}будут ставать мемами в случайном порядке.
        /// </summary>
        internal static string STICKERS_RESPONSE {
            get {
                return ResourceManager.GetString("STICKERS_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Вместе с командой можно указать &lt;b&gt;таймкод начала&lt;/b&gt; фрагмента и его &lt;b&gt;длину&lt;/b&gt;, например:
        ///
        ///&lt;b&gt;/sus@piece_fap_bot&lt;/b&gt;
        ///
        ///зареверсит первую половину
        ///
        ///&lt;b&gt;/sus@piece_fap_bot 0&lt;/b&gt;
        ///
        ///зареверсит видео целиком
        ///
        ///&lt;b&gt;/sus@piece_fap_bot 0:03&lt;/b&gt;
        ///
        ///зареверсит первые 3 секунды
        ///
        ///&lt;b&gt;/sus@piece_fap_bot 0:15 хд&lt;/b&gt;
        ///
        ///зареверсит всё, что идёт после 15-й секунды
        ///
        ///&lt;b&gt;/sus@piece_fap_bot 0:10 0:03&lt;/b&gt;
        ///
        ///зареверсит 3-секундный фрагмент, начиная с 10-й секунды.
        /// </summary>
        internal static string SUS_MANUAL {
            get {
                return ResourceManager.GetString("SUS_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-Temp.
        /// </summary>
        internal static string TEMP_FOLDER {
            get {
                return ResourceManager.GetString("TEMP_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Telegram-Water.
        /// </summary>
        internal static string WATERMARKS_FOLDER {
            get {
                return ResourceManager.GetString("WATERMARKS_FOLDER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Для использования этой команды нужно прописать
        ///
        /// /start@piece_fap_bot.
        /// </summary>
        internal static string WITLESS_ONLY_COMAND {
            get {
                return ResourceManager.GetString("WITLESS_ONLY_COMAND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Отправь эту команду вместе со словом, и я сгенерю чё-нибудь, заканчивая им (если оно есть в словаре).
        /// </summary>
        internal static string ZZ_MANUAL {
            get {
                return ResourceManager.GetString("ZZ_MANUAL", resourceCulture);
            }
        }
    }
}
