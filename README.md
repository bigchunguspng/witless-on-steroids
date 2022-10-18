# witless_sister
_Телеграм бот для генерации сообщений на основе сообщений других пользователей_

Алгоритм генерации основан на [цепи Маркова]. Бот запоминает сообщения из чата, и каждые **N** сообщений генерирует и отправляет свое сообщение. Число **N** можно изменять. Словари разных чатов можно объединять. Бот может составлять словари на основе [истории переписки]. Работает как в беседах, так и в личной переписке. Помимо того бот может генерировать из картинок, стикеров, видео и гифок демотиваторы, ускорять и замедлять медиафайлы, реверсить, обрезать, сжимать и т.д.

##### Первоначальная настройка
1. Создать в рабочей папке ***(~\Witlesss\bin\Debug\netcoreapp3.1)*** файл **.token** и записать туда токен бота.
2. Поместить **ffmpeg.exe** в папку **_C:\ffmpeg\bin_** (нужен для работы с видео и аудио).
3. Создать в рабочей папке папку **_Telegram-Water_** и поместить в неё хотя бы одну вотермарку для демотиваторов. Вотермарка должна иметь расширение **.png**, а её название должно состоять как минимум из двух чисел, обозначающих координаты верхнего левого угла вотермарки на демотиваторе: первое - **X**, последнее - **Y**. Если есть вотерки с одинаковыми координатами, дополнительные слова можно вписать между координатами. Примеры названий: **52 580.png**, **52 a 580.png**. Если вотермарки не нужны, можно создать заглушку - 1x1 пиксель чёрного цвета **0 0.png**.
4. Создать в рабочей папке папку **_Emoji_** и поместить в неё все или хотя бы самые популярные эмоджи, в виде прозрачных png-картинок названных соответственно точкам кода UTF-8 передающих их символов, например 😳 - это **1f633.png**.

Все остальные папки создадутся программой автоматически по мере необходимости. Для синхронизации словарей одних и тех же чатов с разных устройств, создайте в рабочей папке папку ***Telegram-CopyDBs***, закиньте в неё словари с другого устройства и выполните синхронизацию командой **/d**.

##### Консольные команды
- **\+55** - делает активным первый чат, ID которого заканчивается на **55**
- **/w текст** - отправляет в активный чат **текст** как сообщение и добавляет **текст** в его словарь
- **/a текст** - добавляет **текст** в словарь активного чата
- **/s** - сохраняет словари всех чатов
- **/u** - рассылает сообщение из файла **.spam** по всем чатам
- **/с** - удаляет временные файлы (бот делает то же самое автоматически при запуске)
- **/k** - очищает словари всех чатов
- **/e** - удаляет словари недоступных чатов
- **/r** - удаляет пустые словари
- **/x** - чистит все словари от багов
- **/d** - производит синхронизацию словарей для всех чатов с их версиями из папки **Telegram-CopyDBs**
- **/u 100** - рассылает сообщение из файла **.spam** по всем чатам, словари которых весят больше чем **100** байт
- **/r 100** - удаляет словари которые весят меньше чем **100** байт
- **s** - сохраняет словари всех чатов и выключает бота

[цепи Маркова]: <https://ru.wikipedia.org/wiki/%D0%A6%D0%B5%D0%BF%D1%8C_%D0%9C%D0%B0%D1%80%D0%BA%D0%BE%D0%B2%D0%B0>
[истории переписки]: <https://www.maketecheasier.com/export-telegram-chat-history/>