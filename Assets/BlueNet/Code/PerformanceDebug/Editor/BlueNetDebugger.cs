using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System;

namespace BlueNet {
    public class BlueNetDebugger : EditorWindow
    {
        [MenuItem("Window/Analysis/BlueNet_traficDebugger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(BlueNetDebugger));
        }
        string WorkingDirectory = Application.dataPath + "/BlueNet/Code/PerformanceDebug/Editor/BluetoothCompressionGraph/";
        const string pythonScript = "BluetoothCompressionDataGraph.py";

        BlueNet.Compression.GzipCompressor compressor = new Compression.GzipCompressor();
        string[] Tests;
        private void OnFocus()
        {
            WorkingDirectory = Application.dataPath + "/BlueNet/Code/PerformanceDebug/Editor/BluetoothCompressionGraph/";
            ScanTestDirectories();
        }

        void ScanTestDirectories()
        {
            Tests = Directory.GetDirectories(WorkingDirectory);
            for (int i = 0; i < Tests.Length; i++)
            {
                Tests[i] = Path.GetFileName(Tests[i]);
            }
            if (SelectedTest == string.Empty)
            {
                SelectedTest = Tests[0];
            }
            
        }

        Vector2 SelectionScroll;
        string newName = "";
        string SelectedTest = "";
        string logName = "";
        bool isLogging = false;
        int MaxlogTimeInSeconds=60*5;
        int TimeElapsed = 0;
        void StartLogging()
        {
            PerformanceDebugger.ResetStatistics();
            Application.targetFrameRate = 60;
            highestFPS = 0;
            lowestFPS = 1000;
            isLogging = true;
            TimeElapsed = 0;
            LoggingThread = new Thread(() => {
                StreamWriter writer=File.CreateText(WorkingDirectory + SelectedTest + "/" + logName + ".txt");
                UnityEngine.Debug.Log(WorkingDirectory + SelectedTest + "/" + logName + ".txt");
               
                while (isLogging)
                {
                    Thread.Sleep(1000);

                    int sentBytes = PerformanceDebugger.TotalBytesSentSinceLastCheck();
                    int recivedBytes = PerformanceDebugger.TotalBytesReadSinceLastCheck();
                    int objectUpdates = PerformanceDebugger.TotalObjectUpdatesSinceLastCheck();

                    TimeElapsed++;
                    writer.WriteLine(string.Format("{0} {1} {2} {3} {4}", sentBytes, recivedBytes, objectUpdates, currentFPS, (int)MathF.Round(BlueNetManager.Instance.ping * 1000) ));
                    if (TimeElapsed == MaxlogTimeInSeconds)
                    {
                        isLogging = false;
                    }
                }
                writer.Close();


            });
            LoggingThread.Start();
        }
        Thread LoggingThread;

        void StopLogging()
        {
            isLogging = false;
        }
        private void OnInspectorUpdate()
        {
            if (isLogging)
            {
                Repaint();
            }
        }

        int highestFPS=0,lowestFPS=1000,currentFPS=0;
        private void OnGUI()
        {
            GUILayout.Label("Create New Test", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            newName = GUILayout.TextField(newName);
            if (GUILayout.Button("CreateTestProject"))
            {
                if (string.IsNullOrEmpty(newName) == false && Directory.Exists(WorkingDirectory + newName) == false)
                {
                    newName = newName.ToLower().Replace(" ","_");
                    Directory.CreateDirectory(WorkingDirectory + newName);
                    newName = "";
                    ScanTestDirectories();
                    SelectedTest = newName;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Select a Test Project to edit or view", EditorStyles.boldLabel);
            SelectionScroll = GUILayout.BeginScrollView(SelectionScroll);
            GUILayout.BeginHorizontal();
            foreach (var dir in Tests)
            {
                if (GUILayout.Button(dir)) {
                    SelectedTest = dir;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Max Time Logging in minutes: " + MaxlogTimeInSeconds/60, EditorStyles.boldLabel);
            if (int.TryParse(GUILayout.TextField((MaxlogTimeInSeconds/60).ToString()), out int newvalue))
            {
                MaxlogTimeInSeconds = newvalue*60;
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Selected Test project: "+SelectedTest, EditorStyles.boldLabel);

            if (isLogging&& BlueNetManager.Instance != null)
            {
                GUILayout.Label("Time since logging started: " + TimeElapsed.ToString()+" / "+ MaxlogTimeInSeconds);
                GUILayout.Label("Total Bytes Sent: "+ PerformanceDebugger.TotalBytesSent);
                GUILayout.Label("Total Bytes Recived: "+ PerformanceDebugger.TotalBytesRead);
                GUILayout.Label("Total Object Updates Recived: "+ PerformanceDebugger.TotalObjectUpdates);
                GUILayout.Label("Ping: "+ BlueNetManager.Instance.ping * 1000+" ms");
                GUILayout.Label("FPS", EditorStyles.boldLabel);
                currentFPS = (int)(1.0f / Time.smoothDeltaTime);
                if (currentFPS > 60)
                {
                    currentFPS = 60;
                }
                GUILayout.Label("Current FPS: "+ currentFPS);

                if (currentFPS > highestFPS)
                {
                    highestFPS = currentFPS;
                }else if (currentFPS < lowestFPS)
                {
                    lowestFPS = currentFPS;
                }
                GUILayout.Label("Highest FPS: " +highestFPS);
                GUILayout.Label("Lowest FPS: " + lowestFPS);

            }


            //show options for editing test
            if (BlueNetManager.Instance != null)
            {
                if (isLogging == false)
                {
                    logName = BlueNetManager.Instance.ActiveCompressionAlgorithm.ToString();
                    if (GUILayout.Button("Start Logging with comp mode: " + logName))
                    {
                        StartLogging();
                    }
                }
                else
                {
                    if (GUILayout.Button("Stop Logging"))
                    {
                        StopLogging();
                    }
                }
            }
            else if(isLogging)
            {
                StopLogging();
            }


            if (isLogging==false&&GUILayout.Button("Show Graph")) {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = "py";
                start.Arguments = WorkingDirectory + pythonScript + " " + SelectedTest;
                start.WorkingDirectory = WorkingDirectory;

                start.UseShellExecute = true;
                Process.Start(start);
            }

        }


    }
}