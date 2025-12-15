using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

namespace Run8Patcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Run8 ModLoader Patcher\n");
            if (args.Length == 0)
            {
                Console.WriteLine("No game directory specified, using default:");
                Console.WriteLine(@"C:\Program Files\Run8Studios\Run8 Train Simulator V3\n");
            }

            string gameDir = args.Length > 0 ? args[0] : @"C:\Program Files\Run8Studios\Run8 Train Simulator V3";
            string exePath = Path.Combine(gameDir, "Run-8 Train Simulator V3.exe");
            string backupPath = exePath + ".original";

            if (!File.Exists(exePath))
            {
                Console.WriteLine($"Game executable not found: {exePath}");
                Console.WriteLine("\nUsage: Run8Patcher.exe \"path\\to\\game\\folder\"");
                Console.ReadKey();
                return;
            }


            string tempPath = exePath + ".tmp";

            using (var assembly = AssemblyDefinition.ReadAssembly(exePath))
            {
                var mainMethod = assembly.MainModule.EntryPoint;

                bool isPatched = mainMethod.Body.Instructions.Any(i =>
                 i.OpCode == OpCodes.Call &&
                 i.Operand.ToString().Contains("ModLoader"));
                if (isPatched)
                {
                    Console.WriteLine("Game is already patched!");
                    return;
                }

                Console.WriteLine("\nPatching game executable...");

                if (!File.Exists(backupPath))
                {
                    Console.WriteLine("Creating backup...");
                    File.Copy(exePath, backupPath);
                    Console.WriteLine($"Backup created: {Path.GetFileName(backupPath)}");
                }
                else
                {
                    Console.WriteLine("Backup already exists");
                }

                Console.WriteLine($"Entry point: {mainMethod.DeclaringType.Name}.{mainMethod.Name}");

                var il = mainMethod.Body.GetILProcessor();
                var firstInstruction = mainMethod.Body.Instructions[0];

                var appDomainType = assembly.MainModule.ImportReference(typeof(AppDomain));
                var getCurrentDomain = assembly.MainModule.ImportReference(
                    typeof(AppDomain).GetProperty("CurrentDomain").GetMethod);
                var addAssemblyResolve = assembly.MainModule.ImportReference(
                    typeof(AppDomain).GetEvent("AssemblyResolve").AddMethod);
                var resolveEventHandlerType = assembly.MainModule.ImportReference(typeof(ResolveEventHandler));

                var pathType = assembly.MainModule.ImportReference(typeof(System.IO.Path));
                var getCombine = assembly.MainModule.ImportReference(
                    typeof(System.IO.Path).GetMethod("Combine", new[] { typeof(string), typeof(string) }));
                var asmType = assembly.MainModule.ImportReference(typeof(System.Reflection.Assembly));
                var getExecutingAsm = assembly.MainModule.ImportReference(
                    typeof(System.Reflection.Assembly).GetMethod("GetExecutingAssembly"));
                var getLocation = assembly.MainModule.ImportReference(
                    typeof(System.Reflection.Assembly).GetProperty("Location").GetMethod);
                var getDirName = assembly.MainModule.ImportReference(
                    typeof(System.IO.Path).GetMethod("GetDirectoryName", new[] { typeof(string) }));
                var loadFrom = assembly.MainModule.ImportReference(
                    typeof(System.Reflection.Assembly).GetMethod("LoadFrom", new[] { typeof(string) }));
                var getType = assembly.MainModule.ImportReference(
                    typeof(System.Reflection.Assembly).GetMethod("GetType", new[] { typeof(string) }));
                var getMethod = assembly.MainModule.ImportReference(
                    typeof(System.Type).GetMethod("GetMethod", new[] { typeof(string), typeof(System.Reflection.BindingFlags) }));
                var invoke = assembly.MainModule.ImportReference(
                    typeof(System.Reflection.MethodInfo).GetMethod("Invoke", new[] { typeof(object), typeof(object[]) }));
                var bindingFlags = assembly.MainModule.ImportReference(typeof(System.Reflection.BindingFlags));

                // string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Call, getExecutingAsm));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Callvirt, getLocation));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Call, getDirName));

                // string loaderPath = Path.Combine(dir, "Run8ModLoader.dll")
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldstr, "Run8ModLoader.dll"));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Call, getCombine));

                // Assembly loader = Assembly.LoadFrom(loaderPath)
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Call, loadFrom));

                // Type loaderType = loader.GetType("Run8ModLoader.ModLoader")
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldstr, "Run8ModLoader.ModLoader"));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Callvirt, getType));

                // MethodInfo init = loaderType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static)
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldstr, "Initialize"));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldc_I4, (int)(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Callvirt, getMethod));

                // init.Invoke(null, null)
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldnull));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Ldnull));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Callvirt, invoke));
                il.InsertBefore(firstInstruction, il.Create(OpCodes.Pop)); // Discard return value

                Console.WriteLine("Injected ModLoader call");

                assembly.Write(tempPath);
            }

            File.Delete(exePath);
            File.Move(tempPath, exePath);

            Console.WriteLine("\nPatch successful!");
        }
    }
}