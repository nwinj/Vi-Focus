using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ApplicationBlocker
{
    public partial class Form1 : Form
    {
        private readonly List<string> _blockedApplications = new List<string>();
        private readonly List<string> _blockedWebsites = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void BlockApplicationButton_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe",
                InitialDirectory = @"C:\",
                Title = "Select an application to block"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var applicationPath = openFileDialog.FileName;
                _blockedApplications.Add(applicationPath);
                BlockedApplicationsListBox.Items.Add(applicationPath);
            }
        }

        private void BlockWebsiteButton_Click(object sender, EventArgs e)
        {
            var website = WebsiteTextBox.Text;
            if (!string.IsNullOrWhiteSpace(website))
            {
                _blockedWebsites.Add(website);
                BlockedWebsitesListBox.Items.Add(website);
            }
        }

        private void UnblockApplicationButton_Click(object sender, EventArgs e)
        {
            var selectedIndex = BlockedApplicationsListBox.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var applicationPath = BlockedApplicationsListBox.SelectedItem.ToString();
                _blockedApplications.Remove(applicationPath);
                BlockedApplicationsListBox.Items.RemoveAt(selectedIndex);
            }
        }

        private void UnblockWebsiteButton_Click(object sender, EventArgs e)
        {
            var selectedIndex = BlockedWebsitesListBox.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var website = BlockedWebsitesListBox.SelectedItem.ToString();
                _blockedWebsites.Remove(website);
                BlockedWebsitesListBox.Items.RemoveAt(selectedIndex);
            }
        }

        private void StartMonitoringButton_Click(object sender, EventArgs e)
        {
            StartMonitoringButton.Enabled = false;
            StopMonitoringButton.Enabled = true;

            var hostsFilePath = @"C:\Windows\System32\drivers\etc\hosts";

            while (StopMonitoringButton.Enabled)
            {
                var runningProcesses = Process.GetProcesses().Where(p => !p.HasExited);
                foreach (var process in runningProcesses)
                {
                    if (_blockedApplications.Contains(process.MainModule.FileName))
                    {
                        process.Kill();
                    }
                }

                var existingHostsFileLines = File.ReadAllLines(hostsFilePath);
                var filteredHostsFileL
ine = existingHostsFileLines.Where(line => !line.Contains("#") && !_blockedWebsites.Any(line.Contains));
File.WriteAllLines(hostsFilePath, filteredHostsFileLines.Concat(_blockedWebsites.Select(website => "127.0.0.1 " + website)));
            Application.DoEvents();
        }
    }

    private void StopMonitoringButton_Click(object sender, EventArgs e)
    {
        StartMonitoringButton.Enabled = true;
        StopMonitoringButton.Enabled = false;
    }
}
}
