namespace Witlesss.XD;

public static class Texts
{
    public const string START_RESPONSE =
        """
        ВИРУСНАЯ БАЗА ОБНОВЛЕНА!

        Будет не понятно - пиши /help
        """;

    public const string SET_MANUAL =
        """
        <u><b>Тип авто-мемов</b></u>:

        <code>/set [код]</code>
        <blockquote><b>Примеры</b>:
        <code>/set M </code> - мемы☝️
        <code>/set T </code> - подписанки 💬
        <code>/set D </code> - демотиваторы👌
        <code>/set Dg</code> - демотиваторы💀
        <code>/set N </code> - ядерный фритюр 🍤</blockquote>

        <u><b>Авто-опции</b></u>:

        <code>/set [код] [опции]</code>
        <blockquote><b>Примеры</b>:
        <code>/set M wwsd#lime#</code>
        <code>/set T largmmww5"</code>
        <code>/set D imup</code>
        <code>/set Dg xllro-b</code>
        <code>/set N =3</code></blockquote>

        <code>/set [код] 0</code> - сбросить
        """;

    public const string FUSE_MANUAL =
        """
        <b>Слияние / кормёжка</b>

        <u>Команда работает с:</u>
        🔑 айдишниками чатов
        📦 именами словарей со склада
        🗞 TXT и JSON-файлами
        🗞 именами файлов со склада

        <u>Списки:</u>
        <code>/fuse   info</code> - публичный архив 📦
        <code>/fuse ! info</code> - приватный архив 📦
        <code>/fuse * info</code> - приватный архив 🗞

        📖 Гайды: /man_22, /man_221, /man_222
        """;

    public const string FUSE_FAIL_CHAT =
        "К сожалению, у меня нет словаря этой беседы";

    public const string LOG_FUSION_DONE =
        "FUSION DONE";

    public const string LOG_CHATLIST_SAVED =
        "CHATLIST SAVED";

    public const string SET_FREQUENCY_RESPONSE =
        "я буду отвечать на {0}% сообщений";

    public const string SET_P_RESPONSE =
        "{0}% пикч будут ставать мемами";

    public const string FUSE_SUCCESS_RESPONSE =
        """
        Cловарь беседы <b>{0}</b> обновлён!
        Теперь он весит {1} (+{2})
        """;

    public const string MOVING_DONE =
        """
        ♻️ Словарь очищен! *пусто* {0}

        Содержимое {1} как <b>"{2}"</b>
        """;

    public const string A_MANUAL =
        "Отправь эту команду вместе со словом, и я сгенерю чё-нибудь, начиная с него (если оно есть в словаре)";

    public const string ZZ_MANUAL =
        "Отправь эту команду вместе со словом, и я сгенерю чё-нибудь, заканчивая им (если оно есть в словаре)";

    public const string CUT_MANUAL =
        """
        Укажите <b>таймкод начала</b> фрагмента и его <b>длину</b>, например:

        <b>/cut 0:10</b>

        выдаст первые 10 секунд

        <b>/cut 0:10 хд</b>

        оставит всё, что идёт после 10-й секунды

        <b>/cut 0:10 0:15</b>

        вырежет 15-секундный фрагмент, начиная с 10-й секунды

        <b>/cut 0:10 - 0:15</b>

        вырежет 5-секундный фрагмент, с 10-й секунды по 15-ю
        """;

    public const string SUS_MANUAL =
        """
        Вместе с командой можно указать <b>таймкод начала</b> фрагмента и его <b>длину</b>, например:

        <b>/sus</b>

        зареверсит первую половину

        <b>/sus 0</b>

        зареверсит видео целиком

        <b>/sus 0:03</b>

        зареверсит первые 3 секунды

        <b>/sus 0:15 хд</b>

        зареверсит всё, что идёт после 15-й секунды

        <b>/sus 0:10 0:03</b>

        зареверсит 3-секундный фрагмент, начиная с 10-й секунды
        """;

    public const string STICKERS_RESPONSE =
        "cтикеры {0}будут ставать мемами в случайном порядке";

    public const string ADMINS_RESPONSE =
        "Параметры генерации смогут менять {0}";

    public const string GROUPS_ONLY_COMAND =
        "Эта команда только для бесед 🤓";

