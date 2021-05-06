using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NextMind.NeuroTags;

namespace EBookReader
{
    public class UIManager : MonoBehaviour
    {

        [SerializeField] private GameObject _mainMenu;

        [SerializeField] private RectTransform _fileListParent;

        [SerializeField] private UIFileEditor _fileEditor;

        [SerializeField] private UIBookViewer _bookViewer;

        [SerializeField] private Button _newFileButton;

        [SerializeField] private Button _importButton;

        [SerializeField] private Button _extraButton;

        [SerializeField] private GameObject _extraPanel;

        [SerializeField] private Button _pageUpButton;

        [SerializeField] private Button _pageDownButton;

        private UIFileSlot[] _filesSlots;

        private GameObject _openedPanel;

        void Start()
        {
            AppManager.Instance.filesListChanged += UpdateFilesListUI;

            _filesSlots = _fileListParent.GetComponentsInChildren<UIFileSlot>();

            UpdateFilesListUI();

            OpenPanel(_mainMenu);

            _importButton.onClick.AddListener(() => OpenImportPanel());
            _importButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => OpenImportPanel());

            _extraButton.onClick.AddListener(() => _extraPanel.SetActive(!_extraPanel.activeSelf));
            _extraButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => _extraPanel.SetActive(!_extraPanel.activeSelf));

            UnityAction createNewFile = new UnityAction(() =>
            {
                _fileEditor.OpenFile(null);

                OpenPanel(_fileEditor.gameObject);
            });

            _newFileButton.onClick.AddListener(createNewFile);
            _newFileButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(createNewFile);

            _pageUpButton.onClick.AddListener(() => PageUp());
            _pageUpButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => PageUp());

            _pageDownButton.onClick.AddListener(() => PageDown());
            _pageDownButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => PageDown());
        }

        private void PageUp()
        {
            //
        }

        private void PageDown()
        {
            //
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

        private void UpdateFilesListUI()
        {
            var appManager = AppManager.Instance;

            for (int i = 0; i < _filesSlots.Length; i++)
            {
                if (appManager.FilesData == null || i >= appManager.FilesData.Files.Count || appManager.FilesData.Files[i] == null || appManager.FilesData.Files[i].Path == string.Empty)
                {
                    _filesSlots[i].gameObject.SetActive(false);
                    continue;
                }

                var file = appManager.FilesData.Files[i];
                var slot = _filesSlots[i];

                Sprite cover = null;

                if (file.ImagePath != string.Empty)
                {
                    cover = appManager.LoadSprite(appManager.FilesData.Files[i].ImagePath);
                }

                UnityAction openFile = new UnityAction(() =>
                {
                    OpenFile(file);
                });

                slot.Init(cover, appManager.FilesData.Files[i].Name, openFile);
            }
        }

        public void OpenFile(FileData file)
        {
            var appManager = AppManager.Instance;

            switch (file.Type)
            {
                case FileData.FileType.Book:
                    string text = appManager.LoadFile(file.Path);

                    _bookViewer.DisplayFile(file);

                    OpenPanel(_bookViewer.gameObject);
                    break;
                case FileData.FileType.UserFile:
                    _fileEditor.OpenFile(file);

                    OpenPanel(_fileEditor.gameObject);
                    break;
            }
        }

        public void EditFile(FileData file)
        {
            OpenPanel(_fileEditor.gameObject);

            _fileEditor.OpenFile(file);
        }

        public void SaveFile()
        {
            _fileEditor.SaveFile();

            OpenPanel(_mainMenu);
        }

        public void RemoveFile(FileData file)
        {
            AppManager.Instance.RemoveFile(file);

            UpdateFilesListUI();
        }
    }
}
