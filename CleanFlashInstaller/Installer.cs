using CleanFlashCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Ionic.Zip;
using IWshRuntimeLibrary;

namespace CleanFlashInstaller {
    public class Installer {
        public static void RegisterActiveX(string filename) {
            Directory.SetCurrentDirectory(Path.GetDirectoryName(filename));
            ExitedProcess process = ProcessRunner.RunProcess(
                new ProcessStartInfo {
                    FileName = "regsvr32.exe",
                    Arguments = "/s " + Path.GetFileName(filename),
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            );

            if (!process.IsSuccessful) {
                throw new InstallException(string.Format("Failed to register ActiveX plugin: error code {0}\n\n{1}", process.ExitCode, process.Output));
            }
        }

        public static void ExtractArchive(string archiveName, string targetDirectory) {
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CleanFlashInstaller." + archiveName)) {
                using (ZipFile archive = ZipFile.Read(stream)) {
                    archive.ExtractAll(targetDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        public static void CreateShortcut(string folder, string executable, string name, string description) {
            WshShell wsh = new WshShell();
            IWshShortcut shortcut = wsh.CreateShortcut(Path.Combine(folder, name + ".lnk")) as IWshShortcut;

            shortcut.Arguments = "";
            shortcut.TargetPath = executable;
            shortcut.WindowStyle = (int) WshWindowStyle.WshNormalFocus;
            shortcut.Description = description;
            shortcut.WorkingDirectory = Path.GetDirectoryName(executable);
            shortcut.IconLocation = executable;
            shortcut.Save();
        }

        public static void Install(IProgressForm form, InstallFlags flags) {
            if (flags.IsNoneSet()) {
                // No packages should be installed.
                return;
            }

            string flash32Path = SystemInfo.GetFlash32Path();
            string flash64Path = SystemInfo.GetFlash64Path();
            string system32Path = SystemInfo.GetSystem32Path();
            string flashProgram32Path = SystemInfo.GetProgramFlash32Path();
            List<string> registryToApply = new List<string>() { Properties.Resources.installGeneral };

            if (Environment.Is64BitOperatingSystem) {
                registryToApply.Add(Properties.Resources.installGeneral64);
            }

            form.UpdateProgressLabel("Installing Flash Player utilities...", true);
            ExtractArchive("flash_gen_32.zip", system32Path);
            form.UpdateProgressLabel("Extracting uninstaller..", true);
            ExtractArchive("flash_uninstaller.zip", flashProgram32Path);

            if (flags.IsSet(InstallFlags.PEPPER)) {
                form.UpdateProgressLabel("Installing 32-bit Flash Player for Chrome...", true);
                ExtractArchive("flash_pp_32.zip", flash32Path);
                registryToApply.Add(Properties.Resources.installPP);
            }
            if (flags.IsSet(InstallFlags.NETSCAPE)) {
                form.UpdateProgressLabel("Installing 32-bit Flash Player for Firefox...", true);
                ExtractArchive("flash_np_32.zip", flash32Path);
                registryToApply.Add(Properties.Resources.installNP);
            }
            if (flags.IsSet(InstallFlags.ACTIVEX)) {
                form.UpdateProgressLabel("Installing 32-bit Flash Player for Internet Explorer...", true);
                ExtractArchive("flash_ocx_32.zip", flash32Path);
            }
            if (flags.IsSet(InstallFlags.PLAYER)) {
                form.UpdateProgressLabel("Installing 32-bit Standalone Flash Player...", true);
                ExtractArchive("flash_player_32.zip", flashProgram32Path);

                string name = "Flash Player";
                string description = "Standalone Flash Player " + UpdateChecker.GetFlashVersion();
                string executable = Path.Combine(flashProgram32Path, UpdateChecker.GetFlashPlayerExecutable());

                if (UpdateChecker.IsDebug()) {
                    name += " (Debug)";
                    description += " (Debug)";
                }

                if (flags.IsSet(InstallFlags.PLAYER_START_MENU)) {
                    CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), executable, name, description);
                }

                if (flags.IsSet(InstallFlags.PLAYER_DESKTOP)) {
                    CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), executable, name, description);
                }
            }

            if (Environment.Is64BitOperatingSystem) {
                if (flags.IsSet(InstallFlags.PEPPER)) {
                    form.UpdateProgressLabel("Installing 64-bit Flash Player for Chrome...", true);
                    ExtractArchive("flash_pp_64.zip", flash64Path);
                    registryToApply.Add(Properties.Resources.installPP64);
                }
                if (flags.IsSet(InstallFlags.NETSCAPE)) {
                    form.UpdateProgressLabel("Installing 64-bit Flash Player for Firefox...", true);
                    ExtractArchive("flash_np_64.zip", flash64Path);
                    registryToApply.Add(Properties.Resources.installNP64);
                }
                if (flags.IsSet(InstallFlags.ACTIVEX)) {
                    form.UpdateProgressLabel("Installing 64-bit Flash Player for Internet Explorer...", true);
                    ExtractArchive("flash_ocx_64.zip", flash64Path);
                }
            }

            form.UpdateProgressLabel("Applying registry changes...", true);
            RegistryManager.ApplyRegistry(registryToApply);

            if (flags.IsSet(InstallFlags.ACTIVEX)) {
                form.UpdateProgressLabel("Activating 32-bit Flash Player for Internet Explorer...", true);
                RegisterActiveX(Path.Combine(flash32Path, string.Format("Flash32_{0}.ocx", SystemInfo.GetVersionPath())));

                if (Environment.Is64BitOperatingSystem) {
                    form.UpdateProgressLabel("Activating 64-bit Flash Player for Internet Explorer...", true);
                    RegisterActiveX(Path.Combine(flash64Path, string.Format("Flash64_{0}.ocx", SystemInfo.GetVersionPath())));
                }
            }
        }
    }
}