    public const string DEL_SUCCESS_RESPONSE =
        """
        Поздравляю, чат <b>{0}</b> был удалён из списка чатов, а словарь сохранён как <b>{1}</b>!

        Если хотите начать заново - пропишите /start{2}
        """;

    public const string CONSOLE_MANUAL =
        """
        Console Commands:

        s   - save and exit
        +55 - select active chat

        /s  - save dics

        /sp - spam (min size)

        /db - delete blockers
        /DB - delete active if blocked
        /ds - delete by size (max size)

        /cc - clear temp files
        /oo - clear dics
        /Oo - clear active dic

        /l  - activate last chat
        /b  - ban active chat (hours)
        /ub - unban active chat
        """;

    public const string SET_Q_RESPONSE =
        "качество картинок будет {0}%";

    public const string WITLESS_ONLY_COMAND =
        """
        Для использования этой команды нужно прописать

        /start{0}
        """;

    public const string MEME_MANUAL =
        """
        Я могу делать <u><b>{0}</b></u> из картинок 📸 и видео 🎬

        ‼️ Прикрепи файл к команде, либо ответь командой на файл.

        📖 Справка: /man_3
        ℹ️ Список опций: /op_{1}
        """;

    public const string SET_MEME_TYPE_MANUAL =
        """
        Выбрать тип авто-мемов:

        <code>/meme</code> ☝️👇 <code>/set M </code>
        <code>/top </code> 💬🔝 <code>/set T </code>
        <code>/dp  </code> ◼️▪️ <code>/set D </code>
        <code>/dg  </code> ◾️🔐 <code>/set Dg</code>
        <code>/nuke</code> 🍕🍤 <code>/set N </code>
        """;

    public const string SET_MEMES_RESPONSE =
        "Картинки будут превращаться в {0}";

    public const string PIECE_MANUAL =
        """
        Данной командой можно создать словарь 📔, состоящий из ссылок на все посты / сообщения ✍️ из определённого тг-канала / чата.

        <b>Синтаксис:</b>
        <code>/piece [ссылка на последний пост] [название]</code>

        <blockquote><b>Например:</b>
        /piece t.me/memes/1337 MEMES</blockquote>
        """;

    public const string PIECE_RESPONSE =
        """
        <b>Готово!</b> 🥂

        Чтобы влить словарь, пропишите:

        <code>/fuse {0}</code>
        """;

    public const string LINK_MANUAL =
        """
        Я храню ссылки на последние <b><i>{0} постов с реддита</i></b> которые я отослал с момента включения.

        Если вы за этим, пропишите /link в ответ на сообщение с постом
        """;

    public const string BUENOS_DIAS =
        """
        This is the certified {0} classic!
        =======
        {1} на связи!
        """;

    public const string FF_ERROR_REPORT =
        """
        Ошибка произошла во время выполнения следующей команды:

        {0}

        Если хотите чтоб её пофиксили - скиньте этот файл админу вместе с обрабатываемым файлом (づ｡◕‿‿◕｡)づ

        {1}

        Более детальный отчёт (для шарящих юзеров):

        {2}
        """;

    public const string REDDIT_MANUAL =
        """
        Если ты это написал, то очевидно, ты ничего не понял 😣 и тебе нужна инструкция 📃, так ведь? 😎

        <b><i><u>Смотри кароче</u></i></b>:

        <code>/w</code> - самая простая команда, вводишь после неё любой текст ✍️ и я ищю подходящие посты по всему реддиту 🔍
        <code>/ws</code> - более сложная, после неё уже надо вводить название сабреддита 🎟, и я кидаю посты оттуда 🛒

        Думаю, важно напомнить☝️что большая часть контента на реддите - на английском 🤓, а еще нам много нсфв 🥵

        Это так, простенький гайд, более полный можно найти в <a href='https://t.me/piece_fap_club/9228'>этом посте</a> и под ним
        """;

    public const string REDDIT_SUBS_MANUAL =
        """
        Тут всё просто 😎👌

        Пишешь после команды любой текст (желательно англ.) и я выдаю список сабреддитов, которые подходят под запрос
        """;

    public const string REDDIT_COMMENTS_START =
        """
        НАЧИНАЕМ ПРИЗЫВ СОТОНЫ!!!

        {0}
        """;

