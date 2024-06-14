### Capabilities 👌:
- Random text generation 💬
- Memes and demotivators 📸🎬
- Simple video and audio editing 🎬🎧
- Images to stickers 📸 -> 🎟
- Music to voice messages 🎧 -> 🎤
- Videos to videonotes / GIFs
- Executing any **[ffmpeg]** scripts
- Searching for posts on **[Reddit]**
- Downloading music from **[YouTube]** (in the best way possible 😎)

Can be used in **group chats** as well as in **DMs**. Text generation is based on an enchanced [Markov chain] algorithm. Text generation pack for your chat can be extended by sending messages and by fusing it with other chats packs, with reddit comments, and with [exported chat history]. Frequency of text generation can be changed.

### Initial setup ⚙️
- Have **ffmpeg binaries**, **yt-dlp.exe** and **gallery-dl.exe** locations added to **Path**.
- Create in the working directory **config.txt** file (see example).
- Create in the working directory **_Telegram-Arts\ASCII_** folder and drop there some _ASCII-arts.txt_.
- Create in the working directory **_Telegram-Water_** folder and drop there at least one watermark for demotivators - a **.png** file with a name of two numbers - X and Y of top-left corner for placing the watermark onto the demotivator. Any extra words can be placed in the middle, e.g: **586 700.png**, **586 x 700.png**. For no watermarks simply put there 1x1px black square named **0 0.png**.
- Create in the working directory **_Emoji_** folder and drop there all existing emoji (or just the most used ones). They should be transparent **.png** files named as their UTF-8 code points - e.g. 😳 will be **1f633.png**.
- Create in the working directory **BT.json** file with a data for "бугурт-тред" generation, e.g:

`{ "_start": { "_start": 1.0 }, "_mid": { "_end": 1.0, "_start": 3.0 } }`

### Main console commands 💬
- **s** - saves all data and shuts bot down
- **/** - lists all console commands
- **/sp** - send the message from **spam.txt** file to all chats

[Reddit]: <https://www.reddit.com/>
[YouTube]: <https://youtu.be/dQw4w9WgXcQ>
[ffmpeg]: <https://ffmpeg.org/ffmpeg-filters.html>
[Markov chain]: <https://en.wikipedia.org/wiki/Markov_chain>
[exported chat history]: <https://www.maketecheasier.com/export-telegram-chat-history/>
