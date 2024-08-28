### Capabilities 👌:
- Random text generation 💬
- Memes and demotivators 📸🎬😂👌
- Media files editing/converting 📸🎬🎧
- Executing any **[ffmpeg]** and **[imagemagick]** commands 🔥✍️
- _~Fetching **[Reddit]** posts~_ [temporarily unavailable] ❌
- Downloading music & clipping videos from **[YouTube]** 😎

##### Honorable mentions 😲:
- 💬 Generation is based on an enhanced [Markov chain] algorithm.
- 📦 The generation packs can be easily cleaned, or extended via:
  - other chat packs 📦,
  - text files 📄,
  - 4chan threads 🍀.
- ⚙️ Highly customizable.
- 💭 Can be used in group chats and DMs.

### Setup ⚙️
- Have `dotnet-sdk` installed.
- Have `ffmpeg`, `imagemagick`, `yt-dlp`, and `gallery-dl` on your `PATH`.
- Setup the working directory as shown in [`Paths.cs`](src/Backrooms/Static/Paths.cs). Required items:
  - [`config.txt`](config-example.txt),
  - `DB/default.json`,
  - the whole `Static` directory.

### Operating the bot console 💬
- **s** - saves all data and shuts the bot down.
- **/** - lists all other console commands.

[Reddit]: <https://www.reddit.com/>
[YouTube]: <https://youtu.be/dQw4w9WgXcQ>
[ffmpeg]: <https://ffmpeg.org/ffmpeg-filters.html>
[imagemagick]: <https://imagemagick.org/script/command-line-options.php>
[Markov chain]: <https://en.wikipedia.org/wiki/Markov_chain>
