using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.InputFiles;

#pragma warning disable SYSLIB0014

namespace Witlesss.Commands
{
    public class RandomMeme : Command
    {
        private readonly Regex _url = new(@"""url"":""\S+?"""), _title = new(@"""title"":"".+?""");

        private readonly string[] subreddits =
        {
            "comedynecrophilia", "okbuddybaka", "comedycemetery", "okbuddyretard",
            "dankmemes", "memes", "funnymemes","doodoofard", "21stcenturyhumour",
            "breakingbadmemes", "minecraftmemes"
        };

        public override void Run()
        {
            var arg = Text.Split()[0].Split('_', 2);
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

            var path = DownloadMeme(image);
            
            using var stream = File.OpenRead(path);
            Bot.SendPhoto(Chat, new InputOnlineFile(stream), title);
            Log($"{Title} >> r/{sub}");
        }

        private string RandomSub => subreddits[Random.Next(subreddits.Length)];

        private string GetApiResponse(string url)
        {
            using var client = new HttpClient();
            using var response = client.GetAsync(url).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        private string GetImage(string s) => _url  .Match(s).Value[7..^1];
        private string GetTitle(string s) => _title.Match(s).Value[9..^1];

        private string DownloadMeme(string url)
        {
            using var client = new WebClient();
            var name = UniquePath($@"Memes\{url[18..^4]}.png");
            client.DownloadFile(url, name);

            return name;
        }
    }
}