using UnityEditor.Scripting.Python;
using UnityEditor;
using UnityEngine;
using System;

public class EnsureNaming
{
    [MenuItem("Python/Ensure Naming")]
    static void RunEnsureNaming()
    {
        //Environment.SetEnvironmentVariable("PYTHONPATH", @"C:\ProgramData\Anaconda3\", EnvironmentVariableTarget.Process);

        PythonRunner.RunFile($"{Application.dataPath}/hello_world.py");
    }
}