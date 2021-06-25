﻿using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace CleanFlashCommon {
    public class FileUtil {

        public static void TakeOwnership(string filename) {
            FileSecurity security = new FileSecurity();

            SecurityIdentifier sid = WindowsIdentity.GetCurrent().User;
            security.SetOwner(sid);
            security.SetAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl, AccessControlType.Allow));

            File.SetAccessControl(filename, security);

            // Remove read-only attribute
            File.SetAttributes(filename, File.GetAttributes(filename) & ~FileAttributes.ReadOnly);
        }

        public static void RecursiveDelete(DirectoryInfo rootDir, DirectoryInfo baseDir, string filename) {
            if (!baseDir.FullName.StartsWith(rootDir.FullName)) {
                // Sanity check.
                return;
            }

            if (!baseDir.Exists) {
                return;
            }

            foreach (DirectoryInfo dir in baseDir.EnumerateDirectories()) {
                RecursiveDelete(rootDir, dir, filename);
            }

            foreach (FileInfo file in baseDir.GetFiles()) {
                if (!file.FullName.StartsWith(rootDir.FullName)) {
                    // Sanity check.
                    continue;
                }

                if (filename == null || file.Name.Equals(filename)) {
                    DeleteFile(file);
                }
            }
        }

        public static void DeleteFile(FileInfo file) {
            if (!file.Exists) {
                return;
            }

            try {
                file.IsReadOnly = false;
                file.Delete();
            } catch {
                for (int i = 0; i < 10; ++i) {
                    try {
                        TakeOwnership(file.FullName);
                        file.IsReadOnly = false;
                        file.Delete();
                        return;
                    } catch {
                        // Try again after sleeping.
                        Thread.Sleep(500);
                    }
                }

                HandleUtil.KillProcessesUsingFile(file.FullName);
                Thread.Sleep(500);
                file.Delete();
            }
        }

        public static void RecursiveDelete(DirectoryInfo baseDir) {
            RecursiveDelete(baseDir, baseDir, null);
        }

        public static void RecursiveDelete(string baseDir, string filename) {
            DirectoryInfo dirInfo = new DirectoryInfo(baseDir);
            RecursiveDelete(dirInfo, dirInfo, filename);
        }

        public static void RecursiveDelete(string baseDir) {
            DirectoryInfo dirInfo = new DirectoryInfo(baseDir);
            RecursiveDelete(dirInfo, dirInfo, null);
        }

        public static void WipeFolder(string baseDir) {
            DirectoryInfo dirInfo = new DirectoryInfo(baseDir);

            if (!dirInfo.Exists) {
                return;
            }

            RecursiveDelete(dirInfo);

            if (!Directory.EnumerateFileSystemEntries(dirInfo.FullName).Any()) {
                try {
                    dirInfo.Delete();
                } catch {
                    HandleUtil.KillProcessesUsingFile(dirInfo.FullName);
                    Thread.Sleep(500);

                    try {
                        dirInfo.Delete();
                    } catch {
                        // We've tried for long enough...
                    }
                }
            }
        }

        public static void DeleteFile(string file) {
            DeleteFile(new FileInfo(file));
        }
    }
}
