using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using System.Globalization;
using FB2Library;
using System;

[System.Serializable]
public class FilesData
{
    public List<FileData> Files = new List<FileData>();
}

public class AppManager : MonoBehaviour
{
    [SerializeField] private string _folderName = "/Files/";
    [SerializeField] private FB2SampleConverter _fB2SampleConverter;

    private static AppManager _instance;

    public delegate void FilesListChanged();
    public FilesListChanged filesListChanged;

    public delegate void OnFB2Loaded();
    public OnFB2Loaded onFB2Loaded;

    public FilesData FilesData { get; private set; } = new FilesData();

    public static AppManager Instance { get { if (_instance == null) _instance = FindObjectOfType<AppManager>(); return _instance; } private set { } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }

        LoadFilesData();

        FileBrowser.Filter filter = new FileBrowser.Filter("fb2", ".fb2");
        FileBrowser.SetFilters(false, filter);
    }

    void Start()
    {
        onFB2Loaded += ReadFB2FileAsync;
    }

    /// <summary>
    ///  Отобразить окно импорта файла.
    /// </summary>
    public void ShowLoadDialog()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, "Load Book", "Load");

        onFB2Loaded?.Invoke();
    }

    private async void ReadFB2FileAsync()
    {
        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        // If a file was chosen, read its bytes via FileBrowserHelpers
        // Contrary to File.ReadAllBytes, this function works on Android 10+, as well

        if (FileBrowser.Success)
        {
            foreach (string path in FileBrowser.Result)
            {
                string fileName = FileBrowserHelpers.GetFilename(path);
                string text = FileBrowserHelpers.ReadTextFromFile(path);

                FB2File file = await new FB2Reader().ReadAsync(text);
                var lines = await _fB2SampleConverter.ConvertAsync(file);
                string finalText = _fB2SampleConverter.GetLinesAsText();

                byte[] imageData = _fB2SampleConverter.GetCoverImageData();

                string imagePath = Application.persistentDataPath + _folderName + fileName + ".jpg"; ;

                try
                {
                    File.WriteAllBytes(imagePath, imageData);

                    Debug.Log("Image is saved. Path: " + imagePath);
                }
                catch
                {
                    Debug.LogError("Loading image is error!");
                }

                imagePath = imagePath.Replace("/", "\\");

                string filePath = (Application.persistentDataPath + _folderName + fileName).Replace("/", "\\");

                FileData newBook = new FileData(fileName, filePath, imagePath, FileData.FileType.Book);

                SaveFile(newBook, finalText);
            }
        }
    }

    private void SaveFilesData()
    {
        string json = JsonUtility.ToJson(FilesData);
        PlayerPrefs.SetString("Files", json);
    }

    private void LoadFilesData()
    {
        if (PlayerPrefs.HasKey("Files"))
        {
            FilesData = JsonUtility.FromJson<FilesData>(PlayerPrefs.GetString("Files"));
        }
    }

    public Sprite LoadSprite(string path)
    {
        byte[] bytes = new byte[0];

        try
        {
            bytes = FileBrowserHelpers.ReadBytesFromFile(path);
        }
        catch
        {
            Debug.LogError("Loading image is error: no file exist!");
        }

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);

        Rect rec = new Rect(0, 0, tex.width, tex.height);
        Sprite sprite = Sprite.Create(tex, rec, new Vector2(0.5f, 0.5f), 100);

        return sprite;
    }

    public string LoadFile(string path)
    {
        string text = FileBrowserHelpers.ReadTextFromFile(path);

        return text;
    }

    public void RemoveFile(FileData file)
    {
        FileBrowserHelpers.DeleteFile(file.Path);

        if (file.ImagePath != string.Empty)
        {
            try
            {
                FileBrowserHelpers.DeleteFile(file.ImagePath);
            }
            catch
            {
                Debug.Log("Image not destroyed.");
            }
        }

        FilesData.Files.Remove(file);

        SaveFilesData();

        filesListChanged?.Invoke();
    }

    public void SaveFile(FileData file, string text)
    {
        if (file == null)
        {
            string name = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString().Replace(":", "_");
            string path = Application.persistentDataPath + _folderName;

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (IOException ex)
            {
                Debug.Log(ex.Message);
            }

            path = path.Replace("/", "\\");
            path += file.Name + ".txt";

            file = new FileData(name, path, String.Empty, FileData.FileType.UserFile);
        }

        try
        {
            FileBrowserHelpers.WriteTextToFile(file.Path, text);

            if (!FilesData.Files.Contains(file))
            {
                FilesData.Files.Add(file);
            }

            SaveFilesData();

            filesListChanged?.Invoke();

            Debug.Log("File is saved. Path: " + file.Path);
        }
        catch
        {
            Debug.Log("Saving file is end error.");
        }
    }
}