    public const string REDDIT_COMMENTS_MANUAL =
        """
        <b>Пожирание коментов с Reddit:</b>

        <code>/xd [search query] [subreddit*] [-options]</code>

        <b>Примеры:</b>

        ☝️ <code>/xd osaka okbuddybaka*</code>
        👉 <code>/xd okbuddybaka*</code>
        👊 <code>/xd ohio rizz okbuddyretard*</code>
        🤙 <code>/xd real trap shit</code>
        👌 <code>/xd amogus -cm</code>
        """;

    public const string SONG_MANUAL =
        """
        <b><u>Как скачивать музыку с ютуба:</u></b>

        Пишешь <code>/song *ссылка*</code>, и ждешь...

        Или пишешь <code>/song</code> в ответ на сообщение со ссылкой, которую ты, к примеру, нашёл через @vid.

        <b>Кроме того:</b>

        Если хочешь, можешь в конце указать автора и название, или просто название, например:

        <blockquote>/song https://youtu.be/fB6elql_EdM Eminem - My Name Is</blockquote>
        <blockquote>/song https://youtu.be/fB6elql_EdM Eminem's Name Is</blockquote>

        Также можешь найти 🔍 в интернете крутой арт 🌇 на обложку и отправить его вместе с командой, или ответить командой на него.

        А ещё, вставляя после <code>/song</code> определённые буквы, можно кастомизировать запрос, например:

        <code>q</code> - скачает трек в максимальном качестве (около 256 kb/s)
        <code>n</code> - сохранит музыку в формате <i>"Название.mp3"</i>
        <code>p</code> - поставит на обложку 1-ю секунду самого видео
        <code>c</code> - уберёт (текст) в [скобках] из названия трека
        <code>u</code> - укажет автора видео в качестве исполнителя
        <code>s</code> - обрежет обложку до квадрата
        <code>число</code> - скачает элемент из плэйлиста по его номеру

        <b>Итого:</b> <code>/song[опции] ссылка [автор - ][название]</code>
        """;

    public const string SET_MEME_OPS_RESPONSE =
        "Опции команды <b>{0}</b> изменены на <b>{1}</b>";

    public const string DP_OPTIONS =
        """
        <b>Опции команды</b> <code>/dp</code>:

        <code>#color#</code> - цвет текста и рамки
        <code>xx</code> - без верхушки и боковушек

        <b>Шрифты</b>: /fonts
        """;

    public const string TOP_OPTIONS =
        """
        <b>Опции команды</b> <code>/top</code>:

        <code>mm</code> - тонкая плашка
        <code>mm!</code> - супер тонкая плашка
        <code>la</code> - текст по левому краю
        <code>pp</code> - авто-выбор цвета (края)
        <code>pp!</code> - авто-выбор цвета (центр)
        <code>ob</code> - чёрный задник (для стикеров)
        <code>20%</code> - обрезать 20% сверху (0-100)
        <code>-20%</code> - обрезать по 10% сверху и снизу
        <code>10"</code> - стартовый размер шрифта (1-999)
        <code>min5"</code> - мин. размер шрифта (1-999)
        <code>blur</code> - лёгкое размытие 🎬
        <code>#color#</code> - цвет плашки

        <b>Шрифты</b>: /fonts
        """;

    public const string MEME_OPTIONS =
        """
        <b>Опции команды</b> <code>/meme</code>:

        <code>t</code> - только верхний текст
        <code>d</code> - только нижний текст
        <code>s</code> - добавлять нижний текст (к своему)
        <code>100%</code> - непрозрачность тени (0-100)
        <code>10"</code> - стартовый размер шрифта (1-999)
        <code>#color#</code> - цвет фона (стикеры)
        <code>cc</code> - текст случайного цвета 🎨

        <b>Шрифты</b>: /fonts
        """;

    public const string BOARD_START =
        """
        Тредов найдено: <b>{0}</b>

        {1}
        """;

    public const string BOARD_MANUAL =
        """
        Этой командой можно скармливать мне треды с форча.

        <b>Синтаксис:</b>
        <code>/board [ссылка на тред/доску/архив]</code>
        <code>/board [имя сохранёнки]</code>

        Список досок - <code>/boards</code>
        Список сохранёнок - <code>/boards info</code>
        Ссылка на архив = ссылка на доску + <code>archive</code>

        💀На абордаж! 🏴‍☠️
        """;

