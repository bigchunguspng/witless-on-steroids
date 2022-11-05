// Global using directives

global using static Witlesss.Extension;
global using static Witlesss.Logger;
global using static Witlesss.Strings;
global using File = System.IO.File;
global using ChatList  = System.Collections.Concurrent.ConcurrentDictionary<long, Witlesss.Witless>;
global using WitlessDB = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Concurrent.ConcurrentDictionary<string, float>>;