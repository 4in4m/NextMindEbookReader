using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NextMind.NeuroTags;

public class UIBookViewer : MonoBehaviour
{
    [SerializeField] private TMP_Text _contentText;

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

    private void Awake()
    {
        _nextPageButton.onClick.AddListener(() => NextPage());
        _nextPageButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => NextPage());

        _prevPageButton.onClick.AddListener(() => PrevPage());
        _prevPageButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => PrevPage());

        UIManager uIManager = transform.root.GetComponent<UIManager>();

        _backButton.onClick.AddListener(() => uIManager.OpenMainMenu());
        _backButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => uIManager.OpenMainMenu());

        _editButton.onClick.AddListener(() => uIManager.EditFile(_currentFile));
        _editButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => uIManager.EditFile(_currentFile));
    }

    public void NextPage()
    {
        if (_contentText.pageToDisplay >= _contentText.textInfo.pageCount)
        {
            _curCharIndex += 16383;

            string newText = _currentText.Substring(_curCharIndex, 16383);

            _contentText.text = newText;
            _contentText.pageToDisplay = 1;
        }
        else
        {
            _contentText.pageToDisplay++;
        }
    }

    public void PrevPage()
    {
        if (_contentText.pageToDisplay > 1)
        {
            _contentText.pageToDisplay--;
        }
        else if (_curCharIndex >= 16383)
        {
            _curCharIndex -= 16383;

            string newText = _currentText.Substring(_curCharIndex, 16383);

            _contentText.text = newText;
            _contentText.ForceMeshUpdate();
            _contentText.pageToDisplay = _contentText.textInfo.pageCount;
        }
    }

    public void DisplayFile(FileData file)
    {
        _currentFile = file;

        string text = AppManager.Instance.LoadFile(_currentFile.Path);

        _currentText = text;

        string newText = text.Substring(0, 16383);

        _contentText.text = newText;
        _contentText.ForceMeshUpdate();

        Debug.Log(_contentText.firstOverflowCharacterIndex);
    }
}
