using Run8ModAPI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CustomRoutes
{
    public class ModEntry : ModBase
    {
        private Config _config;

        public override void OnLoad()
        {
            Logger.Info("Custom Routes Mod loading...");

            _config = Config.GetConfig<Config>();
            Logger.Info($"Config loaded: {_config.Routes.Count} routes, {_config.CustomRegions.Count} regions");

            Logger.Info("Waiting for game assembly...");
            if (!Game.WaitForGameAssembly())
            {
                Logger.Error("Failed to load game assembly!");
                return;
            }

            Logger.Info($"Game assembly loaded: {Game.GameAssembly.GetName().Name}");

            PatchRoutes();
        }

        private void PatchRoutes()
        {
            try
            {
                Logger.Info("Patching routes...");

                var routeClass = Game.GetType("ns0", "Class896");

                if (routeClass == null)
                {
                    Logger.Error("Class896 not found!");
                    return;
                }

                Logger.Info("Found Class896");

                // Get the dictionaries
                var routePrefixDict = routeClass.GetProperty("RoutePrefixDictionary",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null) as Dictionary<int, string>;

                var regionDict = routeClass.GetProperty("regionFromRoutePrefixDictionary",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null) as Dictionary<int, string>;

                var regionsList = routeClass.GetField("list_3",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                    .GetValue(null) as List<string>;

                if (routePrefixDict == null || regionDict == null || regionsList == null)
                {
                    Logger.Error("Failed to get route dictionaries!");
                    return;
                }

                // Add custom regions
                foreach (var region in _config.CustomRegions)
                {
                    if (!regionsList.Contains(region))
                    {
                        regionsList.Add(region);
                        Logger.Info($"Added region: {region}");
                    }
                }

                // Add custom routes
                foreach (var route in _config.Routes)
                {
                    routePrefixDict[route.Id] = route.Name;
                    regionDict[route.Id] = route.Region;
                    Logger.Info($"Added route {route.Id}: {route.Name} ({route.Region})");
                }

                Logger.Info($"Routes patched! Total routes: {routePrefixDict.Count}");
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to patch routes", ex);
            }
        }

        public override void OnGameStart()
        {
            Logger.Info("Game started!");
        }
    }
}