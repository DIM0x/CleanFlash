using System;
using System.Collections.Generic;
using System.IO;

namespace CleanFlashCommon {
    public class SystemInfo {

        private static string system32Path = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
        private static string system64Path = Environment.GetFolderPath(Environment.SpecialFolder.System);
        private static string macromed32Path = Path.Combine(system32Path, "Macromed");
        private static string macromed64Path = Path.Combine(system64Path, "Macromed");
        private static string flash32Path = Path.Combine(macromed32Path, "Flash");
        private static string flash64Path = Path.Combine(macromed64Path, "Flash");
        private static string version = UpdateChecker.GetFlashVersion();
        private static string versionPath = version.Replace(".", "_");
        private static string versionComma = version.Replace(".", ",");
        private static Dictionary<string, string> replacementStrings = new Dictionary<string, string>() {
            { "${SYSTEM_32_PATH}", system32Path.Replace(@"\", @"\\") },
            { "${SYSTEM_64_PATH}", system64Path.Replace(@"\", @"\\") },
            { "${FLASH_32_PATH}", flash32Path.Replace(@"\", @"\\") },
            { "${FLASH_64_PATH}", flash64Path.Replace(@"\", @"\\") },
            { "${VERSION}", version },
            { "${VERSION_PATH}", versionPath },
            { "${VERSION_COMMA}", versionComma }
        };

        public static string GetSystem32Path() {
            return system32Path;
        }

        public static string GetSystem64Path() {
            return system64Path;
        }
        public static string[] GetSystemPaths() {
            if (Environment.Is64BitOperatingSystem) {
                return new string[] { system32Path, system64Path };
            } else {
                return new string[] { system32Path };
            }
        }

        public static string GetMacromed32Path() {
            return macromed32Path;
        }

        public static string GetMacromed64Path() {
            return macromed64Path;
        }

        public static string[] GetMacromedPaths() {
            if (Environment.Is64BitOperatingSystem) {
                return new string[] { macromed32Path, macromed64Path };
            } else {
                return new string[] { macromed32Path };
            }
        }

        public static string GetFlash32Path() {
            return flash32Path;
        }

        public static string GetFlash64Path() {
            return flash64Path;
        }

        public static string GetVersionPath() {
            return versionPath;
        }

        public static Dictionary<string, string> GetReplacementStrings() {
            return replacementStrings;
        }

        public static string FillString(string str) {
            // Some registry values require special strings to be filled out.
            foreach (KeyValuePair<string, string> pair in replacementStrings) {
                str = str.Replace(pair.Key, pair.Value);
            }

            return str;
        }
    }
}
