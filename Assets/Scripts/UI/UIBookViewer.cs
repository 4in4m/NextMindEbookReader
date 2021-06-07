using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NextMind.NeuroTags;

namespace EBookReader
{
    public class UIBookViewer : MonoBehaviour
    {
        [SerializeField] private UIColorSwitcher _colorSwitcher;

        [SerializeField] private TMP_Text _contentText;

        [SerializeField] private GameObject _loadingPanel;

        [SerializeField] private Button _nextPageButton;

        [SerializeField] private Button _prevPageButton;

        [SerializeField] private Button _backButton;

        [SerializeField] private Button _speakButton;

        [SerializeField] private Button _zoomOutButton;

        [SerializeField] private Button _zoomInButton;

        [SerializeField] private Button _colorModeButton;

        [SerializeField] private Button _editButton;

        [SerializeField] private Button _editNameButton;

        [SerializeField] private Button _deleteButton;

        private FileData _currentFile;
        private string _currentText;
        private int _curCharIndex;

        private List<int> _leftCharsIndexes = new List<int>();

        private SpeechController _speechController;

        private void Awake()
        {
            _speechController = gameObject.AddComponent<SpeechController>();

            _nextPageButton.onClick.AddListener(() => NextPage());
            _nextPageButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => NextPage());

            _prevPageButton.onClick.AddListener(() => PrevPage());
            _prevPageButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => PrevPage());

            UIManager uIManager = transform.root.GetComponent<UIManager>();

            _backButton.onClick.AddListener(() => uIManager.OpenMainMenu());
            _backButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => uIManager.OpenMainMenu());

            _editButton.onClick.AddListener(() => uIManager.EditFile(_currentFile));
            _editButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => uIManager.EditFile(_currentFile));

            _deleteButton.onClick.AddListener(() =>
            {
                AppManager.Instance.RemoveFile(_currentFile);
                uIManager.OpenMainMenu();
            });
            _deleteButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() =>
            {
                AppManager.Instance.RemoveFile(_currentFile);
                uIManager.OpenMainMenu();
            });

            _colorModeButton.onClick.AddListener(() => _colorSwitcher.SwitchColor());
            _colorModeButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => _colorSwitcher.SwitchColor());

            _speakButton.onClick.AddListener(() => SpeakCurrentPage());
            _speakButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => SpeakCurrentPage());
        }

        private void OnEnable()
        {
            StartCoroutine(SyncFile());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator SyncFile()
        {
            _curCharIndex = 0;
            _leftCharsIndexes.Clear();

            _contentText.text = _currentText.Substring(_curCharIndex, 16383);
            _contentText.ForceMeshUpdate();

            while (_curCharIndex < _currentFile.CurCharIndex) 
            {
                _leftCharsIndexes.Add(_curCharIndex);
                _curCharIndex += _contentText.firstOverflowCharacterIndex;

                string newText = _currentText.Substring(_curCharIndex, 16383);

                _contentText.text = newText;
                _contentText.ForceMeshUpdate();

                yield return null;
            }

            _loadingPanel.SetActive(false);
        }

        private void NextPage()
        {
            _speechController.StopSpeaking();

            _leftCharsIndexes.Add(_curCharIndex);
            _curCharIndex += _contentText.firstOverflowCharacterIndex;

            string newText = _currentText.Substring(_curCharIndex, 16383);

            _contentText.text = newText;
            _contentText.ForceMeshUpdate();

            _currentFile.CurCharIndex = _curCharIndex;
            AppManager.Instance.SaveFilesData();
        }

        private void PrevPage()
        {
            _speechController.StopSpeaking();

            if (_leftCharsIndexes.Count > 0)
            {
                _curCharIndex = _leftCharsIndexes[_leftCharsIndexes.Count - 1];

                _leftCharsIndexes.Remove(_curCharIndex);
            }
            else
            {
                _curCharIndex = 0;
            }

            string newText = _currentText.Substring(_curCharIndex, 16383);

            _contentText.text = newText;
            _contentText.ForceMeshUpdate();

            _currentFile.CurCharIndex = _curCharIndex;
            AppManager.Instance.SaveFilesData();
        }

        private void SpeakCurrentPage()
        {
            string text = _currentText.Substring(_curCharIndex, _contentText.firstOverflowCharacterIndex);

            _speechController.SpeakText(text);
        }

        public void DisplayBook(FileData file)
        {
            _loadingPanel.SetActive(true);

            _currentFile = file;

            string text = AppManager.Instance.LoadFile(_currentFile.Path);

            _currentText = text;
        }
    }
}