namespace PF_Bot.Backrooms.Literals;

public static partial class Texts
{
    public const string EDIT_MANUAL =
        """
        ❗️ Команда работает с {0}

        📖 Справка по обработке: /man_4
        """;

    public const string EDIT_MANUAL_SYN =
        """
        ❗️ Команда работает с {0}

        📖 Синтаксис команды: {1}
        📖 Справка по обработке: /man_4
        """;

    public const string MEME_MANUAL =
        """
        ❗️ Команда работает с фото 📸 и видео 🎬

        📖 Справка: /man_3
        ℹ️ Список опций: /op_{0}
        """;

    public const string AUTO_MANUAL =
        """
        ☝️ Установите автообработчик или передайте его аргументом

        📖 Справка: /man_341
        """;

    public const string DEBUG_MANUAL =
        """
        ☝️ Отправь это в ответ на любое сообщение ↩️

        <code>/debug  </code> - полностью
        <code>/debug! </code> - коротко (только с файлами)
        <code>/debug !</code> - ???
        """;

    public const string DEBUG_EX_MANUAL =
        """
        <code>/debug [code]</code>

        <code>m</code> → memory usage
        <code>p</code> → packs info
        <code>r</code> → reddit cache
        <code>e</code> → emoji cache
        <code>g</code> → top GIF tags
        <code>a</code> → top audio tags
        """;

    public const string SET_MANUAL =
        """
        ⚙️ <u><b>Тип авто-мемов</b></u>:

        <code>/set [код]</code>
        <blockquote><b>Варианты</b>:
        <code>/set m </code> - мемы☝️
        <code>/set t </code> - подписанки 💬
        <code>/set d </code> - демотиваторы👌
        <code>/set dg</code> - демотиваторы💀
        <code>/set s </code> - снапчаты 😭
        <code>/set n </code> - ядерный фритюр 🍤
        <code>/set a </code> - авто-обработка 👾</blockquote>

        ⚙️ <u><b>Авто-опции</b></u>:

        <code>/set [код] [опции]</code>
        <blockquote><b>Примеры</b>:
        <code>/set m wwsd#lime#</code>
        <code>/set t largmmww50"*</code>
        <code>/set d imup</code>
        <code>/set dg xllro-b</code>
        <code>/set s !!80%*85</code>
        <code>/set n =3</code>
        <code>/set a u:song; psg:meme</code></blockquote>

        <code>/set [код] ?</code> - узнать 👁
        <code>/set [код] 0</code> - сбросить ❌
        <code>/set [код]+ [опции]</code> - добавить
        <code>/set [код]- [опции]</code> - убрать

        👌 <u><b>Тип и опции сразу</b></u>:
        <code>/set [код]! [опции]</code>
        """;

    public const string SET_MEME_TYPE_MANUAL =
        """
        🤨 Чё ещё за "{0}"?

        Выбери код из списка:

        <code>m </code> → <b>/meme</b>
        <code>t </code> → <b>/top</b>
        <code>d </code> → <b>/dp</b>
        <code>dg</code> → <b>/dg</b>
        <code>s </code> → <b>/snap</b>
        <code>n </code> → <b>/nuke</b>
        <code>a </code> → <b>авто-обработка</b>
        """;

    public const string FUSE_MANUAL =
        """
        <b>Слияние / кормёжка</b>

        <u>Команда работает с:</u>
        🔑 айдишниками чатов
        📦 именами словарей со склада
        🗞 TXT, JSON и ASS файлами
        🗞 именами файлов со склада

        <u>Списки:</u>
        <code>/fuse   info</code> - общий склад 📦
        <code>/fuse ! info</code> - личный склад 📦
        <code>/fuse @ info</code> - общий склад 🗞
        <code>/fuse * info</code> - личный склад 🗞

        📖 Гайды: /man_22, /man_221, /man_222
        """;

    // EDITING

    public const string PIPE_MANUAL =
        """
        <b><u>Труба</u></b> 🔩

        Позволяет пропускать файл через несколько команд за раз.

        <b>Синтаксис:</b>
        <code>/pipe cmd 1 > cmd 2 > cmd N</code>

        Вместо "<code>cmd X</code>" подставляется команда без слэша. Опции и аргументы по желанию.

        <b>Примеры:</b>
        <blockquote expandable>📸 <code>/pipe meme > nuke</code> 👈 зажаренный мем
        📸 <code>/pipe meme3 > nuke3"2</code>
        └─ 3 мема, хорошо зажарены в 2 вариантах
        📸 <code>/pipe meme3 > nuke > im o4:4! .</code>
        └─ 3 мема с прожаркой и фильтром
        📸 <code>/pipe meme POV: > memed^^cc9</code>
        └─ 9 мемов с 🎲 нижним текстом
        🎬 <code>/pipe meme > dp > toppp > shake</code>
        └─ комбо-мем + тряска
        🎬 <code>/pipe shake > damn > nuke > fast 3</code>
        └─ жмынуть и ускорить
        🎧 <code>/pipe damn > eq 100 20 1000 > eq 4000 -30 1000</code> 👈 💀💀💀</blockquote>
        """;

