// Global using directives

global using static Witlesss.Config;
global using        Witlesss.X;
global using static Witlesss.X.Extension;
global using static Witlesss.X.Logger;
global using static Witlesss.X.Strings;
global using File = System.IO.File;
global using ChatList  = Witlesss.X.SyncronizedDictionary<  long, Witlesss.Witless>;
global using WitlessDB = Witlesss.X.SyncronizedDictionary<string, Witlesss.X.SyncronizedDictionary<string, float>>;