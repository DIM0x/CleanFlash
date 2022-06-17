using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace CleanFlashUninstaller {
    static class Program {
        [Flags]
        internal enum MoveFileFlags {
            None = 0,
            ReplaceExisting = 1,
            CopyAllowed = 2,
            DelayUntilReboot = 4,
            WriteThrough = 8,
            CreateHardlink = 16,
            FailIfNotTrackable = 32,
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool MoveFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            MoveFileFlags dwFlags);

        static bool DeleteOnReboot(string filename) {
            return MoveFileEx(filename, null, MoveFileFlags.DelayUntilReboot);
        }

        static string TrimPath(string path) {
            return path.TrimEnd(new[] { '/', '\\' }); 
        }

        static bool EnsureRunInTemp() {
            string tempFolder = TrimPath(Path.GetTempPath());
            string currentPath = Application.ExecutablePath;
            string currentFolder = TrimPath(Path.GetDirectoryName(currentPath));
                
            if (currentFolder.Equals(tempFolder, StringComparison.OrdinalIgnoreCase)) {
                // Already running in the temp directory.
                return true;
            }

            string currentExeName = Path.GetFileName(currentPath);
            string newPath = Path.Combine(tempFolder, currentExeName);

            if (File.Exists(newPath)) {
                try {
                    // Attempt to delete the old version of the uninstaller.
                    File.Delete(newPath);
                } catch {
                    // Uninstaller is already running.
                    Application.Exit();
                    return false;
                }
            }

            // Copy the new file and mark it as delete-on-reboot
            File.Copy(currentPath, newPath);
            DeleteOnReboot(newPath);

            // Start the new process and end the old one
            Process.Start(newPath);
            Application.Exit();
            return false;
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            if (!EnsureRunInTemp()) {
                return;
            }

            if (Environment.OSVersion.Version.Major >= 6) {
                //SetProcessDPIAware();
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UninstallForm());
        }

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
