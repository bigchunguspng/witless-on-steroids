using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Types;

namespace Witlesss.Commands
{
    public class DebugMessage : SyncCommand
    {
        protected override void Run()
        {
            if (Message.ReplyToMessage == null)
            {
                Bot.SendMessage(Chat, DEBUG_MANUAL);
                return;
            }

            var message = Message.ReplyToMessage;

            var name = $"Message-{message.Id}-{message.Chat.Id}.json";
            var path = Path.Combine(Dir_Temp, name);
            Directory.CreateDirectory(Dir_Temp);

            File.WriteAllText(path, JsonSerializer.Serialize(message, _options));
            using var stream = File.OpenRead(path);

            Bot.SendDocument(Chat, InputFile.FromStream(stream, name.Replace("--", "-")));
            Log($"{Title} >> DEBUG");
        }

        private readonly JsonSerializerOptions _options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true
        };
    }
}