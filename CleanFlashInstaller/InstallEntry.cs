namespace CleanFlashInstaller {
    public class InstallEntry {
        public string InstallText { get; }
        public InstallFlags RequiredFlags { get; }
        public string TargetDirectory { get; }
        public string RegistryInstructions { get; }

        public InstallEntry(string installText, int requiredFlags, string targetDirectory, string registryInstructions) {
            InstallText = installText;
            RequiredFlags = new InstallFlags(requiredFlags);
            TargetDirectory = targetDirectory;
            RegistryInstructions = registryInstructions;
        }

        public InstallEntry(string installText, int requiredFlags, string targetDirectory) : this(installText, requiredFlags, targetDirectory, null) { }
    }
}