    public const string MAY_TAKE_A_WHILE =
        "(может занять до пары минут)";

    public const string FUSE_FAIL_BOARD =
        """
        К сожалению, я не нашёл сохранённых обсуждений с таким названием. Пропишите

        <code>/boards info</code>

        и выберите один из вариантов, или пропишите

        <code>/boards</code>

        чтобы найти новый материал
        """;

    public const string USE_ARROWS =
        "\n\nИспользуйте стрелочки для навигации ☝️🤓";

    public const string CROP_MANUAL =
        """
        <b><u>Обрезка видео</u></b> ✋🤚

        <b>Синтаксис:</b> <code>/crop ширина высота X Y</code>

        <b>Дефолтные значения:</b> <code>w</code>, <code>h</code>, <code>x</code>, <code>y</code>

        <b>Аргументы:</b>
        1. Разделяются пробелами
        2. Передаются строго по порядку
        3. Могут быть как числами, обозначающими кол-во пикселей, так и сложными тригонометрическими формулами по времени.


        <b>Валидные примеры:</b>

        <code>/crop min(w,h) min(w,h)</code> 👈 обрезать до квадрата
        <code>/crop w-20 h-20</code> 👈 срезать по 10 пикселей со всех сторон
        <code>/crop w/2</code> 👈 обрезать по 25% слева и справа
        <code>/crop w h*0.5 x 0</code> 👈 оставить только верхнюю половину

        <b>Вырезка квадрата 100×100 пикселей:</b>

        <code>/crop 100 100</code> 👈 с центра
        <code>/crop 100 100 0 0</code> 👈 с левого верхнего угла ↖️
        <code>/crop 100 100 w-100 h-100</code> 👈 с правого нижнего угла ↘️

        📖 <b><a href='https://ffmpeg.org/ffmpeg-filters.html#crop'>Справка ffmpeg/crop</a></b>
        """;

    public const string SCALE_MANUAL =
        """
        <b><u>Масштабирование</u></b>

        <b>Синтаксис:</b> <code>/scale ширина высота флаги</code>

        <b>Примеры:</b>

        <code>/scale 100</code> 👈 ширина - 100px
        <code>/scale -1 100</code> 👈 высота - 100px
        <code>/scale 1280 720</code> 👈 big black...

        ↗️ Увеличить в 2️⃣ раза:
        <code>/scale 2</code>
        <code>/scale w*2</code>

        ↘️ Уменьшить в 2️⃣ раза:
        <code>/scale 0.5</code>
        <code>/scale w/2</code>

        По дефолту соотношение сторон сохраняется

        <b>Использование флагов 🏁:</b>

        <code>/scale 0.5 -1 bilinear</code>
        <code>/scale 0.5 -1 lanczos</code>
        <code>/scale 0.5 -1 lanczos param0=8</code>
        <code>/scale 0.5 -1 bicubic+accurate_rnd</code>

        📖 <b><a href='https://ffmpeg.org/ffmpeg-filters.html#scale-1'>Справка ffmpeg/scale</a></b>
        📖 <b><a href='https://ffmpeg.org/ffmpeg-scaler.html#sws_005fflags'>Список флагов</a></b>
        """;

    public const string EQ_MANUAL =
        """
        <b><u>Эквалайзер</u></b>

        <b>Синтаксис:</b> <code>/eq [частота,Hz] [сила,dB] [ширина,Hz]</code>

        <b>Дефолтные значения:</b> <code>~</code>, <code>10</code>, <code>2000</code>

        <b>Примеры:</b>

        <code>/eq 100</code> 👈 поднять басы на 10dB
        <code>/eq 1000</code> 👈 поднять средние частоты на 10dB
        <code>/eq 50 40 1000</code> 👈 поднять частоту 50Hz на 40dB
        <code>/eq 1000 100 10</code> 👈 запикать
        <code>/eq 10000 -50 8000</code> 👈 заглушить высокие частоты

        <b>Более подробно <a href='https://ffmpeg.org//ffmpeg-filters.html#equalizer'>тут</a></b>
        """;