    public const string G_MANUAL =
        """
        <b><u>Превращение в гифку</u></b>

        <code>/g</code>
        <blockquote>🎬 Видео → видео без звука.
        📸 Фотка → гифка на 5 секунд.</blockquote>

        <code>/g [N]</code><i>, где N ∈ [0.01 - 120]</i>
        <blockquote>📸 Фотка → гифка на N секунд.</blockquote>
        """;

    public const string CUT_MANUAL =
        """
        <u><b>Обрезка (и скачивание) видео</b></u>

        <blockquote><b>Примеры:</b>
        <code>/cut 10</code> ← первые 10 секунд
        <code>/cut 10 .</code> ← всё после 10-й секунды
        <code>/cut 10 5</code> ← с 10-й секунды по 15-ю
        <code>/cut 10 - 15</code> ← тоже</blockquote>

        ⏰ Как передавать время?
        <blockquote><b>В любом удобном формате:</b>
        10 ← 10 секунд
        1:28 ← минута, 28 секунд
        3.65 ← 3 секунды, 650 мс.</blockquote>

        📎 Как скачать по ссылке?
        <blockquote><b>Определённый фрагмент:</b>
        /cut https://youtu.be/dydER0YegMc 1:30
        /cut 1:30 https://youtu.be/dydER0YegMc
        <b>Полностью</b> (если видео короткое):
        /cut https://youtu.be/PgGrwmFWr0I
        </blockquote>
        """;

    public const string CUT_TOO_BIG_RESPONSE =
        """
        {1} К сожалению, скачанный файл весит <b>{0}</b>, что больше допустимого лимита в <b>50 МБ</b>. 

        ☝️ Как вариант, я могу прислать фрагмент видео. Для этого, отредактируйте сообщение, указав таймкоды нужного фрагмента ✍️

        📖 Синтаксис команды: /man_cut
        """;

    public const string SUS_MANUAL =
        """
        <u><b>Реверсы как в пупах</b></u>

        <blockquote><b>Что зареверсить:</b>
        <code>/sus</code> ← первую половину
        <code>/sus 0</code> ← целиком
        <code>/sus 3</code> ← первые 3 секунды
        <code>/sus 15 x</code> ← всё после 15-й секунды
        <code>/sus 10 1</code> ← 11-ю секунду</blockquote>

        ⏰ Как передавать время?
        <blockquote><b>В любом удобном формате:</b>
        10 ← 10 секунд
        1:28 ← минута, 28 секунд
        3.65 ← 3 секунды, 650 мс.</blockquote>
        """;

    public const string SLICE_MANUAL =
        """
        <u><b>Нарезка видео</b></u>

        <b>Синтаксис:</b>
        <code>/slice[дк][*др] [таймкоды]</code>

        <code>дк</code> - относительная длина кусков
        <code>др</code> - относительная длина разрывов
        <blockquote>Стандартное значение для <code>дк</code> - 5.
        Если <code>др</code> не указан, то <code>др</code> = <code>дк</code>.</blockquote>
        <code>таймкоды</code> - указать фрагмент <i>(см. /man_cut)</i>

        <blockquote><b>Примеры:</b>
        <code>/slice</code> ← стандартно
        <code>/slice5</code> ← стандартно
        <code>/slice1</code> ← короткие куски и разрывы
        <code>/slice25</code> ← длинные куски и разрывы
        <code>/slice1*5</code> ← короткие куски, обычные разрывы
        <code>/slice 50</code> ← нарезать только первые 50 секунд</blockquote>

        ☝️ Перевес в сторону <code>дк</code> даёт более длинные видео.
        """;

    public const string RANDOM_MANUAL =
        """
        Ещё нету, приходи позже...
        """;

    public const string CROP_MANUAL =
        """
        <b><u>Обрезка кадра</u></b> ✋🤚

        <b>Синтаксис:</b>
        ☝️ <code>/crop ширина высота X Y</code>
        ✌️ <code>/crop %_обрезки сторона</code>

        <code>сторона</code> = <code>[tlbr]</code> - <code>top</code> / <code>left</code> / <code>bottom</code> / <code>right</code>

        <b>Валидные примеры:</b>
        <blockquote expandable><code>/crop 50 b</code> 👈 убрать нижнюю половину
        <code>/crop 20 t</code> 👈 убрать верхние 20%
        <code>/crop min(w,h) min(w,h)</code> 👈 обрезать до квадрата
        <code>/crop w-20 h-20</code> 👈 срезать по 10 пикселей со всех сторон
        <code>/crop w/2</code> 👈 обрезать по 25% слева и справа</blockquote>

        <b>Вырезка квадрата 100×100 пикселей:</b>
        <blockquote expandable><code>/crop 100 100</code> 👈 с центра
        <code>/crop 100 100 0 0</code> 👈 с левого верхнего угла ↖️
        <code>/crop 100 100 w-100 h-100</code> 👈 с правого нижнего угла ↘️</blockquote>

        📖 <b><a href='https://ffmpeg.org/ffmpeg-filters.html#crop'>Справка ffmpeg/crop</a></b>
        """;

