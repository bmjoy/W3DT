﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using W3DT.CASC;
using W3DT.Events;
using W3DT.Runners;
using SereniaBLPLib;

namespace W3DT
{
    public partial class MapViewerWindow : Form
    {
        private Explorer explorer;
        private Dictionary<string, List<CASCFile>> maps;
        private RunnerMapBuilder runner;

        // File extraction
        private List<ExtractState> requiredFiles;
        private List<string> paths;

        private Queue<RunnerExtractItem> runnerQueue;
        private int queueThreads = 0;

        private int tileTotal = 0;
        private int tileDone = 0;

        private Bitmap image;
        private int drawOffsetX = 0;
        private int drawOffsetY = 0;
        private int lastOffsetX = 0;
        private int lastOffsetY = 0;

        private string selectedMapName;
        private RunnerMapExport exportRunner;
        private LoadingWindow loadingWindow;
        private Action exportCancelCallback;

        // Mouse input
        private int mouseStartX;
        private int mouseStartY;
        private bool isMovingMap = false;

        public MapViewerWindow()
        {
            InitializeComponent();

            maps = new Dictionary<string, List<CASCFile>>();

            requiredFiles = new List<ExtractState>();
            paths = new List<string>();
            runnerQueue = new Queue<RunnerExtractItem>();

            explorer = new Explorer(this, "^World\\Minimaps\\", null, UI_FilterTimer, null, null, new string[] { "blp" }, "MVT_N_{0}", true);
            explorer.ExploreHitCallback = OnExploreHit;
            explorer.ExploreDoneCallback = OnExploreDone;

            EventManager.MapBuildDone += OnMapBuildDone;
            EventManager.CASCLoadStart += OnCASCLoadStart;
            EventManager.FileExtractComplete += OnFileExtractComplete;
            explorer.Initialize();

            exportCancelCallback = CancelExport;
        }

        private void OnFileExploreHit(object sender, EventArgs e)
        {
            FileExploreHitArgs args = (FileExploreHitArgs)e;
            Log.Write("DEBUG EXPLORE: " + args.Entry.FullName);
        }

        private void TerminateRunners()
        {
            // Kill existing map runner if it's already going.
            if (runner != null)
                runner.Kill();

            runner = null;

            // Kill extraction runners.
            foreach (RunnerExtractItem extractRunner in runnerQueue)
                extractRunner.Kill();

            runnerQueue.Clear();
            queueThreads = 0;
        }

        private void OnExploreHit(CASCFile file)
        {
            string[] parts = file.FullName.Split(new char[] { '/', '\\' });
            string mapName = parts[2];

            if (!mapName.Equals("WMO"))
            {
                if (!maps.ContainsKey(mapName))
                {
                    UI_FileList.Nodes.Add(mapName);
                    maps.Add(mapName, new List<CASCFile>());
                }

                maps[mapName].Add(file);
                UpdateSearchState(Constants.SEARCH_STATE_SEARCHING);
            }
        }

        private void OnExploreDone()
        {
            UpdateSearchState(Constants.SEARCH_STATE_DONE);
        }

        private void UpdateSearchState(string state)
        {
            UI_FileCount_Label.Text = string.Format(Constants.MAP_SEARCH_STATE, maps.Count, state);
        }

        private void OnCASCLoadStart(object sender, EventArgs e)
        {
            Close();
        }

        private void OnMapBuildDone(object sender, EventArgs e)
        {
            UI_PreviewStatus.Hide();
            UI_TileStatus.Hide();

            drawOffsetX = lastOffsetX = 0;
            drawOffsetY = lastOffsetY = 0;

            MapBuildDoneArgs args = (MapBuildDoneArgs)e;
            image = args.Data;
            UI_Map.Invalidate();
            UI_ExportButton.Show();
        }

        private void UI_FileList_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selected = UI_FileList.SelectedNode;

