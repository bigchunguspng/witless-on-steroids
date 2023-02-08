using System;
using MediaToolkit.Util;
using BanList = System.Collections.Generic.Dictionary<long, System.DateTime>;

namespace Witlesss
{
    public class BanHammer
    {
        private readonly FileIO<BanList> BansIO;
        private readonly BanList    BannedChats;


        public BanHammer(Bot bot)
        {
            Bot = bot;

            BansIO =  new FileIO<BanList>($@"{DBS_FOLDER}\bans.json");
            BannedChats = BansIO.LoadData();
        }

        private Bot Bot { get; }

        public void BanChat(long chat, int hours = 16)
        {
            BannedChats.TryAdd(chat, DateTime.Now + TimeSpan.FromHours(hours));
            BakaFrom(chat).Banned = true;
            SaveBanList();
            Log($"{chat} >> BANNED", ConsoleColor.Magenta);
        }

        public void UnbanChat(long chat)
        {
            var s = BannedChats.Remove(chat);
            BakaFrom(chat).Banned = false;
            SaveBanList();
            Log($"{chat} >> {(s ? "UNBANNED" : "WAS NOT BANNED")}", ConsoleColor.Magenta);
        }
        private void SaveBanList() => BansIO.SaveData(BannedChats);
        
        private Witless BakaFrom(long chat) => Bot.SussyBakas[chat];


        public  void GiveBans() => BannedChats.Keys.ForEach(chat => BakaFrom(chat).Banned = true);

        public  void PullBanStatus (long chat) => BakaFrom(chat).Banned = BannedChats.ContainsKey(chat);
        public  bool ChatIsBanned  (Witless w) => CheckBan(w.Chat, w.Banned);
        public  bool ChatIsBanned  (long chat) => CheckBan(chat, BannedChats.ContainsKey(chat));

        private bool CheckBan  (long chat, bool banned) => banned && !BanIsOver(chat);
        private bool BanIsOver (long chat)
        {
            var date = BannedChats[chat];
            var o = DateTime.Now > date;
            if (o) UnbanChat(chat);
            else Bot.SendMessage(chat, $"💀РАЗБАН ЧЕРЕЗ {HoursLeft(date)} (чч:мм)");
            return o;
        }

        private string HoursLeft(DateTime date)
        {
            var time = date - DateTime.Now;
            var hours = time.Hours + time.Days * 24;

            return $"{hours}:{time.Minutes}";
        }
    }
}