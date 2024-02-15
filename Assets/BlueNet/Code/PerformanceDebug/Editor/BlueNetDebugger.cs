using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.EditorCoroutines.Editor;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System;
using BlueNet.Compression;
using static BlueNet.Compression.CompressionBase;

namespace BlueNet.Test {
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

        IEnumerator StartLogging()
        {
            PerformanceDebugger.ResetStatistics();
            Application.targetFrameRate = 60;
            highestFPS = 0;
            lowestFPS = 1000;
            isLogging = true;
            TimeElapsed = 0;

            int num = 10;
            if(SelectedTest.Substring(3) == "bots"){
                num = int.Parse(SelectedTest.Substring(0,SelectedTest.IndexOf("_")));
            }
            while (true)
            {
                LoggingThread = new Thread(() =>
                {
                    StreamWriter writer = File.CreateText(WorkingDirectory + SelectedTest + "/" + logName + ".txt");
                    UnityEngine.Debug.Log(WorkingDirectory + SelectedTest + "/" + logName + ".txt");

                    while (isLogging)
                    {
                        Thread.Sleep(1000);

                        int sentBytes = PerformanceDebugger.TotalBytesSentSinceLastCheck();
                        int recivedBytes = PerformanceDebugger.TotalBytesReadSinceLastCheck();
                        int objectUpdates = PerformanceDebugger.TotalObjectUpdatesSinceLastCheck();

                        TimeElapsed++;
                        writer.WriteLine(string.Format("{0} {1} {2} {3} {4}", sentBytes, recivedBytes, objectUpdates, currentFPS, (int)MathF.Round(BlueNetManager.Instance.ping * 1000)));
                        if (TimeElapsed == MaxlogTimeInSeconds)
                        {
                            isLogging = false;
                        }
                    }
                    writer.Close();



                });

                LoggingThread.Start();
                if (TestManager.instance)
                {
                    TestManager.instance.RpcStart(new string[] { num.ToString() });
                }
                yield return new WaitUntil(() => LoggingThread.IsAlive == false);

                if (SelectedTest.Contains("bots"))
                {
                    num += 10;
                    if (num == 60)
                    {
                        break;
                    }
                    else
                    {
                        SelectedTest = num.ToString() + SelectedTest.Substring(2);
                        isLogging = true;
                        TimeElapsed = 0;
                    }
                }
                else
                {
                    break;
                }
            }

            if (TestManager.instance)
            {
                TestManager.instance.RpcStop(null);
            }

        }
        Thread LoggingThread;
        EditorCoroutine LoggerRoutine;
        void StopLogging()
        {
            isLogging = false;
            if (LoggerRoutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(LoggerRoutine);
            }
            
        }
        private void OnInspectorUpdate()
        {
            if (isLogging)
            {
                Repaint();
            }
        }

        int highestFPS=0,lowestFPS=1000,currentFPS=0;

        const string testmessage = "ObjectRPC,0,RpcUpdateAnimations,y,1,3.046199,x,1,-1.723765|ObjectRPC,8,RpcPosition,-57.29464*2.083334*48.1177,-0.02738156*0*5|ObjectRPC,1,RpcRotation,0*315.4963*0|ObjectRPC,1,RpcPosition,52.46152*1.583334*47.32878,-2.453515*0*2.496179|ObjectRPC,1,RpcUpdateAnimations,y,1,2.496179,x,1,-2.453515|ObjectRPC,2,RpcRotation,0*290.8495*0|ObjectRPC,2,RpcPosition,9.785172*1.53819*6.473329,-3.270823*0.030854*1.245702|ObjectRPC,2,RpcUpdateAnimations,y,1,1.245702,x,1,-3.270823|ObjectRPC,10,RpcRotation,0*165.7003*0|ObjectRPC,10,RpcPosition,54.5265*1.58*-27.72256,1.539735*0*-1.276108|ObjectRPC,10,RpcUpdateAnimations,y,1,0.8084801,x,1,-0.5885234|ObjectRPC,0,RpcRotation,0*330.4977*0|ObjectRPC,0,RpcPosition,-41.96736*1.565148*-12.43289,-1.723765*0.01102426*3.046199|";
        string result = "";
        string testmessagechoice = "";
        bool swap = true;

        CompressionBase[] compressionTests = new CompressionBase[] { new StringCompressor(), new DeflateCompressor(), new GzipCompressor(), new ParallelDeflateCompressor(), new ParallelGzipCompressor(),new ParallelCompressionBase()};
        private void OnGUI()
        {
            if(GUILayout.Button("test comp"))
            {

                swap = !swap;
                if (swap)
                {
                    testmessagechoice = testmessage + testmessage + testmessage + testmessage + testmessage + testmessage + testmessage + testmessage;
                }
                else
                {
                    testmessagechoice = testmessage;
                }
                
                List<CompressionAlgorithmSpeedTest> tests = new List<CompressionAlgorithmSpeedTest>();
                foreach (CompressionBase comp in compressionTests)
                {
                    CompressionAlgorithmSpeedTest test =comp.Test(testmessagechoice);
                    tests.Add(test);
                }
                tests.Sort((a,b)=> {
                    return (int)MathF.Round(((float)a.TotalTime()*1000) - ((float)b.TotalTime() * 1000));
                });
                int i = 0;
                result = "";
                foreach (var item in tests)
                {
                    i++;
                    result += "(" + i + ") " + item.name +" comp: "+item.compTime+"ms  deComp: "+item.decompTime+"ms TotalTime: "+item.TotalTime()+"ms\n   Size Reduction: "+item.sizeReduction+ "\n";
                }

            }
            if (string.IsNullOrEmpty(result) == false)
            {
                GUILayout.Label(result, EditorStyles.boldLabel);
            }

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
                        
                        LoggerRoutine=EditorCoroutineUtility.StartCoroutine(StartLogging(), this);

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