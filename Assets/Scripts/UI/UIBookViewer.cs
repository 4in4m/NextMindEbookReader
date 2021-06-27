using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NextMind.NeuroTags;
using UnityEngine.Events;
using System.Linq;
using PdfiumViewer;
using System;

namespace EBookReader
{
    public class UIBookViewer : MonoBehaviour
    {
        [SerializeField] private UIColorSwitcher _colorSwitcher;

        [SerializeField] private TMP_Text _contentText;

        [SerializeField] private Image _pageImage;

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
        private PdfDocument _currentPdf;
        private string _currentText;
        private int _curCharIndex;

        private List<string> _tempImages = new List<string>();

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

            UnityAction onBackAction = new UnityAction(() =>
            {
                _speechController.StopSpeaking();
                uIManager.OpenMainMenu();
            });

            _backButton.onClick.AddListener(onBackAction);
            _backButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(onBackAction);

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
            
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator SyncTextFile()
        {
            _currentText = AppManager.Instance.LoadFile(_currentFile.Path);

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
        }

        private void SyncFile()
        {
            _curCharIndex = _currentFile.CurCharIndex;

            _currentPdf = PdfDocument.Load(_currentFile.Path);
            var sprite = AppManager.Instance.GetPage(_currentPdf, _curCharIndex);

            _pageImage.sprite = sprite;
        }

        private void NextPage()
        {
            if (_currentFile.Type == FileData.FileType.PdfFile)
            {
                if (_curCharIndex < _currentPdf.PageCount - 1)
                {
                    _curCharIndex++;
                }

                var sprite = AppManager.Instance.GetPage(_currentPdf, _curCharIndex);

                _pageImage.sprite = sprite;
            }
            else
            {
                _speechController.StopSpeaking();

                _leftCharsIndexes.Add(_curCharIndex);
                _curCharIndex += _contentText.firstOverflowCharacterIndex;

                string newText = _currentText.Substring(_curCharIndex, 16383);

                _contentText.text = newText;
                _contentText.ForceMeshUpdate();
            }

            _currentFile.CurCharIndex = _curCharIndex;
            AppManager.Instance.SaveFilesData();
        }

        private void PrevPage()
        {
            if (_currentFile.Type == FileData.FileType.PdfFile)
            {
                if (_curCharIndex <= 0)
                {
                    return;
                }

                _curCharIndex--;

                var sprite = AppManager.Instance.GetPage(_currentPdf, _curCharIndex);

                _pageImage.sprite = sprite;
            }
            else
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
            }

            _currentFile.CurCharIndex = _curCharIndex;
            AppManager.Instance.SaveFilesData();
        }

        private void SpeakCurrentPage()
        {
            if (_currentText != null)
            {
                string text = _currentText.Substring(_curCharIndex, _contentText.firstOverflowCharacterIndex);

                _speechController.SpeakText(text);
            }
        }

        public IEnumerator DisplayBook(FileData file)
        {
            _loadingPanel.SetActive(true);

            if (file != _currentFile)
            {
                yield return new WaitForSeconds(0.01f);

                _currentFile = file;

                switch (_currentFile.Type)
                {
                    case FileData.FileType.PdfFile:
                        SyncFile();
                        break;
                    default:
                        yield return SyncTextFile();

                        _loadingPanel.SetActive(false);
                        _contentText.enabled = true;
                        _pageImage.enabled = false;
                        break;
                }
            }

            switch (file.Type)
            {
                case FileData.FileType.PdfFile:
                    _contentText.enabled = false;
                    _pageImage.enabled = true;
                    _loadingPanel.SetActive(false);
                    break;
            }
        }
    }
}