﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Witlesss.XD {
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
    internal class Texts {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Texts() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Witlesss.XD.Texts", typeof(Texts).Assembly);
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
        ///   Looks up a localized string similar to Этой командой можно скармливать мне треды с форча.
        ///
        ///&lt;b&gt;Синтаксис:&lt;/b&gt;
        ///&lt;code&gt;/board [ссылка на тред/доску/архив]&lt;/code&gt;
        ///&lt;code&gt;/board [имя сохранёнки]&lt;/code&gt;
        ///
        ///Список досок - &lt;code&gt;/boards&lt;/code&gt;
        ///Список сохранёнок - &lt;code&gt;/boards info&lt;/code&gt;
        ///Ссылка на архив = ссылка на доску + &lt;code&gt;archive&lt;/code&gt;
        ///
        ///💀На абордаж! 🏴‍☠️.
        /// </summary>
        internal static string BOARD_MANUAL {
            get {
                return ResourceManager.GetString("BOARD_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Тредов найдено: &lt;b&gt;{0}&lt;/b&gt;
        ///
        ///{1}.
        /// </summary>
        internal static string BOARD_START {
            get {
                return ResourceManager.GetString("BOARD_START", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This is the certified {0} classic!
        ///=======
        ///{1} на связи!.
        /// </summary>
        internal static string BUENOS_DIAS {
            get {
                return ResourceManager.GetString("BUENOS_DIAS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Console Commands:
        ///
        ///s   - save and exit
        ///+55 - select active chat
        ///
        /// /s  - save dics
        ///
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
        ///   Looks up a localized string similar to &lt;b&gt;&lt;u&gt;Обрезка видео&lt;/u&gt;&lt;/b&gt; ✋🤚
        ///
        ///&lt;b&gt;Синтаксис:&lt;/b&gt; &lt;code&gt;/crop ширина высота X Y&lt;/code&gt;
        ///
        ///&lt;b&gt;Дефолтные значения:&lt;/b&gt; &lt;code&gt;w&lt;/code&gt;, &lt;code&gt;h&lt;/code&gt;, &lt;code&gt;x&lt;/code&gt;, &lt;code&gt;y&lt;/code&gt;
        ///
        ///&lt;b&gt;Аргументы:&lt;/b&gt;
        ///1. Разделяются пробелами
        ///2. Передаются строго по порядку
        ///3. Могут быть как числами, обозначающими кол-во пикселей, так и сложными тригонометрическими формулами по времени.
        ///
        ///
        ///&lt;b&gt;Валидные примеры:&lt;/b&gt;
        ///
        ///&lt;code&gt;/crop min(w,h) min(w,h)&lt;/code&gt; 👈 обрезать до квадрата
        ///&lt;code&gt;/crop w-20 h-20&lt;/code&gt; 👈 срезат [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CROP_MANUAL {
            get {
                return ResourceManager.GetString("CROP_MANUAL", resourceCulture);
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
        ///• видео 🎬, гифками 📺
        ///• видео-стикерами 🎥
        ///• музыкой 🎧, wav-файлами🥭
        ///• кружками 📹, голосовыми🎙
        ///
        ///Ответь командой на файл 🗞 или прикрепи его к команде ✍️.
        /// </summary>
        internal static string DAMN_MANUAL {
            get {
                return ResourceManager.GetString("DAMN_MANUAL", resourceCulture);
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
        ///   Looks up a localized string similar to &lt;b&gt;Опции команды&lt;/b&gt; &lt;code&gt;/dg&lt;/code&gt;:
        ///
        ///&lt;code&gt;ll&lt;/code&gt; - в одну строку, без нижнего текста
        ///&lt;code&gt;nn&lt;/code&gt; - без вотермарок
        ///&lt;code&gt;__&amp;&lt;/code&gt; - шрифт верхнего текста
        ///&lt;code&gt;__*&lt;/code&gt; - шрифт нижнего текста
        ///
        ///&lt;b&gt;Шрифты&lt;/b&gt;: /fonts.
        /// </summary>
        internal static string DG_OPTIONS {
            get {
                return ResourceManager.GetString("DG_OPTIONS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;Опции команды&lt;/b&gt; &lt;code&gt;/dp&lt;/code&gt;:
        ///
        ///&lt;code&gt;#color#&lt;/code&gt; - цвет текста и рамки
        ///&lt;code&gt;xx&lt;/code&gt; - без верхушки и боковушек
        ///
        ///&lt;b&gt;Шрифты&lt;/b&gt;: /fonts.
        /// </summary>
        internal static string DP_OPTIONS {
            get {
                return ResourceManager.GetString("DP_OPTIONS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;&lt;u&gt;Эквалайзер&lt;/u&gt;&lt;/b&gt;
        ///
        ///&lt;b&gt;Синтаксис:&lt;/b&gt; &lt;code&gt;/eq [частота,Hz] [сила,dB] [ширина,Hz]&lt;/code&gt;
        ///
        ///&lt;b&gt;Дефолтные значения:&lt;/b&gt; &lt;code&gt;~&lt;/code&gt;, &lt;code&gt;10&lt;/code&gt;, &lt;code&gt;2000&lt;/code&gt;
        ///
        ///&lt;b&gt;Примеры:&lt;/b&gt;
        ///
        ///&lt;code&gt;/eq 100&lt;/code&gt; 👈 поднять басы на 10dB
        ///&lt;code&gt;/eq 1000&lt;/code&gt; 👈 поднять средние частоты на 10dB
        ///&lt;code&gt;/eq 50 40 1000&lt;/code&gt; 👈 поднять частоту 50Hz на 40dB
        ///&lt;code&gt;/eq 1000 100 10&lt;/code&gt; 👈 запикать
        ///&lt;code&gt;/eq 10000 -50 8000&lt;/code&gt; 👈 заглушить высокие частоты
        ///
        ///&lt;b&gt;Более подробно &lt;a href=&apos;https://ffmpe [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string EQ_MANUAL {
            get {
                return ResourceManager.GetString("EQ_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ошибка произошла во время выполнения следующей команды:
        ///
        ///{0}
        ///
        ///Если хотите чтоб её пофиксили - скиньте этот файл админу вместе с обрабатываемым файлом (づ｡◕‿‿◕｡)づ
        ///
        ///{1}
        ///
        ///Более детальный отчёт (для шарящих юзеров):
        ///
        ///{2}.
        /// </summary>
        internal static string FF_ERROR_REPORT {
            get {
                return ResourceManager.GetString("FF_ERROR_REPORT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 📝 &lt;u&gt;&lt;b&gt;Шрифты:&lt;/b&gt;&lt;/u&gt;
        ///
        ///&lt;u&gt;&lt;b&gt;Обычные:&lt;/b&gt;&lt;/u&gt;
        ///&lt;code&gt;im&lt;/code&gt; - &lt;b&gt;&lt;i&gt;Impact&lt;/i&gt;&lt;/b&gt;
        ///&lt;code&gt;rg&lt;/code&gt; - &lt;b&gt;&lt;i&gt;Roboto&lt;/i&gt;&lt;/b&gt; ✨
        ///&lt;code&gt;sg&lt;/code&gt; - &lt;b&gt;&lt;i&gt;Segoe UI&lt;/i&gt;&lt;/b&gt; ✨
        ///&lt;code&gt;ro&lt;/code&gt; - &lt;b&gt;&lt;i&gt;Times New Roman&lt;/i&gt;&lt;/b&gt; ✨
        ///&lt;code&gt;co&lt;/code&gt; - &lt;b&gt;&lt;i&gt;Comic Sans MS&lt;/i&gt;&lt;/b&gt; ✨
        ///&lt;code&gt;bb&lt;/code&gt; - &lt;b&gt;&lt;i&gt;Bender&lt;/i&gt;&lt;/b&gt; ✨
        ///&lt;code&gt;ft&lt;/code&gt; - &lt;b&gt;&lt;i&gt;Futura XBlkCn BT&lt;/i&gt;&lt;/b&gt;
        ///
        ///&lt;u&gt;&lt;b&gt;Тематические:&lt;/b&gt;&lt;/u&gt;
        ///&lt;code&gt;ru&lt;/code&gt; - &lt;b&gt;&lt;i&gt;CyrillicOld&lt;/i&gt;&lt;/b&gt; - летописный
        ///&lt;code&gt;go&lt;/code&gt; - &lt;b&gt;&lt;i&gt;CyrillicGoth&lt;/i&gt;&lt;/b&gt; - готиче [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string FONTS_CHEAT_SHEET {
            get {
                return ResourceManager.GetString("FONTS_CHEAT_SHEET", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to К сожалению, я не нашёл сохранённых обсуждений с таким названием
        ///
        ///&lt;code&gt;/boards@piece_fap_club info&lt;/code&gt;
        ///
        ///и выберите один из вариантов, или пропишите
        ///
        ///&lt;code&gt;/boards@piece_fap_club&lt;/code&gt;
        ///
        ///чтобы найти новый материал.
        /// </summary>
        internal static string FUSE_FAIL_BOARD {
            get {
                return ResourceManager.GetString("FUSE_FAIL_BOARD", resourceCulture);
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
        ///   Looks up a localized string similar to ёкарный бабай)0.
        /// </summary>
        internal static string FUSE_FAIL_SELF {
            get {
                return ResourceManager.GetString("FUSE_FAIL_SELF", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///
        ///&lt;code&gt;/fuse@piece_fap_club his *&lt;/code&gt; - скормить всё сразу.
        /// </summary>
        internal static string FUSE_HIS_ALL {
            get {
                return ResourceManager.GetString("FUSE_HIS_ALL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;&lt;i&gt;Kоманда /fuse позволяет быстро пополнить ваш словарь уже готовым материалом, а именно:&lt;/i&gt;&lt;/b&gt;
        ///
        ///1. &lt;u&gt;Словарём другой беседы&lt;/u&gt;. Пропишите &lt;b&gt;в другой беседе&lt;/b&gt; &lt;code&gt;/chat_id@piece_fap_bot&lt;/code&gt;, скопируйте &lt;b&gt;полученное число&lt;/b&gt; и пропишите &lt;b&gt;здесь&lt;/b&gt; &lt;code&gt;/fuse [полученное число]&lt;/code&gt;
        ///
        ///Пример: &lt;code&gt;/fuse -1001541923355&lt;/code&gt;
        ///
        ///2. &lt;u&gt;Словарями&lt;/u&gt;, созданными командой /move. Список таковых можно получить прописав &lt;code&gt;/fuse info&lt;/code&gt;.
        ///
        ///Выбрав нужный, пропишите &lt;code&gt;/fuse [имя]&lt; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string FUSE_MANUAL {
            get {
                return ResourceManager.GetString("FUSE_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cловарь беседы &lt;b&gt;{0}&lt;/b&gt; обновлён!
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
        ///• видео 🎬, гифками 📺
        ///• видео-сообщениями 📹
        ///• видео-стикерами 🎥.
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
        ///   Looks up a localized string similar to Я храню ссылки на последние &lt;b&gt;&lt;i&gt;{0} постов с реддита&lt;/i&gt;&lt;/b&gt; которые я отослал с момента включения.
        ///
        ///Если вы за этим, пропишите /link в ответ на сообщение с постом.
        /// </summary>
        internal static string LINK_MANUAL {
            get {
                return ResourceManager.GetString("LINK_MANUAL", resourceCulture);
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
        ///   Looks up a localized string similar to (может занять до пары минут).
        /// </summary>
        internal static string MAY_TAKE_A_WHILE {
            get {
                return ResourceManager.GetString("MAY_TAKE_A_WHILE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Я могу делать &lt;u&gt;&lt;b&gt;{0}&lt;/b&gt;&lt;/u&gt; из картинок 📸 и видео 🎬
        ///
        ///Прикрепи файл к команде, либо ответь командой на файл.
        ///
        ///📖 Справка: /man_3
        ///ℹ️ Список опций: /op_{1}.
        /// </summary>
        internal static string MEME_MANUAL {
            get {
                return ResourceManager.GetString("MEME_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;Опции команды&lt;/b&gt; &lt;code&gt;/meme&lt;/code&gt;:
        ///
        ///&lt;code&gt;t&lt;/code&gt; - только верхний текст
        ///&lt;code&gt;d&lt;/code&gt; - только нижний текст
        ///&lt;code&gt;s&lt;/code&gt; - добавлять нижний текст (к своему)
        ///&lt;code&gt;100%&lt;/code&gt; - непрозрачность тени (0-100)
        ///&lt;code&gt;10&quot;&lt;/code&gt; - стартовый размер шрифта (1-999)
        ///&lt;code&gt;#color#&lt;/code&gt; - цвет фона (стикеры)
        ///&lt;code&gt;cc&lt;/code&gt; - текст случайных цветов 🎨
        ///
        ///&lt;b&gt;Шрифты&lt;/b&gt;: /fonts.
        /// </summary>
        internal static string MEME_OPTIONS {
            get {
                return ResourceManager.GetString("MEME_OPTIONS", resourceCulture);
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
        ///   Looks up a localized string similar to ♻️ Словарь очищен! *пусто* {0}
        ///
        ///Содержимое {1} как &lt;b&gt;&quot;{2}&quot;&lt;/b&gt;.
        /// </summary>
        internal static string MOVING_DONE {
            get {
                return ResourceManager.GetString("MOVING_DONE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;Опции команды&lt;/b&gt; &lt;code&gt;/nuke&lt;/code&gt;:
        ///
        ///&lt;code&gt;N&quot;&lt;/code&gt; - кол-во проходов (1-9 📸, 1-3 🎬).
        /// </summary>
        internal static string NUKE_OPTIONS {
            get {
                return ResourceManager.GetString("NUKE_OPTIONS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Годятся только &lt;b&gt;JSON&lt;/b&gt;-файлы, в виде &lt;b&gt;списка строк&lt;/b&gt;, например:.
        /// </summary>
        internal static string ONLY_ARRAY_JSON {
            get {
                return ResourceManager.GetString("ONLY_ARRAY_JSON", resourceCulture);
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
        ///   Looks up a localized string similar to Словарь &lt;b&gt;&quot;{0}&quot;&lt;/b&gt; опубликован!
        ///
        ///Проверить: &lt;code&gt;/fuse info&lt;/code&gt;.
        /// </summary>
        internal static string PUB_EX_DONE {
            get {
                return ResourceManager.GetString("PUB_EX_DONE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} не могу найти словаря с таким названием((
        ///
        ///Пропишите &lt;code&gt;/fuse ! info&lt;/code&gt;, чтобы посмотреть весь список..
        /// </summary>
        internal static string PUB_EX_NOT_FOUND {
            get {
                return ResourceManager.GetString("PUB_EX_NOT_FOUND", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;Пожирание коментов с Reddit:&lt;/b&gt;
        ///
        ///&lt;code&gt;/xd [search query] [subreddit*] [-options]&lt;/code&gt;
        ///
        ///&lt;b&gt;Примеры:&lt;/b&gt;
        ///
        ///☝️ &lt;code&gt;/xd osaka okbuddybaka*&lt;/code&gt;
        ///👉 &lt;code&gt;/xd okbuddybaka*&lt;/code&gt;
        ///👊 &lt;code&gt;/xd ohio rizz okbuddyretard*&lt;/code&gt;
        ///🤙 &lt;code&gt;/xd real trap shit&lt;/code&gt;
        ///👌 &lt;code&gt;/xd amogus -cm&lt;/code&gt;.
        /// </summary>
        internal static string REDDIT_COMMENTS_MANUAL {
            get {
                return ResourceManager.GetString("REDDIT_COMMENTS_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to НАЧИНАЕМ ПРИЗЫВ СОТОНЫ!!!
        ///
        ///{0}.
        /// </summary>
        internal static string REDDIT_COMMENTS_START {
            get {
                return ResourceManager.GetString("REDDIT_COMMENTS_START", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Если ты это написал, то очевидно, ты ничего не понял 😣 и тебе нужна инструкция 📃, так ведь? 😎
        ///
        ///&lt;b&gt;&lt;i&gt;&lt;u&gt;Смотри кароче&lt;/u&gt;&lt;/i&gt;&lt;/b&gt;:
        ///
        ///&lt;code&gt;/w&lt;/code&gt; - самая простая команда, вводишь после неё любой текст ✍️ и я ищю подходящие посты по всему реддиту 🔍
        ///&lt;code&gt;/ws&lt;/code&gt; - более сложная, после неё уже надо вводить название сабреддита 🎟, и я кидаю посты оттуда 🛒
        ///
        ///Думаю, важно напомнить☝️что большая часть контента на реддите - на английском 🤓, а еще нам много нсфв 🥵
        ///
        ///Это так, простенький гайд, бол [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string REDDIT_MANUAL {
            get {
                return ResourceManager.GetString("REDDIT_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Тут всё просто 😎👌
        ///
        ///Пишешь после команды любой текст (желательно англ.) и я выдаю список сабреддитов, которые подходят под запрос.
        /// </summary>
        internal static string REDDIT_SUBS_MANUAL {
            get {
                return ResourceManager.GetString("REDDIT_SUBS_MANUAL", resourceCulture);
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
        ///   Looks up a localized string similar to &lt;b&gt;&lt;u&gt;Масштабирование&lt;/u&gt;&lt;/b&gt;
        ///
        ///&lt;b&gt;Синтаксис:&lt;/b&gt; &lt;code&gt;/scale ширина высота флаги&lt;/code&gt;
        ///
        ///&lt;b&gt;Примеры:&lt;/b&gt;
        ///
        ///&lt;code&gt;/scale 100&lt;/code&gt; 👈 ширина - 100px
        ///&lt;code&gt;/scale -1 100&lt;/code&gt; 👈 высота - 100px
        ///&lt;code&gt;/scale 1280 720&lt;/code&gt; 👈 big black...
        ///
        ///↗️ Увеличить в 2️⃣ раза:
        ///&lt;code&gt;/scale 2&lt;/code&gt;
        ///&lt;code&gt;/scale w*2&lt;/code&gt;
        ///
        ///↘️ Уменьшить в 2️⃣ раза:
        ///&lt;code&gt;/scale 0.5&lt;/code&gt;
        ///&lt;code&gt;/scale w/2&lt;/code&gt;
        ///
        ///По дефолту соотношение сторон сохраняется
        ///
        ///&lt;b&gt;Использование флагов 🏁:&lt;/b&gt;
        ///
        ///&lt;code&gt;/scale 0.5 -1 bilinear [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SCALE_MANUAL {
            get {
                return ResourceManager.GetString("SCALE_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;&lt;u&gt;Тип авто-мемов&lt;/u&gt;&lt;/b&gt;:
        ///
        ///&lt;code&gt;/set [буква]&lt;/code&gt;
        ///
        ///&lt;code&gt;M &lt;/code&gt; - мемы☝️
        ///&lt;code&gt;T &lt;/code&gt; - подписанки 💬
        ///&lt;code&gt;D &lt;/code&gt; - демотиваторы👌
        ///&lt;code&gt;Dg&lt;/code&gt; - демотиваторы💀
        ///&lt;code&gt;N &lt;/code&gt; - ядерный фритюр 🍤
        ///
        ///&lt;b&gt;&lt;u&gt;Опции в мемчиках по умолчанию&lt;/u&gt;&lt;/b&gt;:
        ///
        ///&lt;code&gt;/set [буква] [опции]&lt;/code&gt;
        ///
        ///&lt;code&gt;/set M wwsd#lime#…&lt;/code&gt;
        ///&lt;code&gt;/set T largmmww15&quot;…&lt;/code&gt;
        ///&lt;code&gt;/set D cpupbb-i…&lt;/code&gt;
        ///&lt;code&gt;/set Dg x=5n…&lt;/code&gt;
        ///&lt;code&gt;/set N =3…&lt;/code&gt;
        ///
        ///&lt;code&gt;/set [буква] 0&lt;/code&gt; - сбросить.
        /// </summary>
        internal static string SET_FREQUENCY_MANUAL {
            get {
                return ResourceManager.GetString("SET_FREQUENCY_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to я буду отвечать на {0}% сообщений.
        /// </summary>
        internal static string SET_FREQUENCY_RESPONSE {
            get {
                return ResourceManager.GetString("SET_FREQUENCY_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Опции команды &lt;b&gt;{0}&lt;/b&gt; изменены на &lt;b&gt;{1}&lt;/b&gt;.
        /// </summary>
        internal static string SET_MEME_OPS_RESPONSE {
            get {
                return ResourceManager.GetString("SET_MEME_OPS_RESPONSE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Выбрать тип авто-мемов:
        ///
        ///&lt;code&gt;/meme&lt;/code&gt; ☝️👇 &lt;code&gt;/set M &lt;/code&gt;
        ///&lt;code&gt;/top &lt;/code&gt; 💬🔝 &lt;code&gt;/set T &lt;/code&gt;
        ///&lt;code&gt;/dp  &lt;/code&gt; ◼️▪️ &lt;code&gt;/set D &lt;/code&gt;
        ///&lt;code&gt;/dg  &lt;/code&gt; ◾️🔐 &lt;code&gt;/set Dg&lt;/code&gt;
        ///&lt;code&gt;/nuke&lt;/code&gt; 🍕🍤 &lt;code&gt;/set N &lt;/code&gt;.
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
        ///   Looks up a localized string similar to {0}% пикч будут ставать мемами.
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
        ///   Looks up a localized string similar to &lt;u&gt;Команда работает с&lt;/u&gt;:
        ///
        ///• видео 🎬, гифками 📺
        ///• видео-стикерами 🎥
        ///• музыкой 🎧, wav-файлами🥭
        ///• кружками 📹, голосовыми🎙
        ///• ссылками на видео📎
        ///
        ///Ответь командой на файл 🗞 или прикрепи его к команде ✍️.
        /// </summary>
        internal static string SLICE_MANUAL {
            get {
                return ResourceManager.GetString("SLICE_MANUAL", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;&lt;u&gt;Как скачивать музыку с ютуба:&lt;/u&gt;&lt;/b&gt;
        ///
        ///Пишешь &lt;code&gt;/song *ссылка*&lt;/code&gt;, и ждешь...
        ///
        ///Или пишешь &lt;code&gt;/song&lt;/code&gt; в ответ на сообщение со ссылкой, которую ты, к примеру, нашёл через @vid.
        ///
        ///&lt;b&gt;Кроме того:&lt;/b&gt;
        ///
        ///Если хочешь, можешь в конце указать автора и название, или просто название, например:
        ///
        ///&lt;blockquote&gt;/song https://youtu.be/fB6elql_EdM Eminem - My Name Is&lt;/blockquote&gt;
        ///&lt;blockquote&gt;/song https://youtu.be/fB6elql_EdM Eminem&apos;s Name Is&lt;/blockquote&gt;
        ///
        ///Также можешь найти 🔍 в интернете кр [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SONG_MANUAL {
            get {
                return ResourceManager.GetString("SONG_MANUAL", resourceCulture);
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
        ///   Looks up a localized string similar to &lt;b&gt;Опции команды&lt;/b&gt; &lt;code&gt;/top&lt;/code&gt;:
        ///
        ///&lt;code&gt;mm&lt;/code&gt; - тонкая плашка
        ///&lt;code&gt;mm!&lt;/code&gt; - супер тонкая плашка
        ///&lt;code&gt;la&lt;/code&gt; - текст по левому краю
        ///&lt;code&gt;pp&lt;/code&gt; - авто-выбор цвета (края)
        ///&lt;code&gt;pp!&lt;/code&gt; - авто-выбор цвета (центр)
        ///&lt;code&gt;ob&lt;/code&gt; - чёрный задник (для стикеров)
        ///&lt;code&gt;20%&lt;/code&gt; - обрезать 20% сверху (0-100)
        ///&lt;code&gt;-20%&lt;/code&gt; - обрезать по 10% сверху и снизу
        ///&lt;code&gt;10&quot;&lt;/code&gt; - стартовый размер шрифта (1-999)
        ///&lt;code&gt;min5&quot;&lt;/code&gt; - мин. размер шрифта (1-999)
        ///&lt;code&gt;blur&lt;/code&gt; - [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string TOP_OPTIONS {
            get {
                return ResourceManager.GetString("TOP_OPTIONS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 🎮 &lt;b&gt;SUPER GAMING BATTLE&lt;/b&gt; 🎳
        ///
        ///🫵 Одолей меня чтобы удалить словарь
        ///
        ///🏠 - я, 🚜 - ты.
        /// </summary>
        internal static string TRACTOR_GAME_RULES {
            get {
                return ResourceManager.GetString("TRACTOR_GAME_RULES", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///
        ///Используйте стрелочки для навигации ☝️🤓.
        /// </summary>
        internal static string USE_ARROWS {
            get {
                return ResourceManager.GetString("USE_ARROWS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;&lt;u&gt;Громкость&lt;/u&gt;&lt;/b&gt;
        ///
        ///&lt;b&gt;Синтаксис:&lt;/b&gt; &lt;code&gt;/vol число/формула&lt;/code&gt;
        ///
        ///&lt;b&gt;Примеры:&lt;/b&gt;
        ///
        ///&lt;code&gt;/vol 2&lt;/code&gt; 👈 сделать в 2 раза громче
        ///&lt;code&gt;/vol 10&lt;/code&gt; 👈 сделать в 10 раз громче
        ///&lt;code&gt;/vol 0.5&lt;/code&gt; 👈 сделать в 2 раза тише
        ///&lt;code&gt;/vol 0.5+t*0.1&lt;/code&gt; 👈 рост громкости с ×0,5 до ×2 за 15 секунд
        ///&lt;code&gt;/vol if(gt(t,1),1,t)&lt;/code&gt; 👈 плавный 1-секундный вход
        ///&lt;code&gt;/vol if(gt(t,0.5),1,2*t)&lt;/code&gt; 👈 плавный полсекундный вход
        ///
        ///&lt;b&gt;Более подробно &lt;a href=&apos;https://ffmpeg.org//ffmpeg-filters. [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string VOLUME_MANUAL {
            get {
                return ResourceManager.GetString("VOLUME_MANUAL", resourceCulture);
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
