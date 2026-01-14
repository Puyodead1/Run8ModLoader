using de4dot.cui;
using de4dot.code;
using dnlib;

namespace Run8Patcher
{
    public class DeobfuscationHelper
    {
        public void DeobfuscateFile(string inputPath, string outputPath, bool silent = true, bool verbose = false)
        {
            if (silent)
            {
                Logger.Instance.CanIgnoreMessages = true;
                Logger.Instance.MaxLoggerEvent = dnlib.DotNet.LoggerEvent.Error;
            }

            if (verbose)
            {
                Logger.Instance.CanIgnoreMessages = false;
                Logger.Instance.MaxLoggerEvent = dnlib.DotNet.LoggerEvent.Verbose;
            }

            var options = new FilesDeobfuscator.Options
            {
                RenameSymbols = true,
                ControlFlowDeobfuscation = true,
                KeepObfuscatorTypes = false,
                OneFileAtATime = true
            };

            options.DeobfuscatorInfos.Add(
                new de4dot.code.deobfuscators.CryptoObfuscator.DeobfuscatorInfo()
            );

            var fileOptions = new ObfuscatedFile.Options
            {
                Filename = inputPath,
                NewFilename = outputPath,
                ControlFlowDeobfuscation = true,
                KeepObfuscatorTypes = false,
                RenamerFlags = options.RenamerFlags,
                MetadataFlags = options.MetadataFlags
            };

            var obfuscatedFile = new ObfuscatedFile(fileOptions, options.ModuleContext);
            options.Files.Add(obfuscatedFile);

            var deobfuscator = new FilesDeobfuscator(options);
            deobfuscator.DoIt();
        }
    }
}
