using Run8ModAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Run8ModLoader
{
    public static class ModLoader
    {
        private static StreamWriter _logFile;
        private static List<ModBase> _loadedMods = new List<ModBase>();

        public static void Initialize()
        {
            try
            {
                var gameDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var logPath = Path.Combine(gameDir, "ModLoader.log");
                _logFile = new StreamWriter(logPath, false) { AutoFlush = true };

                Log("--- Run8 ModLoader Startup ---");
                Log($"Time: {DateTime.Now}");
                Log($"Game Directory: {gameDir}");
                Log($"API Version: {typeof(ModBase).Assembly.GetName().Version}");

                var modsFolder = Path.Combine(gameDir, "Mods");
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

                Log("\nLoading mods...");
                int loadedCount = 0;

                foreach (var (info, dllPath) in sortedMods)
                {
                    try
                    {
                        Log($"\n[{info.Id}] Loading...");

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

                        Log($"[{info.Id}] ✓ Loaded successfully");
                    }
                    catch (Exception ex)
                    {
                        Log($"[{info.Id}] ✗ Load failed: {ex}");

                        File.WriteAllText(
                            Path.Combine(info.Directory, "error.log"),
                            ex.ToString());
                    }
                }

                Log($"\nLoaded {loadedCount}/{sortedMods.Count} mods");

                Log("\nTriggering OnGameStart...");
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

        private static void Log(string message)
        {
            _logFile?.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}