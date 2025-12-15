using System;
using System.Collections.Generic;

namespace Run8ModAPI
{
    /// <summary>
    /// Information about a mod
    /// </summary>
    public class ModInfo
    {
        /// <summary>
        /// Unique identifier for this mod (e.g. "com.yourname.customroutes")
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Author name
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Mod IDs this mod depends on
        /// </summary>
        public List<string> Dependencies { get; set; } = new List<string>();

        /// <summary>
        /// Directory where mod is located
        /// </summary>
        public string Directory { get; set; }
    }
}