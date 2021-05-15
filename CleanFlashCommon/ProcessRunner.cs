using System.Diagnostics;
using System.Text;

namespace CleanFlashCommon {
    public class ProcessRunner {

        public static ExitedProcess RunProcess(ProcessStartInfo startInfo) {
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            StringBuilder outputBuilder = new StringBuilder();
            Process process = new Process {
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

            return new ExitedProcess {
                ExitCode = process.ExitCode,
                Output = outputBuilder.ToString().Trim()
            };
        }

        public static Process RunUnmanagedProcess(ProcessStartInfo startInfo) {
            Process process = new Process {
                StartInfo = startInfo
            };
            process.Start();
            process.WaitForExit();
            return process;
        }
    }
}
