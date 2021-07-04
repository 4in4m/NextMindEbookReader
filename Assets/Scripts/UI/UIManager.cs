using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NextMind.NeuroTags;
using UnityEngine.SceneManagement;
using System.Collections;
using SimpleFileBrowser;

namespace EBookReader
{
    public class UIManager : MonoBehaviour
    {
        private const int FILES_PER_PAGE = 4;

        [SerializeField] private GameObject _mainMenu;

        [SerializeField] private RectTransform _fileListParent;

        [SerializeField] private UIFileEditor _fileEditor;

        [SerializeField] private UIBookViewer _bookViewer;

        [SerializeField] private Button _newFileButton;

        [SerializeField] private Button _addButton;

        [SerializeField] private Button _extraButton;

        [SerializeField] private GameObject _extraPanel;

        [SerializeField] private Button _calibrationButton;

        [SerializeField] private Button _exitButton;

        [SerializeField] private Button _pageUpButton;

        [SerializeField] private Button _pageDownButton;

        private UIFileSlot[] _filesSlots;

        private GameObject _openedPanel;

        private int _currentFileIndex;

        void Start()
        {
            AppManager.Instance.filesListChanged += UpdateFilesListUI;

            _filesSlots = _fileListParent.GetComponentsInChildren<UIFileSlot>();

            UpdateFilesListUI();

            OpenPanel(_mainMenu);

            _addButton.onClick.AddListener(() => OpenImportPanel());
            //_addButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => OpenImportPanel());

            _extraButton.onClick.AddListener(() => _extraPanel.SetActive(!_extraPanel.activeSelf));
            _extraButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => _extraPanel.SetActive(!_extraPanel.activeSelf));

            _calibrationButton.onClick.AddListener(() => StartCalibration());
            _calibrationButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => StartCalibration());

            _exitButton.onClick.AddListener(() => Application.Quit());
            _exitButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => Application.Quit());

            UnityAction createNewFile = new UnityAction(() =>
            {
                _fileEditor.OpenFile(null);

                OpenPanel(_fileEditor.gameObject);
            });

            _newFileButton.onClick.AddListener(createNewFile);
            _newFileButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(createNewFile);

            _pageUpButton.onClick.AddListener(() => ShowPrevFiles());
            _pageUpButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => ShowPrevFiles());

            _pageDownButton.onClick.AddListener(() => ShowNextFiles());
            _pageDownButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => ShowNextFiles());
        }

        private void StartCalibration()
        {
            _extraPanel.SetActive(false);

            SceneManager.LoadScene("MyCalibration", LoadSceneMode.Additive);
        }

        private void EndCalibration()
        {
            SceneManager.UnloadSceneAsync("MyCalibartion"); ;
        }

        private void ShowPrevFiles()
        {
            if (_currentFileIndex == 0)
            {
                return;
            }

            _currentFileIndex -= FILES_PER_PAGE;

            if (_currentFileIndex < 0)
            {
                _currentFileIndex = 0;
            }

            UpdateFilesListUI();
        }

        private void ShowNextFiles()
        {
            if (AppManager.Instance.FilesData == null || AppManager.Instance.FilesData.Files == null)
            {
                return;
            }

            int fileIndex = _currentFileIndex + FILES_PER_PAGE;

            if (fileIndex >= AppManager.Instance.FilesData.Files.Count)
            {
                return;
            }

            _currentFileIndex = fileIndex;

            UpdateFilesListUI();
        }

        private void UpdateFilesListUI()
        {
            var appManager = AppManager.Instance;
            int fileIndex = _currentFileIndex;

            for (int i = 0; i < _filesSlots.Length; i++)
            {
                if (appManager.FilesData == null || fileIndex >= appManager.FilesData.Files.Count || appManager.FilesData.Files[fileIndex] == null || appManager.FilesData.Files[fileIndex].Path == string.Empty)
                {
                    _filesSlots[i].gameObject.SetActive(false);
                    continue;
                }

                var file = appManager.FilesData.Files[fileIndex];
                var slot = _filesSlots[i];

                if (!FileBrowserHelpers.FileExists(file.Path))
                {
                    appManager.FilesData.Files.Remove(file);
                    appManager.SaveFilesData();

                    _filesSlots[i].gameObject.SetActive(false);
                    continue;
                }

                Sprite cover = null;

                if (file.CoverImagePath != string.Empty)
                {
                    if (!FileBrowserHelpers.FileExists(file.CoverImagePath))
                    {
                        file.CoverImagePath = string.Empty;
                        appManager.SaveFilesData();
                    }
                    else
                    {
                        cover = appManager.LoadSprite(file.CoverImagePath);
                    }
                }

                UnityAction openFile = new UnityAction(() =>
                {
                    OpenFile(file);
                });

                slot.Init(cover, file.Name, openFile);

                fileIndex++;
            }
        }

        public void OpenImportPanel()
        {
            AppManager.Instance.ShowLoadDialog();
        }

        public void OpenPanel(GameObject panel)
        {
            if (_openedPanel != null)
            {
                _openedPanel.SetActive(false);
            }

            _openedPanel = panel;

            _openedPanel.SetActive(true);
        }

        public void OpenMainMenu()
        {
            OpenPanel(_mainMenu);
        }

        public void OpenFile(FileData file)
        {
            var appManager = AppManager.Instance;

            switch (file.Type)
            {
                case FileData.FileType.Book:
                    string text = appManager.LoadFile(file.Path);

                    OpenPanel(_bookViewer.gameObject);

                    StartCoroutine(_bookViewer.DisplayBook(file));
                    break;
                case FileData.FileType.UserFile:
                    OpenPanel(_fileEditor.gameObject);

                    _fileEditor.OpenFile(file);
                    break;
                case FileData.FileType.PdfFile:
                    OpenPanel(_bookViewer.gameObject);

                    StartCoroutine(_bookViewer.DisplayBook(file));
                    break;
            }
        }

        public void EditFile(FileData file)
        {
            if (file.Type == FileData.FileType.PdfFile)
            {
                return;
            }

            OpenPanel(_fileEditor.gameObject);

            _fileEditor.OpenFile(file);
        }
    }
}
