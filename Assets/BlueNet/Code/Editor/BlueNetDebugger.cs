using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BlueNet {
    public class BlueNetDebugger : EditorWindow
    {
        [MenuItem("Window/Analysis/BlueNet_traficDebugger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(BlueNetDebugger));
        }
        string WorkingDirectory = Application.dataPath + "/BlueNet/Code/Editor/BluetoothCompressionGraph/";
        const string pythonScript = "BluetoothCompressionDataGraph.py";

        string[] Tests;
        private void OnFocus()
        {
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

                    int sentBytes = 10;
                    int recivedBytes = 10;
                    int objectUpdates = 10;

                    TimeElapsed++;
                    writer.WriteLine(string.Format("{0} {1} {2} {3}", sentBytes, recivedBytes, objectUpdates, currentFPS));
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
                    File.Create(WorkingDirectory + newName + "/Comp_Data.txt");
                    File.Create(WorkingDirectory + newName + "/Full_Size_Data.txt");
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

            if (isLogging)
            {
                GUILayout.Label("Time since logging started: " + TimeElapsed.ToString()+" / "+ MaxlogTimeInSeconds);
                GUILayout.Label("Total Bytes Sent: ");
                GUILayout.Label("Total Bytes Recived: ");
                GUILayout.Label("Total Object Updates Recived: ");

                GUILayout.Label("FPS", EditorStyles.boldLabel);
                currentFPS = (int)(1.0f / Time.smoothDeltaTime);
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