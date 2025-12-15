namespace Run8ModAPI
{
    /// <summary>
    /// Base class for all Run8 mods
    /// </summary>
    public abstract class ModBase
    {
        /// <summary>
        /// Metadata about this mod
        /// </summary>
        public ModInfo Info { get; internal set; }

        /// <summary>
        /// Logger for this mod
        /// </summary>
        public ILogger Logger { get; internal set; }

        /// <summary>
        /// Configuration manager for this mod
        /// </summary>
        public IConfigManager Config { get; internal set; }

        /// <summary>
        /// Access to game assemblies and types
        /// </summary>
        public IGameAccess Game { get; internal set; }

        /// <summary>
        /// Harmony instance for patching
        /// </summary>
        public HarmonyLib.Harmony Harmony { get; internal set; }

        /// <summary>
        /// Called when the mod is loaded
        /// </summary>
        public abstract void OnLoad();

        /// <summary>
        /// Called when the mod is unloaded (if ever)
        /// </summary>
        public virtual void OnUnload() { }

        /// <summary>
        /// Called when game starts
        /// </summary>
        public virtual void OnGameStart() { }
    }
}