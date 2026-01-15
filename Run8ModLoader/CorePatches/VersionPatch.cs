using HarmonyLib;
using System;

namespace Run8ModLoader.CorePatches
{
    public class VersionPatch
    {
        public static void ApplyPatch()
        {
            Type targetType = AccessTools.TypeByName("Class616");
            if (targetType != null)
            {
                var field = AccessTools.Field(targetType, "string_1");
                if (field != null)
                {
                    string originalValue = (string)field.GetValue(null);
                    field.SetValue(null, originalValue + " (Modded)");
                    ModLoader.Log($"VersionPatch: Patched to '{field.GetValue(null)}'.");
                }
                else
                {
                    ModLoader.Log("VersionPatch: Field 'string_1' not found in 'Class616'.");
                }
            }
            else
            {
                ModLoader.Log("VersionPatch: Target type 'Class616' not found.");
            }
        }
    }
}