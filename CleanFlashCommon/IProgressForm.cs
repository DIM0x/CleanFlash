namespace CleanFlashCommon {
    public interface IProgressForm {

        void UpdateProgressLabel(string text, bool tick);
        void TickProgress();
    }
}
