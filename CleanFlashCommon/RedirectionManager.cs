using System;
using System.Runtime.InteropServices;

namespace CleanFlashCommon {
    public class RedirectionManager {
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);

        public static IntPtr DisableRedirection() {
            IntPtr redirectionPtr = (IntPtr)(-1);

            try {
                Wow64DisableWow64FsRedirection(ref redirectionPtr);
            } catch {
                // No Wow64 redirection possible.
            }

            return redirectionPtr;
        }

        public static void EnableRedirection(IntPtr redirectionPtr) {
            try {
                Wow64RevertWow64FsRedirection(redirectionPtr);
            } catch {
                // No Wow64 redirection possible.
            }
        }
    }
}
