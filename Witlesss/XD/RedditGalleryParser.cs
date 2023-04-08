using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

#pragma warning disable SYSLIB0014

namespace Witlesss.XD
{
    public static class RedditGalleryParser
    {
        private static readonly Regex UL = new(@"<ul.*ul>"), LI = new(@"<li.*?href=""(.*?)"".*?li>");

        public static IEnumerable<InputMediaPhoto> AlbumFromGallery(PostData post)
        {
            using var client = new WebClient();
            string html = client.DownloadString(post.URL);

            var list = LI.Matches(UL.Match(html).Value);
            var pics = list.Select(DownloadedPhoto).ToList();

            var captioned = false;
            return pics.Select(UploadPhoto).Take(10);


            string DownloadedPhoto(Match match)
            {
                var url = match.Groups[1].Value.Replace("&amp;", "&");
                var name = Path.GetFileNameWithoutExtension(url);
                return DownloadPhoto(url, name);
            }

            InputMediaPhoto UploadPhoto(string file)
            {
                var cap = captioned ? null : post.Title;
                captioned = true;
                var name = Path.GetFileNameWithoutExtension(file);
                var photo = new InputMedia(File.OpenRead(file), name);
                return new InputMediaPhoto(photo) { Caption = cap };
            }
        }

        private static string DownloadPhoto(string url, string name)
        {
            using var client = new WebClient();
            var dir = $@"{PICTURES_FOLDER}\reddit";
            var path = $@"{dir}\{name}.jpg";
            Directory.CreateDirectory(dir);
            client.DownloadFile(url, path);

            return path;
        }
    }
}