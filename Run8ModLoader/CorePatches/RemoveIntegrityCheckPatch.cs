using HarmonyLib;

namespace Run8ModLoader.CorePatches
{
    [HarmonyPatch("Class616", "smethod_1")]
    public class RemoveIntegrityCheckPatch
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
