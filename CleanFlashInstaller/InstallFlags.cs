﻿using System;

namespace CleanFlashInstaller {
    public class InstallFlags {
        public static int NONE = 0;
        public static int PEPPER = 1 << 0;
        public static int NETSCAPE = 1 << 1;
        public static int ACTIVEX = 1 << 2;
        public static int PLAYER = 1 << 3;
        public static int PLAYER_START_MENU = 1 << 4;
        public static int PLAYER_DESKTOP = 1 << 5;
        public static int X64 = 1 << 6;
        public static int DEBUG = 1 << 7;

        private static int UNINSTALL_TICKS = 9;
        private static int INSTALL_GENERAL_TICKS = 5;

        private int value = 0;

        public InstallFlags(int value) {
            this.value = value;
        }

        public InstallFlags() : this(0) { }

        public int GetValue() {
            return value;
        }

        public bool IsSet(int flag) {
            return (value & flag) == flag;
        }

        public bool IsSet(InstallFlags flags) {
            return IsSet(flags.GetValue());
        }

        public bool IsNoneSet() {
            return value == 0;
        }

        public void SetFlag(int flag) {
            value |= flag;
        }

        public void SetConditionally(bool set, int flag) {
            if (set) {
                SetFlag(flag);
            }
        }

        public int GetTicks() {
            int ticks = (IsSet(PEPPER) ? 1 : 0) + (IsSet(NETSCAPE) ? 1 : 0) + (IsSet(ACTIVEX) ? 2 : 0);

            if (Environment.Is64BitOperatingSystem) {
                ticks *= 2;
            }

            if (IsSet(PLAYER)) {
                ticks++;
            }

            ticks += UNINSTALL_TICKS;
            ticks += INSTALL_GENERAL_TICKS;
            return ticks;
        }
    }
}
