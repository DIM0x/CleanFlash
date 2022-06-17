using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CleanFlashCommon {
    public class RegistryManager {

        public static void ApplyRegistry(string registryContents) {
            registryContents = "Windows Registry Editor Version 5.00\n\n" + SystemInfo.FillString(registryContents);
            string filename = Path.GetTempFileName();

            File.WriteAllText(filename, registryContents, Encoding.Unicode);

            Directory.SetCurrentDirectory(Path.GetDirectoryName(filename));

            ExitedProcess process = ProcessUtils.RunProcess(
                new ProcessStartInfo {
                    FileName = "reg.exe",
                    Arguments = "import " + Path.GetFileName(filename),
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            );

            File.Delete(filename);

            if (!process.IsSuccessful) {
                throw new InstallException(string.Format("Failed to apply changes to registry: error code {0}\n\n{1}", process.ExitCode, process.Output));
            }
        }

        public static void ApplyRegistry(List<string> registryContents) {
            ApplyRegistry(string.Join("\n\n", registryContents));
        }

        public static void ApplyRegistry(params string[] registryContents) {
            ApplyRegistry(string.Join("\n\n", registryContents));
        }
    }
}
