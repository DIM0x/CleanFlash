using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CleanFlashCommon {
    public class ProcessUtils {

        class Native {
            internal enum ModuleFilter {
                ListModulesDefault = 0x0,
                ListModules32Bit = 0x01,
                ListModules64Bit = 0x02,
                ListModulesAll = 0x03,
            }

            [DllImport("psapi.dll")]
            public static extern bool EnumProcessModulesEx(IntPtr hProcess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)][In][Out] IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded, uint dwFilterFlag);

            [DllImport("psapi.dll")]
            public static extern bool EnumProcessModules(IntPtr hProcess, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)][In][Out] IntPtr[] lphModule, int cb, [MarshalAs(UnmanagedType.U4)] out int lpcbNeeded);

            [DllImport("psapi.dll")]
            public static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In][MarshalAs(UnmanagedType.U4)] uint nSize);
        }

        public static List<string> CollectModules(Process process) {
            List<string> collectedModules = new List<string>();
            bool ex = true;

            IntPtr[] modulePointers = new IntPtr[0];
            int bytesNeeded;

            // Determine number of modules
            try {
                if (!Native.EnumProcessModulesEx(process.Handle, modulePointers, 0, out bytesNeeded, (uint)Native.ModuleFilter.ListModulesAll)) {
                    return collectedModules;
                }
            } catch (EntryPointNotFoundException) {
                if (!Native.EnumProcessModules(process.Handle, modulePointers, 0, out bytesNeeded)) {
                    return collectedModules;
                }

                ex = false;
            } catch {
                return collectedModules;
            }

            int totalModules = bytesNeeded / IntPtr.Size;
            modulePointers = new IntPtr[totalModules];

            // Collect modules from the process
            if ((ex && !Native.EnumProcessModulesEx(process.Handle, modulePointers, bytesNeeded, out bytesNeeded, (uint) Native.ModuleFilter.ListModulesAll)) || (!ex && !Native.EnumProcessModules(process.Handle, modulePointers, bytesNeeded, out bytesNeeded))) {
                return collectedModules;
            }

            for (int i = 0; i < totalModules; ++i) {
                StringBuilder moduleFilePath = new StringBuilder(1024);
                Native.GetModuleFileNameEx(process.Handle, modulePointers[i], moduleFilePath, (uint) moduleFilePath.Capacity);

                string moduleName = Path.GetFileName(moduleFilePath.ToString());
                collectedModules.Add(moduleName);
            }

            return collectedModules;
        }
        
        public static ExitedProcess RunProcess(ProcessStartInfo startInfo)
        {
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder outputBuilder = new StringBuilder();
            Process process = new Process
            {
                StartInfo = startInfo
            };
            DataReceivedEventHandler outputHandler = new DataReceivedEventHandler(
                delegate (object sender, DataReceivedEventArgs e) {
                    outputBuilder.AppendLine(e.Data);
                }
            );

            process.OutputDataReceived += outputHandler;
            process.ErrorDataReceived += outputHandler;

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.CancelOutputRead();
            process.CancelErrorRead();

            return new ExitedProcess
            {
                ExitCode = process.ExitCode,
                Output = outputBuilder.ToString().Trim()
            };
        }

        public static Process RunUnmanagedProcess(ProcessStartInfo startInfo)
        {
            Process process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
            process.WaitForExit();
            return process;
        }

    }
}
