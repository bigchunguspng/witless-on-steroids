# нечёткие приколы™
_Telegram-bot made for fun_

### Features:
- Nonsense text generation
- Memes and demotivators 📸/🎬
- Simple video and audio editing 🎬/🎧
- Making stickers from images 📸 --> 🎟
- Making animations and videonotes from videos
- Searching for memes on **[Reddit]**
- Downloading music from YouTube

Bot can be used in group chats and DMs. Text generation is based on [Markov chain]. Bot vocabulary in a specific chat can be increased by sending messages, fusing it with other chats' vocabulary, fusing it with reddit comments or with [chat history]. Frequency of text generation can be changed.

### Initial setup
- Create in working directory **config.txt** file (see example).
- Have **ffmpeg binaries** and **yt-dlp.exe** locations added to **Path**.
- Create in working directory **_Telegram-Arts\ASCII_** folder and drop there some ASCII-arts in txt format.
- Create in working directory **_Telegram-Water_** folder and drop there at least one watermark for demotivators. It has to be a **.png** file, and it's name should contain two numbers - X and Y of top left corner of the watermark when it's placed on the demotivator. For watermarks with the same coords, any extra words can be placed in the middle, e.g: **52 580.png**, **52 a 580.png**. Or just simply made 1x1px black square and named it **0 0.png**.
- Create in working directory **_Emoji_** folder and drop there all emoji (or just the most used ones). They should be transparent **.png** files named as their UTF-8 code points, e.g. 😳 will be **1f633.png**. All of them you can get [here].
- Create in working directory **BT.json** file with a data for "bouhourt-thread" generation, e.g:

`{ "_start": { "_start": 1.0 }, "_mid": { "_end": 1.0, "_start": 3.0 } }`

### Main console commands
- **s** - saves all data and shuts bot down
- **/** - lists all console commands
- **\+55** - activates the first chat which ID ends with **55**
- **/w text** - send **text** message to the active chat
- **/sp** - send the message from **spam.txt** file to all chats

[Reddit]: <https://www.reddit.com/>
[Markov chain]: <https://en.wikipedia.org/wiki/Markov_chain>
[chat history]: <https://www.maketecheasier.com/export-telegram-chat-history/>
[here]:<https://emojipedia.org/>
