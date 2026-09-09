namespace PF_Bot.Backrooms.Literals;

public static partial class Texts
{
    public const string DG_OPTIONS =
        """
        <b>Опции команды</b> <code>/dg</code>:

        <code>ll</code> - в одну строку, без нижнего текста
        <code>nn</code> - без вотермарок
        <code>__&</code> - шрифт верхнего текста
        <code>__*</code> - шрифт нижнего текста

        <b>Шрифты</b>: /fonts
        <b>Общие опции</b>: /man_32
        """;

    public const string DP_OPTIONS =
        """
        <b>Опции команды</b> <code>/dp</code>:

        <code>#color#</code> - цвет текста и рамки
        <code>xx</code> - без верхушки и боковушек
        <code>100"</code> - стартовый размер шрифта (1-999)
        <code>min10"</code> - мин. размер шрифта (1-999)

        <b>Шрифты</b>: /fonts
        <b>Общие опции</b>: /man_32
        """;

    public const string MEME_OPTIONS =
        """
        <b>Опции команды</b> <code>/meme</code>:

        <code>lo</code> - текст нижним регистром
        <code>t</code> - только верхний текст
        <code>d</code> - только нижний текст
        <code>s</code> - добавлять нижний текст (к своему)
        <code>mm</code> - текст без отступов
        <code>mm!</code> - текст вылазит за края
        <code>100%</code> - непрозрачность тени (0-100)
        <code>100w</code> - толщина тени (0-999)
        <code>100"</code> - стартовый размер шрифта (1-999)
        <code>50!</code> - сдвиг текста (0-100)
        <code>!!</code> - случайный сдвиг текста
        <code>#color#</code> - цвет текста
        <code>!color!</code> - цвет тени
        <code>_color_</code> - цвет фона (стикеры)
        <code>cc</code> - текст случайного цвета 🎨

        <b>Шрифты</b>: /fonts
        <b>Общие опции</b>: /man_32
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
        <code>100"</code> - стартовый размер шрифта (1-999)
        <code>min10"</code> - мин. размер шрифта (1-999)
        <code>#color#</code> - цвет плашки

        <b>Шрифты</b>: /fonts
        <b>Общие опции</b>: /man_32
        """;

    public const string SNAP_OPTIONS =
        """
        <b>Опции команды</b> <code>/snap</code>:

        <code>62%</code> - непрозрачность плашки (0-100)
        <code>50!</code> - сдвиг плашки (0-100)
        <code>!!</code> - случайный сдвиг плашки
        <code>100"</code> - стартовый размер шрифта (1-999)
        <code>min10"</code> - мин. размер шрифта (1-999)
        <code>#color#</code> - цвет текста
        <code>!color!</code> - цвет плашки
        <code>_color_</code> - цвет фона (стикеры)

        <b>Шрифты</b>: /fonts
        <b>Общие опции</b>: /man_32
        """;

    public const string NUKE_OPTIONS =
        """
        <b>Опции команды</b> <code>/nuke</code>:

        <code>N"</code> - кол-во проходов (1-9 📸, 1-3 🎬)
        <code>Nx</code> - вероятность пикселизации (0-100)

        <b>Общие опции</b>: /man_32
        """;

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
        <code>ug</code> - <b><i>Upright (Sigma)</i></b>

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

        <code>^^</code> - рандомный 🎲

        ✨ - поддержка стилей.
        🔠 - только верхний регистр.


        👀 <u>В виде картинки</u>: /fonts_0
        📖 <u>Как использовать</u>: /man_33
        """;
}