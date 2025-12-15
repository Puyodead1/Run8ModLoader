using System;
using System.Reflection;

namespace Run8ModAPI
{
    public interface IGameAccess
    {
        /// <summary>
        /// The main game assembly
        /// </summary>
        Assembly GameAssembly { get; }

        /// <summary>
        /// Get a type from the game assembly by name
        /// </summary>
        Type GetType(string typeName);

        /// <summary>
        /// Get a type by namespace and name
        /// </summary>
        Type GetType(string namespace_, string typeName);

        /// <summary>
        /// Wait for game assembly to load (blocks until loaded or timeout)
        /// </summary>
        bool WaitForGameAssembly(int timeoutMs = 10000);
    }
}