    public const string SHAKE_MANUAL =
        """
        /text
        <b><u>Тряска</u></b> 🫨

        <b>Синтаксис:</b> <code>/shake %_обрезки скорость сдвиг</code>

        <b>Дефолтные значения:</b> <code>0.95</code>, <code>random(0)</code>, <code>random(0)</code>

        <b>Простые примеры:</b>
        😵‍💫 <code>/shake 0.99</code> - лёгкая тряска
        😵 <code>/shake 0.75</code> - сильная тряска
        🫨 <code>/shake 0.75 30</code> - быстрая сильная
        😤 <code>/shake 0.75 3</code> - медленная сильная

        <b>Сложный пример:</b>
        <blockquote expandable><code>/shake 0.97 pow(max(0,n-20),2)/1200 random(0)*pow(max(0,n-20),2)/(100*t)</code>

        <code>0.97</code> - % видео, остающийся в кадре
        <code>n</code> - текущий кадр
        <code>t</code> - текущее время
        <code>20</code> - кадр с которого начинается тряска
        <code>/1200</code> - калибруем период нарастания
        <code>/(100*t)</code> - калибруем рандомность</blockquote>

        <b>Формула:</b>
        <blockquote expandable><code>/shake</code> = <code>/crop</code>, где
        <code>w</code> = <code>w*({%_обрезки})</code>
        <code>h</code> = <code>h*({%_обрезки})</code>
        <code>x</code> = <code>(w-out_w)/2+((w-out_w)/2)*sin(t*({скорость})-{сдвиг})</code>
        <code>y</code> = <code>(h-out_h)/2+((h-out_h)/2)*sin(t*({скорость})+2*{сдвиг})</code></blockquote>

        📖 <a href="https://ffmpeg.org/ffmpeg-filters.html#crop"><b>Справка ffmpeg/crop</b></a>
        """;

    public const string SPEED_MANUAL =
        """
        ⚡️ <b><u>Ускорение и замедление</u></b> 🐌

        <b>Синтаксис:</b>
        <code>/fast[опции] [скорость]</code>
        <code>/slow[опции] [длительность]</code>

        <b>Примеры:</b>
        <code>/fast</code> ← ускорить в 2 раза
        <code>/fast 3</code> ← ускорить в 3 раза
        <code>/slow 5</code> ← замедлить в 5 раз
        <code>/fastp 1.25</code> ← ускорить в 1.25 раза, отпустив питч

        <b>Опции:</b>
        <code>p</code> ← изменять <u>п</u>итч аудио
        <code>r</code> ← использовать аудиофильтр <i><u>r</u>ubberband</i> вместо <i>atempo</i> (другое качесто)

        Стандартные скорость и длительность - 2. При использовании опции <code>p</code>, фильтр <i>rubberband</i> используется автоматически.
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

        <b>Дефолтные значения:</b> <code>~</code>, <code>15</code>, <code>2000</code>

        <b>Примеры:</b>

        <code>/eq 100</code> 👈 поднять басы на 15dB
        <code>/eq 1000</code> 👈 поднять средние частоты на 15dB
        <code>/eq 50 40 1000</code> 👈 поднять частоту 50Hz на 40dB
        <code>/eq 1000 100 10</code> 👈 запикать
        <code>/eq 10000 -50 8000</code> 👈 заглушить высокие частоты

        📖 <b>Более подробно <a href='https://ffmpeg.org//ffmpeg-filters.html#equalizer'>тут</a></b>
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
        <code>/vol min(1,t)</code> 👈 плавный 1-секундный вход
        <code>/vol min(1,2*t)</code> 👈 плавный полсекундный вход

        📖 <b>Более подробно <a href='https://ffmpeg.org//ffmpeg-filters.html#volume'>тут</a></b>
        """;

    // INTERNET

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

