using System.Text.Json;
using System.Text.Json.Serialization;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Messages;
using PF_Bot.Routing.Messages.Commands;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Types;

namespace PF_Bot.Core;

public static class Unluckies
{
    private static readonly FileLogger_Simple _errorLogger = new (File_Errors);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = NewtonsoftJsonCompatibleEncoder.Encoder,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        Converters =
        {
            new  CommandContextJsonConverter(),
            new CallbackContextJsonConverter(),
            new      JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower),
        },
    };

    /// Logs exception and its context to <see cref="Paths.File_Errors"/>.
    public static void Handle(Exception e, object? context, string title)
    {
        LogError($"{title} >> BRUH | {e.GetErrorMessage()}");
        try
        {
            var json = JsonSerializer.Serialize(context, JsonOptions);
            var entry =
                $"""
                 # {DateTime.Now:MMM' 'dd', 'HH:mm:ss.fff}

                 ## Error
                 ```c#
                 {e}
                 ```
                 ## Context
                 ```json
                 {json}
                 ```


                 """;
            _errorLogger.Log(entry);
        }
        catch
        {
            //
        }
    }

    /// Creates report, saves it to <see cref="Paths.Dir_Reports"/> and sends it to the context origin. 
    public static void HandleProcessException(ProcessException e, MessageContext context)
    {
        LogError($"{context.Title} >> PROCESS FAILED | {e.File} / {e.Result.ExitCode}");
        var path = Dir_Reports
            .EnsureDirectoryExist()
            .Combine($"{DateTime.Now:yyyy-MM-dd}-{e.File}-{context.Chat}-{Desert.GetSilt(11)}.txt")
            .MakeUnique();

        var output = "*пусто*";
        var sbOutput = e.Result.Output;
        if (sbOutput.Length > 0)
        {
            if (e.Result.WasKilled)
                sbOutput.Append("\n[I KILLED THE PROCESS :3]");

            output = sbOutput.ToString();
        }

        var text = FF_ERROR_REPORT.Format(e.File, e.Result.Arguments, GetRandomASCII(), output);
        File.WriteAllText(path, text);
        using var stream = new MemoryStream(text.GetBytes_UTF8());
        App.Bot.SendDocument(context.Origin, InputFile.FromStream(stream, "произошла ашыпка.txt"));
    }
}