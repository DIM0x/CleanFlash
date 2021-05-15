using CleanFlashCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CleanFlashInstaller {
    public partial class InstallForm : Form, IProgressForm {
        private static int UNINSTALL_TICKS = 9;
        private static int INSTALL_GENERAL_TICKS = 2;
        private static string COMPLETE_INSTALL_TEXT = @"Clean Flash Player has been successfully installed!
Don't forget, Flash Player is no longer compatible with new browsers. We recommend using:
   •  Older Google Chrome ≤ 87
   •  Older Mozilla Firefox ≤ 84 or Basilisk Browser

For Flash Player updates, check out Clean Flash Player's website!";
        private static string COMPLETE_UNINSTALL_TEXT = @"
All versions of Flash Player have been successfully uninstalled.

If you ever change your mind, check out Clean Flash Player's website!";

        public InstallForm() {
            InitializeComponent();
        }

        private void CheckAgreeBox() {
            nextButton.Enabled = disclaimerBox.Checked;
        }

        private void HideAllPanels() {
            disclaimerPanel.Visible = false;
            choicePanel.Visible = false;
            beforeInstallPanel.Visible = false;
            installPanel.Visible = false;
            completePanel.Visible = false;
            failurePanel.Visible = false;
        }

        private void OpenDisclaimerPanel() {
            HideAllPanels();
            disclaimerPanel.Visible = true;
            prevButton.Text = "QUIT";
            nextButton.Text = "AGREE";
        }

        private void OpenChoicePanel() {
            HideAllPanels();
            choicePanel.Visible = true;
            prevButton.Text = "BACK";
            nextButton.Text = "NEXT";
        }

        private string JoinStringsWithAnd(List<string> strings) {
            string text = string.Join(", ", strings);
            int index = text.LastIndexOf(", ");

            if (index != -1) {
                text = text.Substring(0, index) + " and " + text.Substring(index + 2);
            }

            return text;
        }

        private void OpenBeforeInstall() {
            HideAllPanels();
            string text;

            if (pepperBox.Checked || netscapeBox.Checked || activeXBox.Checked) {
                List<string> browsers = new List<string>();

                if (pepperBox.Checked) {
                    browsers.Add("Google Chrome");
                }
                if (netscapeBox.Checked) {
                    browsers.Add("Mozilla Firefox");
                }
                if (activeXBox.Checked) {
                    browsers.Add("Internet Explorer");
                }

                text = string.Format("You are about to install Clean Flash Player.\nPlease close all browsers, including Google Chrome, Mozilla Firefox and Internet Explorer.\n\nThe installer will close all browser windows, uninstall previous versions of Flash Player and\nFlash Center, and install Flash for {0}.", JoinStringsWithAnd(browsers));
                nextButton.Text = "INSTALL";
            } else {
                text = "You are about to uninstall Clean Flash Player.\nPlease close all browsers, including Google Chrome, Mozilla Firefox and Internet Explorer.\n\nThe installer will completely remove all versions of Flash Player from this computer,\nincluding Clean Flash Player and older versions of Adobe Flash Player.";
                nextButton.Text = "UNINSTALL";
            }
            
            beforeInstallLabel.Text = text;
            beforeInstallPanel.Visible = true;
            prevButton.Text = "BACK";
        }

        private void OpenInstall() {
            HideAllPanels();
            installPanel.Visible = true;
            prevButton.Text = "BACK";
            nextButton.Text = "NEXT";
            prevButton.Enabled = false;
            nextButton.Visible = false;
            BeginInstall();
        }

        private void OpenComplete() {
            HideAllPanels();
            completePanel.Visible = true;
            prevButton.Text = "QUIT";
            prevButton.Enabled = true;

            completeLabel.Links.Clear();

            if (pepperBox.Checked || netscapeBox.Checked || activeXBox.Checked) {
                completeLabel.Text = COMPLETE_INSTALL_TEXT;
                completeLabel.Links.Add(new LinkLabel.Link(212, 16));
                completeLabel.Links.Add(new LinkLabel.Link(268, 28));
            } else {
                completeLabel.Text = COMPLETE_UNINSTALL_TEXT;
                completeLabel.Links.Add(new LinkLabel.Link(110, 28));
            }
        }

        private void OpenFailure(Exception e) {
            HideAllPanels();
            failurePanel.Visible = true;
            prevButton.Text = "QUIT";
            prevButton.Enabled = true;
            nextButton.Text = "RETRY";
            nextButton.Visible = true;
            failureBox.Text = e.ToString();
        }

        private void BeginInstall() {
            int requiredValue = (pepperBox.Checked ? 1 : 0) + (netscapeBox.Checked ? 1 : 0) + (activeXBox.Checked ? 2 : 0);

            if (Environment.Is64BitOperatingSystem) {
                requiredValue *= 2;
            }

            requiredValue += UNINSTALL_TICKS;
            requiredValue += INSTALL_GENERAL_TICKS;

            progressBar.Value = 0;
            progressBar.Maximum = requiredValue;

            new Task(new Action(() => {
                IntPtr redirection = RedirectionManager.DisableRedirection();

                try {
                    Uninstaller.Uninstall(this);
                    Installer.Install(this, pepperBox.Checked, netscapeBox.Checked, activeXBox.Checked);
                    Complete();
                } catch (Exception e) {
                    Failure(e);
                } finally {
                    RedirectionManager.EnableRedirection(redirection);
                }
            })).Start();
        }

        private void disclaimerBox_CheckedChanged(object sender, EventArgs e) {
            CheckAgreeBox();
        }

        private void disclaimerLabel_Click(object sender, EventArgs e) {
            disclaimerBox.Checked = !disclaimerBox.Checked;
        }

        private void InstallForm_Load(object sender, EventArgs e) {
            string version = UpdateChecker.GetFlashVersion();

            subtitleLabel.Text = string.Format("built from version {0} (China)", version);
            Text = string.Format("Clean Flash Player {0} Installer", version);

            OpenDisclaimerPanel();
            CheckAgreeBox();    
        }

        private void prevButton_Click(object sender, EventArgs e) {
            if (disclaimerPanel.Visible || completePanel.Visible || failurePanel.Visible) {
                Application.Exit();
            } else if (choicePanel.Visible) {
                OpenDisclaimerPanel();
            } else if (beforeInstallPanel.Visible) {
                OpenChoicePanel();
            }
        }

        private void nextButton_Click(object sender, EventArgs e) {
            if (disclaimerPanel.Visible) {
                OpenChoicePanel();
            } else if (choicePanel.Visible) {
                OpenBeforeInstall();
            } else if (beforeInstallPanel.Visible || failurePanel.Visible) {
                OpenInstall();
            }
        }

        private void pepperLabel_Click(object sender, EventArgs e) {
            pepperBox.Checked = !pepperBox.Checked;
        }

        private void netscapeLabel_Click(object sender, EventArgs e) {
            netscapeBox.Checked = !netscapeBox.Checked;
        }

        private void activeXLabel_Click(object sender, EventArgs e) {
            activeXBox.Checked = !activeXBox.Checked;
        }

        private void button1_Click(object sender, EventArgs e) {
            BeginInstall();
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
                Process.Start("https://www.basilisk-browser.org");
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
