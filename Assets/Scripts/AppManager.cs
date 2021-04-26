using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;
using System.IO;
using System.Globalization;
using FB2Library;
using System;

[System.Serializable]
public class BooksData
{
    public List<BookData> books = new List<BookData>();
}

[System.Serializable]
public class FilesData
{
    public List<FileData> files = new List<FileData>();
}

public class AppManager : MonoBehaviour
{
    [SerializeField] private string folderName = "/Books/";
    [SerializeField] private FB2SampleConverter fB2SampleConverter;

    public delegate void BooksListChanged();
    public BooksListChanged booksListChanged;
    public BooksListChanged filesListChanged;
    public delegate void OnFB2Loaded();
    public OnFB2Loaded onFB2Loaded;

    public BooksData booksData { get; private set; } = new BooksData();
    public FilesData filesData { get; private set; } = new FilesData();

    public int curCharIndex { get; set; }
    public string curText { get; set; }

    private void Awake()
    {
        LoadBooks();
        LoadFiles();

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
        Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        // If a file was chosen, read its bytes via FileBrowserHelpers
        // Contrary to File.ReadAllBytes, this function works on Android 10+, as well

        if (FileBrowser.Success)
        {
            string targetPath = Application.persistentDataPath + folderName;

            // create folder and save to disk
            try
            {
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
            }
            catch (IOException ex)
            {
                Debug.Log(ex.Message);
            }

            foreach (string path in FileBrowser.Result)
            {
                string fileName = FileBrowserHelpers.GetFilename(path);
                string text = FileBrowserHelpers.ReadTextFromFile(path);

                FB2File file = await new FB2Reader().ReadAsync(text);
                var lines = await fB2SampleConverter.ConvertAsync(file);
                string resultText = fB2SampleConverter.GetLinesAsText();

                BookData newBook = new BookData();
                newBook.name = fileName;
                newBook.path = (targetPath + fileName).Replace("/", "\\");

                try
                {
                    FileBrowserHelpers.WriteTextToFile(newBook.path, resultText);

                    booksData.books.Add(newBook);

                    SaveBooksData();

                    booksListChanged?.Invoke();

                    Debug.Log("File is saved. Path: " + targetPath + fileName);
                }
                catch
                {
                    Debug.Log("Saving file is end error.");
                }
            }
        }
    }

    public string LoadFile(string path)
    {
        string text = FileBrowserHelpers.ReadTextFromFile(path);

        return text;
    }

    public BookData GetBook(int index)
    {
        return booksData.books[index];
    }

    public FileData GetFile(int index)
    {
        if(index < 0)
        {
            return null;
        } else
        {
            return filesData.files[index];
        }
    }

    private void SaveBooksData()
    {
        string json = JsonUtility.ToJson(booksData);
        PlayerPrefs.SetString("Books", json);
    }

    private void SaveFilesData()
    {
        string json = JsonUtility.ToJson(filesData);
        PlayerPrefs.SetString("Files", json);
    }

    public void RemoveBook(int index)
    {
        BookData book = GetBook(index);

        FileBrowserHelpers.DeleteFile(book.path);

        booksData.books.Remove(book);

        SaveBooksData();

        booksListChanged?.Invoke();
    }

    public void RemoveFile(int index)
    {
        FileData file = GetFile(index);

        FileBrowserHelpers.DeleteFile(file.path);

        filesData.files.Remove(file);

        SaveFilesData();

        filesListChanged?.Invoke();
    }

    private void LoadBooks()
    {
        if (PlayerPrefs.HasKey("Books"))
        {
            booksData = JsonUtility.FromJson<BooksData>(PlayerPrefs.GetString("Books"));
        }
    }

    public void LoadFiles()
    {
        if (PlayerPrefs.HasKey("Files"))
        {
            filesData = JsonUtility.FromJson<FilesData>(PlayerPrefs.GetString("Files"));
        }
    }

    public void SaveFile(FileData file, string text)
    {
        if (file == null)
        {
            file = new FileData();
            file.name = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString().Replace(":", "_"); ;

            string path = Application.persistentDataPath + "/Files/";

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

            file.path = path + file.name + ".txt";
            file.path = file.path.Replace("/", "\\");
        }

        try
        {
            FileBrowserHelpers.WriteTextToFile(file.path, text);

            if (!filesData.files.Contains(file))
            {
                filesData.files.Add(file);
            }

            SaveFilesData();

            filesListChanged?.Invoke();

            Debug.Log("File is saved. Path: " + file.path);
        }
        catch
        {
            Debug.Log("Saving file is end error.");
        }
    }
}
