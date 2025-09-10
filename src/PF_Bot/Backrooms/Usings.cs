// Global using directives

global using System;
global using System.IO;
global using System.Linq;
global using System.Collections.Generic;
global using System.Text.RegularExpressions;
global using System.Threading.Tasks;

global using PF_Bot.Backrooms;
global using static PF_Bot.Backrooms.Extensions;
global using static PF_Bot.Core.Paths;
global using static PF_Bot.Telegram.Literals.Texts;
global using static PF_Bot.Telegram.Literals.Responses;

global using PF_Tools.Backrooms.Extensions;
global using PF_Tools.Backrooms.Types;
global using PF_Tools.Logging;
global using static PF_Tools.Backrooms.Extensions.Extensions_Time;
global using static PF_Tools.ProcessRunning.CLIs;
global using static PF_Tools.Logging.ConsoleLogger;

global using File = System.IO.File;
global using Size = SixLabors.ImageSharp.Size;
global using MessageOrigin = (long Chat, int? Thread);
