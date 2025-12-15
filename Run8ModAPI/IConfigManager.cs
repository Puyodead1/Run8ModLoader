namespace Run8ModAPI
{
    public interface IConfigManager
    {
        /// <summary>
        /// Load configuration from file, or create default if not exists
        /// </summary>
        T GetConfig<T>() where T : class, new();

        /// <summary>
        /// Save configuration to file
        /// </summary>
        void SaveConfig<T>(T config) where T : class;
    }
}