using System;
using System.Linq;
using System.Reflection;

namespace Run8ModAPI.GameAccess
{
    internal class GameAccess : IGameAccess
    {
        public Assembly GameAssembly { get; private set; }

        public bool WaitForGameAssembly(int timeoutMs = 10000)
        {
            var iterations = timeoutMs / 100;

            for (int i = 0; i < iterations; i++)
            {
                GameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name.Equals("Run-8 Train Simulator V3",
                        StringComparison.OrdinalIgnoreCase));

                if (GameAssembly != null)
                    return true;

                System.Threading.Thread.Sleep(100);
            }

            return false;
        }

        public Type GetType(string typeName)
        {
            if (GameAssembly == null)
                throw new InvalidOperationException("Game assembly not loaded yet!");

            return GameAssembly.GetType(typeName);
        }

        public Type GetType(string namespace_, string typeName)
        {
            return GetType($"{namespace_}.{typeName}");
        }
    }
}