using CleanFlashCommon;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CleanFlashUninstaller {
    public partial class UninstallForm : Form, IProgressForm {
        private static int UNINSTALL_TICKS = 9;

        public UninstallForm() {
            InitializeComponent();
        }

        private void HideAllPanels() {
            beforeInstallPanel.Visible = false;
            installPanel.Visible = false;
            completePanel.Visible = false;
            failurePanel.Visible = false;
        }

        private void OpenBeforeInstall() {
            HideAllPanels();
            beforeInstallPanel.Visible = true;
            prevButton.Enabled = true;
            nextButton.Text = "UNINSTALL";
        }

        private void OpenInstall() {
            HideAllPanels();
            installPanel.Visible = true;
            prevButton.Enabled = false;
            nextButton.Visible = false;
            BeginInstall();
        }

        private void OpenComplete() {
            HideAllPanels();
            completePanel.Visible = true;
            prevButton.Enabled = true;

            completeLabel.Links.Clear();
            completeLabel.Links.Add(new LinkLabel.Link(110, 28));
        }

        private void OpenFailure(Exception e) {
            HideAllPanels();
            failurePanel.Visible = true;
            prevButton.Enabled = true;
            nextButton.Text = "RETRY";
            nextButton.Visible = true;
            failureBox.Text = e.ToString();
        }

        private void BeginInstall() {
            progressBar.Value = 0;
            progressBar.Maximum = UNINSTALL_TICKS;

            new Task(new Action(() => {
                IntPtr redirection = RedirectionManager.DisableRedirection();

                try {
                    Uninstaller.Uninstall(this);
                    Complete();
                } catch (Exception e) {
                    Failure(e);
                } finally {
                    RedirectionManager.EnableRedirection(redirection);
                }
            })).Start();
        }

        private void UninstallForm_Load(object sender, EventArgs e) {
            string version = UpdateChecker.GetFlashVersion();

            subtitleLabel.Text = string.Format("built from version {0} (China)", version);
            Text = string.Format("Clean Flash Player {0} Uninstaller", version);

            OpenBeforeInstall();
        }

        private void prevButton_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void nextButton_Click(object sender, EventArgs e) {
            if (beforeInstallPanel.Visible || failurePanel.Visible) {
                OpenInstall();
            }
        }

        public void UpdateProgressLabel(string text, bool tick) {
            Invoke(new Action(() => {
                progressLabel.Text = text;

                if (tick) {
                    progressBar.Value++;
                }
            }));
        }

        public void TickProgress() {
            Invoke(new Action(() => {
                progressBar.Value++;
            }));
        }

        public void Complete() {
            Invoke(new Action(OpenComplete));
        }

        public void Failure(Exception e) {
            Invoke(new Action(() => OpenFailure(e)));
        }

        private void completeLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) {
            if (e.Link.Start == 212) {
                Process.Start("https://classic.waterfox.net");
            } else {
                Process.Start("https://cleanflash.github.io");
            }
        }

        private void copyErrorButton_Click(object sender, EventArgs e) {
            Clipboard.SetText(failureBox.Text);
            MessageBox.Show("Copied error message to clipboard!", "Clean Flash Installer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
