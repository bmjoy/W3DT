﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using W3DT.Events;
using W3DT.Runners;
using W3DT.JSONContainers;

namespace W3DT
{
    public partial class SplashScreen : Form, ISourceSelectionParent
    {
        private bool isDoneLoading = false;
        private bool isUpdateCheckDone = false;
        private bool hasShownSourceScreen = false;
        private string currentVersion = "1.0.0.0";

        public SplashScreen()
        {
            InitializeComponent();
            EventManager.H_UpdateCheckComplete += OnUpdateCheckComplete;
            EventManager.H_UpdateDownloadComplete += OnUpdateDownloadComplete;

            currentVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Debug.WriteLine("Current build: " + currentVersion);

            // Check for and remove leftover update package.
            try
            {
                if (File.Exists(Constants.UPDATE_PACKAGE_FILE))
                    File.Delete(Constants.UPDATE_PACKAGE_FILE);
            }
            catch (Exception ex)
            {
                // File locked, or something. Brazenly continue onwards.
                Debug.WriteLine("Unable to delete leftover package file (" + ex.GetType().Name + ")");
                Debug.WriteLine("Exception info: " + ex.Message);
            }

            if (Program.DO_UPDATE)
            {
                new RunnerUpdateCheck().Begin();
            }
            else if (!Program.Settings.ShowSourceSelector)
            {
                isDoneLoading = true;
                isUpdateCheckDone = true;
            }
        }

        public void OnUpdateCheckComplete(LatestReleaseData data)
        {
            bool isUpdating = false;

            // Check our update data.
            if (data.message != null)
            {
                // If a message is set, it was some kind of error.
                Debug.WriteLine("Not updating, GitHub gave us an error: " + data.message);
            }
            else
            {
                // Ensure our remote version number is not malformed.
                data.tag_name = data.tag_name.Trim(); // Trim any whitespace that might have slipped in.
                Regex versionCheck = new Regex(@"^\d+\.\d+\.\d+\.\d+$", RegexOptions.IgnoreCase);
                if (versionCheck.Match(data.tag_name).Success)
                {
                    Debug.WriteLine("Remote latest version: " + data.tag_name);

                    Version localVersion = new Version(currentVersion);
                    Version remoteVersion = new Version(data.tag_name);

                    if (remoteVersion.CompareTo(localVersion) > 0)
                    {
                        if (data.assets.Length > 0)
                        {
                            // Local version is out-of-date, lets fix that.
                            isUpdating = true;
                            new RunnerDownloadUpdate(data.assets[0].browser_download_url).Begin();
                        }
                        else
                        {
                            // Missing assets. Human error (most likely).
                            Debug.WriteLine("Not updating, remote version has no assets attached!");
                        }
                    }
                    else
                    {
                        // Local version is equal to remote (or somehow newer).
                        Debug.WriteLine("Not updating, local version is newer or equal to remote version.");
                    }
                }
                else
                {
                    // Version number is not valid, generally caused by human error.
                    Debug.WriteLine("Not updating, remote version number is malformed: " + data.tag_name);
                }
            }

            // There shouldn't be more than one event fired, but unregister anyway.
            EventManager.H_UpdateCheckComplete -= OnUpdateCheckComplete;

            if (!isUpdating)
            {
                if (!Program.Settings.ShowSourceSelector)
                    isDoneLoading = true;

                isUpdateCheckDone = true;
            }
        }

        private void ShowSourceSelectionScreen()
        {
            SourceSelectionScreen sourceScreen = new SourceSelectionScreen(this);
            sourceScreen.Show();
            sourceScreen.Focus();
        }

        public void OnSourceSelectionDone()
        {
            isDoneLoading = true;
        }

        public void OnUpdateDownloadComplete(bool success)
        {
            if (success)
            {
                Process.Start("W3DT_Updater.exe");
                Program.STOP_LOAD = true;
                Close();
            }
            else
            {
                // Mark loading as done to continue with app launch.
                isUpdateCheckDone = true;
                isDoneLoading = true;
            }

            // There shouldn't be more than one event fired, but unregister anyway.
            EventManager.H_UpdateDownloadComplete -= OnUpdateDownloadComplete;
        }

        private void Timer_SplashClose_Tick(object sender, EventArgs e)
        {
            // Speed up the timer after the first pass.
            if (Timer_SplashClose.Interval == 4000)
                Timer_SplashClose.Interval = 100;

            if (!hasShownSourceScreen && Program.Settings.ShowSourceSelector)
            {
                hasShownSourceScreen = true;
                ShowSourceSelectionScreen();
            }

            // Check if we're done loading.
            if (isDoneLoading)
            {
                Timer_SplashClose.Enabled = false; // Disable timer.
                this.Close(); // Close the splash screen.
            }
        }
    }
}
