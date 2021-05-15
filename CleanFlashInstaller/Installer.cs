using CleanFlashCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Ionic.Zip;

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

        public static void Install(IProgressForm form, bool installPP, bool installNP, bool installOCX) {
            if (!installPP && !installNP && !installOCX) {
                // No packages should be installed.
                return;
            }

            string flash32Path = SystemInfo.GetFlash32Path();
            string flash64Path = SystemInfo.GetFlash64Path();
            string system32Path = SystemInfo.GetSystem32Path();
            List<string> registryToApply = new List<string>() { Properties.Resources.installGeneral };

            if (Environment.Is64BitOperatingSystem) {
                registryToApply.Add(Properties.Resources.installGeneral64);
            }

            form.UpdateProgressLabel("Installing Flash Player utilities...", true);
            ExtractArchive("flash_gen_32.zip", system32Path);
            form.UpdateProgressLabel("Extracting uninstaller..", true);
            ExtractArchive("flash_uninstaller.zip", flash32Path);

            if (installPP) {
                form.UpdateProgressLabel("Installing 32-bit Flash Player for Chrome...", true);
                ExtractArchive("flash_pp_32.zip", flash32Path);
                registryToApply.Add(Properties.Resources.installPP);
            }
            if (installNP) {
                form.UpdateProgressLabel("Installing 32-bit Flash Player for Firefox...", true);
                ExtractArchive("flash_np_32.zip", flash32Path);
                registryToApply.Add(Properties.Resources.installNP);
            }
            if (installOCX) {
                form.UpdateProgressLabel("Installing 32-bit Flash Player for Internet Explorer...", true);
                ExtractArchive("flash_ocx_32.zip", flash32Path);
                registryToApply.Add(Properties.Resources.installOCX);
            }

            if (Environment.Is64BitOperatingSystem) {
                if (installPP) {
                    form.UpdateProgressLabel("Installing 64-bit Flash Player for Chrome...", true);
                    ExtractArchive("flash_pp_64.zip", flash64Path);
                    registryToApply.Add(Properties.Resources.installPP64);
                }
                if (installNP) {
                    form.UpdateProgressLabel("Installing 64-bit Flash Player for Firefox...", true);
                    ExtractArchive("flash_np_64.zip", flash64Path);
                    registryToApply.Add(Properties.Resources.installNP64);
                }
                if (installOCX) {
                    form.UpdateProgressLabel("Installing 64-bit Flash Player for Internet Explorer...", true);
                    ExtractArchive("flash_ocx_64.zip", flash64Path);
                    registryToApply.Add(Properties.Resources.installOCX64);
                }
            }

            form.UpdateProgressLabel("Applying registry changes...", true);
            RegistryManager.ApplyRegistry(registryToApply);

            if (installOCX) {
                form.UpdateProgressLabel("Activating 32-bit Flash Player for Internet Explorer...", true);
                RegisterActiveX(Path.Combine(flash32Path, string.Format("Flash_{0}.ocx", SystemInfo.GetVersionPath())));

                if (Environment.Is64BitOperatingSystem) {
                    form.UpdateProgressLabel("Activating 64-bit Flash Player for Internet Explorer...", true);
                    RegisterActiveX(Path.Combine(flash64Path, string.Format("Flash_{0}.ocx", SystemInfo.GetVersionPath())));
                }
            }
        }
    }
}