            if (selected != null)
            {
                // Clean up previous excursions.
                TerminateRunners(); // Terminate runners that be running.
                requiredFiles.Clear(); // Clear required file list.
                paths.Clear(); // Clear paths cache.
                image = null; // Prevent redrawing the old map.

                UI_Map.Invalidate();
                UI_ExportButton.Hide();

                // Detatch mouse control (this shouldn't ever be an issue, really).
                isMovingMap = false;

                string mapName = selected.Text;
                UI_PreviewStatus.Text = string.Format(Constants.MAP_VIEWER_LOADING_MAP, mapName);
                UI_PreviewStatus.Show();

                foreach (CASCFile file in maps[mapName])
                {
                    string tempPath = Path.Combine(Constants.TEMP_DIRECTORY, file.FullName);
                    ExtractState state = new ExtractState(file);

                    if (!File.Exists(tempPath))
                    {
                        RunnerExtractItem extractRunner = new RunnerExtractItem(file);
                        state.TrackerID = extractRunner.runnerID;
                        state.State = false;

                        runnerQueue.Enqueue(extractRunner);
                    }
                    else
                    {
                        state.State = true;
                    }

                    requiredFiles.Add(state);
                    paths.Add(tempPath);
                }

                selectedMapName = mapName;

                if (runnerQueue.Count > 0)
                {
                    tileDone = 0;
                    tileTotal = runnerQueue.Count;

                    CheckRunnerQueue();
                }
                else
                {
                    BeginMapBuild();
                }
            }
        }

        private void CheckRunnerQueue()
        {
            for (int i = 0; i < 15; i++)
            {
                if (runnerQueue.Count > 0)
                {
                    runnerQueue.Dequeue().Begin();
                    queueThreads++;
                }
                else
                {
                    break;
                }
            }
        }

        private void BeginMapBuild()
        {
            // Minor clean-up here, probably not needed.
            requiredFiles.Clear();

            runner = new RunnerMapBuilder(paths.ToArray());
            runner.Begin();
        }

        private void OnFileExtractComplete(object sender, EventArgs e)
        {
            FileExtractCompleteArgs args = (FileExtractCompleteArgs)e;

            ExtractState state = requiredFiles.Where(s => s.TrackerID == args.RunnerID).FirstOrDefault();
            if (state != null)
            {
                // Note: We don't actually check for success here.
                // It the tile cannot be extracted, we'll just render nothing in it's place.

                state.State = true;
                queueThreads--;

                tileDone++;
                UI_TileStatus.Text = string.Format(Constants.MAP_VIEWER_TILE_STATUS, tileDone, tileTotal);
                UI_TileStatus.Show();

                if (requiredFiles.Any(s => !s.State))
                {
                    // If no threads are running, poke the next batch.
                    if (queueThreads == 0)
                        CheckRunnerQueue();
                }
                else
                {
                    // We've got all the tiles we wanted; build!
                    BeginMapBuild();
                }
            }
        }

        private void MapViewerWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Unregister events.
            EventManager.MapBuildDone -= OnMapBuildDone;
            EventManager.CASCLoadStart -= OnCASCLoadStart;
            EventManager.FileExtractComplete -= OnFileExtractComplete;

            CancelExport();

            TerminateRunners();
            explorer.Dispose();
        }

        private void MapViewerWindow_ResizeEnd(object sender, EventArgs e)
        {
            UI_Map.Invalidate();
        }

        private void UI_Map_MouseDown(object sender, MouseEventArgs e)
        {
            isMovingMap = true;
            mouseStartX = e.X;
            mouseStartY = e.Y;
        }

        private void UI_Map_MouseUp(object sender, MouseEventArgs e)
        {
            isMovingMap = false;

            lastOffsetX = drawOffsetX;
            lastOffsetY = drawOffsetY;
        }

        private void UI_Map_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMovingMap)
            {
                drawOffsetX = lastOffsetX + (e.X - mouseStartX);
                drawOffsetY = lastOffsetY + (e.Y - mouseStartY);

                // ToDo: Prevent scrolling out of bounds here.

                UI_Map.Invalidate();
            }
        }

        private void UI_Map_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(UI_Map.BackColor);

            if (image != null)
                e.Graphics.DrawImage(image, drawOffsetX, drawOffsetY);
        }

        private void UI_ExportButton_Click(object sender, EventArgs e)
        {
            // Ensure we actually have a map selected.
            if (selectedMapName == null)
            {
                UI_ExportButton.Hide();
                return;
            }

            exportRunner = new RunnerMapExport(selectedMapName);
            exportRunner.Begin();

            loadingWindow = new LoadingWindow(string.Format("Exporting {0}...", selectedMapName), "Depending on map size, this may take a while.", true, exportCancelCallback);
            loadingWindow.ShowDialog();
        }

        private void CancelExport()
        {
            if (loadingWindow != null)
            {
                if (!loadingWindow.IsDisposed && loadingWindow.Visible)
                    loadingWindow.Close();

                loadingWindow = null;
            }

            if (exportRunner != null)
            {
                exportRunner.Kill();
                exportRunner = null;
            }
        }
    }
}
