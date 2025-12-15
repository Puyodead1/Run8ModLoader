using HarmonyLib;
using Run8ModAPI.Configuration;
using Run8ModAPI.Logging;
using System.IO;

namespace Run8ModAPI
{
    /// <summary>
    /// Internal helper to initialize a mod with all its dependencies
    /// </summary>
    public static class ModLoadContext
    {
        public static void Initialize(ModBase mod, ModInfo info)
        {
            mod.Info = info;

            var logPath = Path.Combine(info.Directory, $"{info.Id}.log");
            mod.Logger = new Logger(info.Name, logPath);

            mod.Config = new ConfigManager(info.Directory);

            mod.Game = new GameAccess.GameAccess();

            mod.Harmony = new Harmony(info.Id);

            mod.Logger.Info($"{info.Name} v{info.Version}");
            mod.Logger.Info($"Author: {info.Author}");
        }
    }
}