    public const string VOLUME_MANUAL =
        """
        <b><u>Громкость</u></b>

        <b>Синтаксис:</b> <code>/vol число/формула</code>

        <b>Примеры:</b>

        <code>/vol 2</code> 👈 сделать в 2 раза громче
        <code>/vol 10</code> 👈 сделать в 10 раз громче
        <code>/vol 0.5</code> 👈 сделать в 2 раза тише
        <code>/vol 0.5+t*0.1</code> 👈 рост громкости с ×0,5 до ×2 за 15 секунд
        <code>/vol if(gt(t,1),1,t)</code> 👈 плавный 1-секундный вход
        <code>/vol if(gt(t,0.5),1,2*t)</code> 👈 плавный полсекундный вход

        <b>Более подробно <a href='https://ffmpeg.org//ffmpeg-filters.html#volume'>тут</a></b>
        """;

    public const string TRACTOR_GAME_RULES =
        """
        🎮 <b>SUPER GAMING BATTLE</b> 🎳

        🫵 Одолей меня чтобы удалить словарь

        🏠 - я, 🚜 - ты
        """;

    public const string FUSE_HIS_ALL =
        "\n\n<code>/fuse * *</code> - скормить всё сразу";

    public const string ONLY_ARRAY_JSON =
        "Годятся только <b>JSON</b>-файлы, в виде <b>списка строк</b>, например:";

    public const string FONTS_CHEAT_SHEET =
        """
        📝 <u><b>Шрифты:</b></u>

        <u><b>Обычные:</b></u>
        <code>im</code> - <b><i>Impact</i></b>
        <code>rg</code> - <b><i>Roboto</i></b> ✨
        <code>sg</code> - <b><i>Segoe UI</i></b> ✨
        <code>ro</code> - <b><i>Times New Roman</i></b> ✨
        <code>co</code> - <b><i>Comic Sans MS</i></b> ✨
        <code>bb</code> - <b><i>Bender</i></b> ✨
        <code>ft</code> - <b><i>Futura XBlkCn BT</i></b>

        <u><b>Комиксные:</b></u>
        <code>ap</code> - <b><i>v_Armor Piercing 2.0 BB</i></b>
        <code>bc</code> - <b><i>v_CCBattleCry-Regular</i></b> 🔠
        <code>bl</code> - <b><i>v_Blowhole BB</i></b> 🔠
        <code>mc</code> - <b><i>v_CCMarianChurchland</i></b> 🔠
        <code>vb</code> - <b><i>v_Billy The Flying Robot BB LC</i></b>
        <code>vg</code> - <b><i>v_GiantSizedSpectacular Std BB</i></b>

        <u><b>Тематические:</b></u>
        <code>ru</code> - <b><i>CyrillicOld</i></b> - летописный
        <code>go</code> - <b><i>CyrillicGoth</i></b> - готический
        <code>st</code> - <b><i>a_Stamper</i></b> 🔠 - трафаретный
        <code>vn</code> - <b><i>v_NokiaCellphoneFC</i></b> - пиксельный
        <code>vp</code> - <b><i>v_Pythia</i></b> - древнегреческий

        ✨ - поддержка стилей.
        🔠 - только верхний регистр.


        📖 <u>Как использовать</u>: /man_33
        """;

    public const string DG_OPTIONS =
        """
        <b>Опции команды</b> <code>/dg</code>:

        <code>ll</code> - в одну строку, без нижнего текста
        <code>nn</code> - без вотермарок
        <code>__&</code> - шрифт верхнего текста
        <code>__*</code> - шрифт нижнего текста

        <b>Шрифты</b>: /fonts
        """;

    public const string NUKE_OPTIONS =
        """
        <b>Опции команды</b> <code>/nuke</code>:

        <code>N"</code> - кол-во проходов (1-9 📸, 1-3 🎬)
        """;

    public const string PUB_EX_NOT_FOUND =
        """
        {0} не могу найти словаря с таким названием((

        Пропишите <code>/fuse ! info</code>, чтобы посмотреть весь список.
        """;

    public const string PUB_EX_DONE =
        """
        Словарь <b>"{0}"</b> опубликован!

        Проверить: <code>/fuse info</code>
        """;

    public const string EDIT_MANUAL_SYN =
        """
        ❗️ Команда работает с {0}

        📖 Справка по обработке: /man_4
        📖 Синтаксис команды: {1}
        """;

    public const string EDIT_MANUAL =
        """
        ❗️ Команда работает с {0}

        📖 Справка по обработке: /man_4
        """;
}