    public const string PIECE_MANUAL =
        """
        🍱 <u><b>Блин блинский мангу скачать</b></u>

        🧐 Откуда качаем? <a href='https://tcbonepiecechapters.com/'>Отсюда</a>

        <b>Синтаксис:</b>
        <code>/piece info</code> - список тайтлов (числа и кодовые имена)
        <code>/piece [тайтл]</code> - список глав
        <code>/piece [тайтл] [глава]</code> - скачать

        <blockquote expandable><b>Примеры:</b>
        📃 <code>/piece one-piece</code>
        📃 <code>/piece 5</code>
        💾 <code>/piece one-piece 1045</code>
        💾 <code>/piece 5 1045</code>
        📃 <code>/piece jujutsu-kaisen</code>
        📃 <code>/piece 4</code>
        💾 <code>/piece jujutsu-kaisen 221</code>
        💾 <code>/piece 4 221</code></blockquote>
        """;

    public const string REDDIT_MANUAL =
        """
        Если ты это написал, то очевидно, ты ничего не понял 😣 и тебе нужна инструкция 📃, так ведь? 😎

        <b><i><u>Смотри кароче</u></i></b>:

        <code>/w</code> - самая простая команда, вводишь после неё любой текст ✍️ и я ищю подходящие посты по всему реддиту 🔍
        <code>/ws</code> - более сложная, после неё уже надо вводить название сабреддита 🎟, и я кидаю посты оттуда 🛒

        Думаю, важно напомнить☝️что большая часть контента на реддите - на английском 🤓, а еще нам много нсфв 🥵

        Но это так, простенький гайд, более полный можно найти прописав /man_5
        """;

    public const string REDDIT_SUBS_MANUAL =
        """
        Тут всё просто 😎👌

        Пишешь после команды любой текст (желательно англ.) и я выдаю список сабреддитов, которые подходят под запрос
        """;

    public const string REDDIT_LINK_MANUAL =
        """
        Я храню ссылки на последние <b><i>{0} постов с реддита</i></b> которые я отослал с момента включения.

        Если вы за этим, пропишите /link в ответ на сообщение с постом
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

    public const string BOARD_MANUAL =
        """
        🍀 <b><u>Кормёжка тредами с <a href="https://www.4chan.org/">форчана</a></u></b>

        <b>Синтаксис:</b>
        <blockquote expandable><code>/board [тред/доска/архив]</code>
        /board <a href="https://boards.4chan.org/a/">a</a>
        /board https://boards.4chan.org/a/
        /board https://boards.4chan.org/vm/archive
        /board https://boards.4chan.org/g/thread/102519935
        /board https://desuarchive.org/a/thread/273519954</blockquote>
        <blockquote><code>/board [имя сохранёнки]</code>
        /board 2024-12-06 g.103418613</blockquote>
        <blockquote><code>/board [_/код доски](!) [запрос]</code>
        /board a higurashi ← <i>by text</i>
        /board a! made in abyss ← <i>by subject</i>
        /board _ hood classic</blockquote>

        <code>/boards</code> - список досок 🍀📝
        <code>/board info</code> - список сохранёнок 💾📝 
        """;

    public const string PLANK_MANUAL =
        """
        ⚡️ <b><u>Кормёжка тредами с <a href="https://2ch.org/">двача</a></u></b>

        <b>Синтаксис:</b>
        <blockquote expandable><code>/plank [тред/доска]</code>
        /plank <a href="https://2ch.org/a/">a</a>
        /plank https://2ch.org/a/
        /plank https://2ch.org/hw/
        /plank https://2ch.org/a/res/7819159.html</blockquote>
        <blockquote><code>/plank [имя сохранёнки]</code>
        /plank 2024-12-08 di.527818</blockquote>
        <blockquote><code>/plank [код доски] [запрос]</code>
        /plank a ван пис
        /plank mu рхчп</blockquote>

        <code>/planks</code> - список досок ⚡️📝
        <code>/plank info</code> - список сохранёнок 💾📝
        """;

    //

    public const string SPAM_MANUAL =
        """
        <code>/spam[g/p/aN/sB]? [text|message]</code>

        <u><b>Chat filters</b></u>:
        <code>g</code> → only <u><b>g</b></u>roups
        <code>p</code> → only <u><b>p</b></u>rivate chats

        📆 Last <u><b>a</b></u>ctivity, days ago
        <blockquote><code>a[&gt;|&lt;|&gt;=|&lt;=]?[int]</code>
        <i>Examples</i>:
        - <code>a&lt;30</code> → some activity in last 30 days
        - <code>a&gt;7 </code> → no activity in last 7 days</blockquote>
        📦 Pack <u><b>s</b></u>ize, [K|M]B
        <blockquote><code>s[&gt;|&lt;|&gt;=|&lt;=]?[int][K|M]?</code>
        <i>Examples</i>:
        - <code>s&gt;=10M</code> → pack is 10 MB minimum
        - <code>s&lt;100K</code> → pack is less than 100 KB
        - <code>s&lt;=34 </code> → pack is 34 bytes (empty)</blockquote>
        """;
}