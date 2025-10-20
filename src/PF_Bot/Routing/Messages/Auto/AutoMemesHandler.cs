using PF_Bot.Routing.Messages.Commands;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Messages.Auto;

public interface AutoMemesHandler
{
    void Automemes_PassContext(CommandContext context);

    Task ProcessPhoto(FileBase file);
    Task ProcessStick(FileBase file);
    Task ProcessVideo(FileBase file, string extension = ".mp4", bool round = false);
}