using System.Net.Http;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.InputFiles;

#pragma warning disable SYSLIB0014

namespace Witlesss.Commands
{
    public class RandomMeme : Command
    {
        private readonly Regex _url = new(@"""url"":""\S+?"""), _title = new(@"""title"":"".+?""");
        private readonly HttpClient _client = new();
        private HttpResponseMessage _response;
        private readonly StopWatch _watch = new();
        private readonly char[] separators = { ' ', '_' };

        private readonly string[] subreddits =
        {
            "comedynecrophilia", "okbuddybaka", "comedycemetery", "okbuddyretard",
            "dankmemes", "memes", "funnymemes","doodoofard", "21stcenturyhumour",
            "breakingbadmemes", "minecraftmemes", "shitposting"
        };

        public override void Run()
        {
            var arg = Text.Split(separators, 2);
            var sub = arg.Length > 1 ? arg[1] : RandomSub;
            
            var url = $"https://meme-api.com/gimme/{sub}";

            var s = GetApiResponse(url);
            if (s.Contains("\"code\":4"))
            {
                Bot.SendMessage(Chat, "💀");
                return;
            }

            var image = GetImage(s);
            var title = GetTitle(s);

            Bot.SendPhoto(Chat, new InputOnlineFile(image), title); // todo catch ex >> dl > compress > send
            _watch.Log("TO SEND PIC");
            Log($"{Title} >> r/{sub}");
        }

        private string RandomSub => subreddits[Random.Next(subreddits.Length)];

        private string GetApiResponse(string url)
        {
            _watch.Log("WITHOUT REDDIT");
            _response = _client.GetAsync(url).Result; // <-- the bottleneck
            _watch.Log("TO GET RESPONSE");
            return _response.Content.ReadAsStringAsync().Result;
        }

        private string GetImage(string s) => _url  .Match(s).Value[7..^1];
        private string GetTitle(string s) => _title.Match(s).Value[9..^1];
    }
}