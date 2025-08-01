﻿using System.Text;
using Telegram.Bot.Types;
using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.Commands.Packing;
using Witlesss.Memes;
using Witlesss.Memes.Shared;

namespace Witlesss.Commands.Meme
{
    public class Nuke : MakeMemeCore<int>
    {
        private static readonly DukeNukem _nukem = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override SerialTaskQueue Queue { get; } = _queue;
        protected override IMemeGenerator<int> MemeMaker => _nukem;

        protected override Regex _cmd { get; } = new(@"^\/nuke(\S*)");

        protected override string VideoName => "piece_fap_bot-nuke.mp4";

        protected override string Log_STR => "NUKED";
        protected override string Command => "/nuke";
        protected override string Suffix  => "-Nuked"; // Needs more nuking!

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
            DukeNukem.Depth = OptionsParsing.GetInt(Request, _depth, 1);
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

            lock (DukeNukem.LogsLock)
            {
                if (!DukeNukem.Logs.TryGetValue(pagination.Origin.Chat, out var entries))
                {
                    Bot.SendMessage(pagination.Origin, NUKE_LOG_EXPLANATION);
                    return;
                }

                var single = entries.Count <= perPage;

                var lastPage = (int)Math.Ceiling(entries.Count / (double)perPage) - 1;
                var sb = new StringBuilder("🍤 <b>Последние вариации /nuke:</b>");
                if (!single) sb.Append($" 📃{page + 1}/{lastPage + 1}");
                sb.Append("\n\n").AppendJoin('\n', GetNukeLog(entries, page, perPage));
                sb.Append("\n\nИспользование: <code>/pegc [фильтр] .</code>");
                if (!single) sb.Append(USE_ARROWS);
                
                var buttons = single ? null : GetPaginationKeyboard(page, perPage, lastPage, "nl");
                Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
            }
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