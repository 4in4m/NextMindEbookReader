using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

using UnityEngine;

using SimpleFileBrowser;
using FB2Library;
using VersOne.Epub;
using HtmlAgilityPack;

namespace EBookReader
{
    [System.Serializable]
    public class FilesData
    {
        public List<FileData> Files = new List<FileData>();
    }

    public class AppManager : MonoBehaviour
    {
        [SerializeField] private string _sourceFolder = "/Import/";
        [SerializeField] private string _targetFolder = "/Files/";
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

            FileBrowser.Filter filter = new FileBrowser.Filter("fb2", ".fb2", "epub", ".epub");
            FileBrowser.SetFilters(false, filter);

            ImportFiles();
        }

        private async void ImportFiles()
        {
            string path = Application.persistentDataPath + _sourceFolder;

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
                return;
            }

            var files = FileBrowserHelpers.GetEntriesInDirectory(path);

            foreach (var file in files)
            {
                string fileName = string.Empty;
                string filePath = string.Empty;
                string imagePath = string.Empty;

                string text = string.Empty;
                string finalText = string.Empty;
                byte[] imageData = null;
                FileData newBook = null;

                if (!file.IsDirectory)
                {
                    switch (file.Extension)
                    {
                        case ".fb2":
                            fileName = FileBrowserHelpers.GetFilename(file.Path);
                            text = FileBrowserHelpers.ReadTextFromFile(file.Path);

                            FB2File fb2File = await new FB2Reader().ReadAsync(text);
                            var lines = await _fB2SampleConverter.ConvertAsync(fb2File);
                            finalText = _fB2SampleConverter.GetLinesAsText();

                            imageData = _fB2SampleConverter.GetCoverImageData();

                            if (imageData != null)
                            {
                                imagePath = Application.persistentDataPath + _targetFolder + fileName + ".jpg"; ;

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
                            }

                            filePath = (Application.persistentDataPath + _targetFolder + fileName).Replace("/", "\\");

                            newBook = new FileData(fileName, filePath, imagePath, FileData.FileType.Book);

                            SaveFile(newBook, finalText);

                            FileBrowserHelpers.DeleteFile(file.Path);
                            break;
                        case ".epub":
                            EpubBook epubFile = EpubReader.ReadBook(file.Path);

                            fileName = epubFile.Title + " (" + epubFile.Author + ")";

                            foreach (EpubTextContentFile textContentFile in epubFile.ReadingOrder)
                            {
                                HtmlDocument htmlDocument = new HtmlDocument();
                                htmlDocument.LoadHtml(textContentFile.Content);

                                StringBuilder sb = new StringBuilder();
                                foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//text()"))
                                {
                                    sb.AppendLine(node.InnerText.Replace("\n", "").Replace("\r", ""));
                                }

                                finalText += sb.ToString();
                            }

                            imageData = epubFile.CoverImage;

                            var imageName = string.Empty;

                            if (imageData == null)
                            {
                                imageData = epubFile.Content.Images.FirstOrDefault().Value.Content;
                                imageName = epubFile.Content.Images.FirstOrDefault().Key;
                            }
                            else
                            {
                                imageName = epubFile.Content.Cover.FileName;
                            }

                            if (imageData != null)
                            {
                                imagePath = Application.persistentDataPath + _targetFolder + imageName;

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
                            }

                            filePath = (Application.persistentDataPath + _targetFolder + fileName).Replace("/", "\\");

                            newBook = new FileData(fileName, filePath, imagePath, FileData.FileType.Book);

                            SaveFile(newBook, finalText);

                            FileBrowserHelpers.DeleteFile(file.Path);
                            break;
                        case ".pdf":
                            var document = PdfiumViewer.PdfDocument.Load(file.Path);
                            var title = document.GetInformation().Title;

                            if (FilesData.Files.Any(x => x.Name == title))
                            {
                                return;
                            }

                            fileName = FileBrowserHelpers.GetFilename(file.Path);
                            filePath = Application.persistentDataPath + _targetFolder + fileName;

                            imagePath = Application.persistentDataPath + _targetFolder + fileName.Remove(fileName.Length - 4) + ".png";

                            var image = document.Render(0, 72, 72, false);
                            image.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

                            imagePath = imagePath.Replace("/", "\\");
                            filePath = filePath.Replace("/", "\\");

                            document.Dispose();

                            FileBrowserHelpers.MoveFile(file.Path, filePath);

                            newBook = new FileData(title, filePath, imagePath, FileData.FileType.PdfFile);

                            SaveFile(newBook);
                            
                            break;
                    }
                }
                else
                {
                    FileBrowserHelpers.DeleteDirectory(file.Path);
                }
            }
        }

        /// <summary>
        ///  Отобразить окно импорта файла.
        /// </summary>
        public void ShowLoadDialog()
        {
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        private IEnumerator ShowLoadDialogCoroutine()
        {
            // Show a load file dialog and wait for a response from user
            // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
            yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, null, "Load Book", "Load");

            if (FileBrowser.Success)
            {
                foreach (string path in FileBrowser.Result)
                {
                    string fileName = FileBrowserHelpers.GetFilename(path);
                    string targetPath = Application.persistentDataPath + _sourceFolder + fileName;
                    FileBrowserHelpers.MoveFile(path, targetPath);

                    ImportFiles();
                }
            }

            onFB2Loaded?.Invoke();
        }

        private async void ReadFB2File(string path)
        {
            string fileName = FileBrowserHelpers.GetFilename(path);
            string text = FileBrowserHelpers.ReadTextFromFile(path);

            FB2File file = await new FB2Reader().ReadAsync(text);
            var lines = await _fB2SampleConverter.ConvertAsync(file);
            string finalText = _fB2SampleConverter.GetLinesAsText();

            byte[] imageData = _fB2SampleConverter.GetCoverImageData();

            string imagePath = Application.persistentDataPath + _targetFolder + fileName + ".jpg"; ;

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

            string filePath = (Application.persistentDataPath + _targetFolder + fileName).Replace("/", "\\");

            FileData newBook = new FileData(fileName, filePath, imagePath, FileData.FileType.Book);

            SaveFile(newBook, finalText);
        }

        public void SaveFilesData()
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

        public void GetFilesForImport()
        {
            //
        }

        public Sprite LoadSprite(string path)
        {
            byte[] bytes = new byte[0];
            Texture2D tex = new Texture2D(2, 2);

            bytes = FileBrowserHelpers.ReadBytesFromFile(path);
            tex.LoadImage(bytes);

            Rect rec = new Rect(0, 0, tex.width, tex.height);
            Sprite sprite = Sprite.Create(tex, rec, new Vector2(0.5f, 0.5f), 100);

            return sprite;
        }

        public Sprite GetPage(PdfiumViewer.PdfDocument doc, int pageNum)
        {
            if (pageNum >= doc.PageCount)
            {
                return null;
            }

            var tempPath = Application.persistentDataPath + _targetFolder + "_tempImage_.png";

            if (FileBrowserHelpers.FileExists(tempPath))
            {
                FileBrowserHelpers.DeleteFile(tempPath);
            }

            var image = doc.Render(pageNum, 72, 72, false);
            image.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

            var sprite = LoadSprite(tempPath);

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

            if (file.CoverImagePath != string.Empty)
            {
                try
                {
                    FileBrowserHelpers.DeleteFile(file.CoverImagePath);
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

        public void SaveFile(FileData file, string text = null)
        {
            if (file == null)
            {
                string name = DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToLongTimeString().Replace(":", "_");
                string path = Application.persistentDataPath + _targetFolder;

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
                path += name + ".txt";

                file = new FileData(name, path, String.Empty, FileData.FileType.UserFile);
            }

            try
            {
                if (text != null)
                {
                    FileBrowserHelpers.WriteTextToFile(file.Path, text);
                }

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
}