using Run8ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using HarmonyLib;
using Run8ModLoader.CorePatches;
using Run8ModAPI.GameAccess;

namespace Run8ModLoader
{
    public static class ModLoader
    {
        public static readonly string SUPPORTED_GAME_VERSION = "Update23 Dec.04.2025";

        private static string gameDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string logPath = Path.Combine(gameDir, "ModLoader.log");
        private static string modsFolder = Path.Combine(gameDir, "Mods");
        private static StreamWriter _logFile = new StreamWriter(logPath, false) { AutoFlush = true };
        private static List<ModBase> _loadedMods = new List<ModBase>();
        private static IGameAccess gameAccess = new GameAccess();

        public static void Initialize()
        {
            try
            {
                Log("--- Run8 ModLoader Startup ---");
                Log($"Time: {DateTime.Now}");
                Log($"Game Directory: {gameDir}");
                string gameVersion = gameAccess.GetVersionString();
                Log($"Game Version: {gameVersion}");
                Log($"API Version: {typeof(ModBase).Assembly.GetName().Version}");

                if (gameVersion != SUPPORTED_GAME_VERSION)
                {
                    Log("CRITICAL ERROR: The game version you are running is not supported by this version of the mod loader !!!");
                    Log($"!!! Supported version: {SUPPORTED_GAME_VERSION} !!!");
                    return;
                }

                // core mods
                Log("Registering core patches...");
                var harmony = new Harmony("me.puyodead1.run8.core");
                harmony.PatchAll();

                VersionPatch.ApplyPatch();


                if (!Directory.Exists(modsFolder))
                {
                    Directory.CreateDirectory(modsFolder);
                    Log($"Created Mods folder: {modsFolder}");
                    return;
                }

                var modFolders = Directory.GetDirectories(modsFolder);
                Log($"Found {modFolders.Length} mod folder(s)");

                if (modFolders.Length == 0)
                {
                    Log("No mods to load");
                    return;
                }

                var modInfos = new List<(ModInfo info, string dllPath)>();

                foreach (var modFolder in modFolders)
                {
                    try
                    {
                        var modName = Path.GetFileName(modFolder);
                        var modInfoPath = Path.Combine(modFolder, "mod.json");

                        if (!File.Exists(modInfoPath))
                        {
                            Log($"  Warning: {modName} has no mod.json, skipping");
                            continue;
                        }

                        var modInfoJson = File.ReadAllText(modInfoPath);
                        var modInfo = JsonConvert.DeserializeObject<ModInfo>(modInfoJson);
                        modInfo.Directory = modFolder;

                        // version check
                        if (modInfo.SupportedGameVersion != SUPPORTED_GAME_VERSION)
                        {
                            Log($"  ERROR: {modName} does not support this game version! Supported Version: {modInfo.SupportedGameVersion}, skipping");
                            continue;
                        }

                        var dllPath = Path.Combine(modFolder, modInfo.Id + ".dll");
                        if (!File.Exists(dllPath))
                        {
                            var dlls = Directory.GetFiles(modFolder, "*.dll");
                            if (dlls.Length > 0)
                            {
                                dllPath = dlls[0];
                            }
                            else
                            {
                                Log($"  Error: No DLL found for {modName}");
                                continue;
                            }
                        }

                        modInfos.Add((modInfo, dllPath));
                        Log($"  Found: {modInfo.Name} v{modInfo.Version} by {modInfo.Author}");
                    }
                    catch (Exception ex)
                    {
                        Log($"  Error reading mod info: {ex.Message}");
                    }
                }

                var sortedMods = SortByDependencies(modInfos);

                Log("Loading mods...");
                int loadedCount = 0;

                foreach (var (info, dllPath) in sortedMods)
                {
                    try
                    {
                        Log($"[{info.Id}] Loading...");

                        foreach (var dep in info.Dependencies)
                        {
                            if (!_loadedMods.Any(m => m.Info.Id == dep))
                            {
                                Log($"[{info.Id}] Missing dependency: {dep}");
                                throw new Exception($"Missing dependency: {dep}");
                            }
                        }

                        var modAssembly = Assembly.LoadFrom(dllPath);

                        var modType = modAssembly.GetTypes()
                            .FirstOrDefault(t => t.IsSubclassOf(typeof(ModBase)) && !t.IsAbstract);

                        if (modType == null)
                        {
                            Log($"[{info.Id}] No ModBase subclass found!");
                            continue;
                        }

                        var mod = (ModBase)Activator.CreateInstance(modType);

                        ModLoadContext.Initialize(mod, info);

                        mod.OnLoad();

                        _loadedMods.Add(mod);
                        loadedCount++;

                        Log($"[{info.Id}] Loaded successfully");
                    }
                    catch (Exception ex)
                    {
                        Log($"[{info.Id}] Load failed: {ex}");

                        File.WriteAllText(
                            Path.Combine(info.Directory, "error.log"),
                            ex.ToString());
                    }
                }

                Log($"Loaded {loadedCount}/{sortedMods.Count} mods");

                Log("Triggering OnGameStart...");
                foreach (var mod in _loadedMods)
                {
                    try
                    {
                        mod.OnGameStart();
                    }
                    catch (Exception ex)
                    {
                        Log($"[{mod.Info.Id}] OnGameStart failed: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"CRITICAL ERROR: {ex}");
            }
        }

        private static List<(ModInfo, string)> SortByDependencies(List<(ModInfo info, string dllPath)> mods)
        {
            var sorted = new List<(ModInfo, string)>();
            var remaining = new List<(ModInfo, string)>(mods);

            while (remaining.Count > 0)
            {
                var canLoad = remaining.Where(m =>
                    m.Item1.Dependencies.All(dep => sorted.Any(s => s.Item1.Id == dep))
                ).ToList();

                if (canLoad.Count == 0)
                {
                    Log("Warning: Circular or missing dependencies detected, loading remainder in arbitrary order");
                    sorted.AddRange(remaining);
                    break;
                }

                sorted.AddRange(canLoad);
                remaining.RemoveAll(m => canLoad.Contains(m));
            }

            return sorted;
        }

        public static void Log(string message)
        {
            _logFile?.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}