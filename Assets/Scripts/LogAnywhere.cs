using UnityEngine;

public class LogAnywhere : MonoBehaviour
{
    string filename = "";

    void OnEnable() { Application.logMessageReceived += Log; Debug.Log("Logging started."); }
    void OnDisable() { Application.logMessageReceived -= Log; }

    public void Log(string logString, string stackTrace, LogType type)
    {
        if (filename == "")
        {
            string dirPath = Application.persistentDataPath;

            System.IO.Directory.CreateDirectory(dirPath);

            filename = dirPath + "/log.txt";
        }

        try
        {
            System.IO.File.AppendAllText(filename, logString + "\n");
        }
        catch { }
    }
}