using HarmonyLib;
using Run8ModAPI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Dash9LightTweaks
{
    public class ModEntry : ModBase
    {
        private Config _config;

        public override void OnLoad()
        {
            Logger.Info("Dash9LightTweaks loading...");

            //_config = Config.GetConfig<Config>();
            //Logger.Info($"Config loaded");

            Logger.Info("Waiting for game assembly...");
            if (!Game.WaitForGameAssembly())
            {
                Logger.Error("Failed to load game assembly!");
                return;
            }

            Logger.Info($"Game assembly loaded: {Game.GameAssembly.GetName().Name}");

            var harmony = new Harmony("me.puyodead1.dash9tweaks");

            new C44Dash9WLightsPatch(harmony, Logger);
        }

        public override void OnGameStart()
        {
            Logger.Info("Game started!");
        }
    }
}