using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Handlers.Manage.Packs;
using PF_Bot.Handlers.Memes.Core;
using Telegram.Bot.Types;

namespace PF_Bot.Handlers.Memes
{
    public class Nuke : MakeMemeCore<int>
    {
        private MemeOptions_Nuke _options;

        protected override IMemeGenerator<int> MemeMaker => new DukeNukem(_options, Chat);

        protected override Regex _cmd { get; } = new(@"^\/nuke(\S*)");

        protected override string VideoName => "piece_fap_bot-nuke.mp4";

        protected override string Log_STR => "NUKED";
        protected override string Command => "/nuke";
        protected override string Suffix  => "Nuked"; // Needs more nuking!

        protected override string? DefaultOptions => Data.Options?.Nuke;


        protected override Task Run()
        {
            if /**/ (Args is "log" or "logs" || Context.Command!.StartsWith("/nuke_log"))
                SendNukeLog(new ListPagination(Origin, PerPage: 5));
            else if (Args is null)
                return RunInternal("nuke\n⏳История фильтров: /nuke_log");

            return Task.CompletedTask;
        }

        protected override bool CropVideoNotes   => false;
        protected override bool ResultsAreRandom => true;

        protected override void ParseOptions()
        {
            _options.Depth = OptionsParsing.GetInt(Request, _depth, 1);
        }

        protected override int GetMemeText(string? text) => 0;

        private static readonly Regex _depth = new(@"^\/nuke\S*?([1-9])("")\S*");
        
        public static void HandleCallback(CallbackQuery query, string[] data)
        {
            var pagination = query.GetPagination(data);

            if      (data[0] == "nl") SendNukeLog (pagination);
            // else if (data[0] == "ni") SendNukeInfo(pagination); // nah, i'm lazy
        }

        private static void SendNukeLog(ListPagination pagination)
        {
            var (origin, messageId, page, perPage) = pagination;

            if (DukeNukem.Logs.TryGetValue_Failed(pagination.Origin.Chat, out var entries))
            {
                Bot.SendMessage(pagination.Origin, NUKE_LOG_EXPLANATION);
                return;
            }

            var single = entries.Count <= perPage;

            var lastPage = (int)Math.Ceiling(entries.Count / (double)perPage) - 1;
            var sb = new StringBuilder("🍤 <b>Последние вариации /nuke:</b>");
            if (single.Janai()) sb.Append($" 📃{page + 1}/{lastPage + 1}");
            sb.Append("\n\n").AppendJoin('\n', GetNukeLog(entries, page, perPage));
            sb.Append("\n\nИспользование: <code>/pegc [фильтр] .</code>");
            if (single.Janai()) sb.Append(USE_ARROWS);
                
            var buttons = single ? null : GetPaginationKeyboard(page, perPage, lastPage, "nl");
            Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
        }

        private static IEnumerable<string> GetNukeLog(List<DukeNukem.NukeLogEntry> entries, int page = 0, int perPage = 25)
        {
            if (entries.Count == 0)
            {
                yield return NUKE_LOG_EXPLANATION;
                yield break;
            }

            foreach (var entry in entries.OrderByDescending(x => x.Time).Skip(page * perPage).Take(perPage))
            {
                var logo = entry.Type switch
                {
                    MemeSourceType.Image => "📸",
                    MemeSourceType.Sticker => "🎟",
                    MemeSourceType.Video => "🎬",
                    _ => throw new ArgumentOutOfRangeException()
                };
                yield return $"{logo} <b>{entry.Time:MM'/'dd' 'HH:mm:ss}</b>\n<blockquote><code>{entry.Command}</code></blockquote>";
            }
        }
    }
}