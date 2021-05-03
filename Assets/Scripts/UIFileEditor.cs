using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NextMind.NeuroTags;

public class UIFileEditor : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;

    [SerializeField] private Button _saveButton;

    [SerializeField] private Button _closeButton;

    [SerializeField] private Button _backspaceButton;

    [SerializeField] private Button _enterButton;

    [SerializeField] private Button _spaceButton;

    [SerializeField] private Button _symbolModeButton;

    [SerializeField] private UIExpandButton[] _expandButtons;

    private bool _firstSymbolInputMode = true;

    private FileData _currentFile;

    void Awake()
    {
        UISymbolButton[] buttons = Resources.FindObjectsOfTypeAll(typeof(UISymbolButton)) as UISymbolButton[];

        foreach (var button in buttons)
        {
            button.GetComponent<Button>().onClick.AddListener(() => InputSymbol(button));
            button.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => InputSymbol(button));
        }

        foreach (var btn in _expandButtons)
        {
            btn.GetComponent<Button>().onClick.AddListener(() => ExpandPanel(btn));
            btn.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => ExpandPanel(btn));
        }

        _saveButton.onClick.AddListener(() => SaveFile());
        _saveButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => SaveFile());

        UIManager uIManager = transform.root.GetComponent<UIManager>();

        _closeButton.onClick.AddListener(() => uIManager.OpenFile(_currentFile));
        _closeButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => uIManager.OpenFile(_currentFile));

        _backspaceButton.onClick.AddListener(() => RemoveLastSymbol());
        _backspaceButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => RemoveLastSymbol());

        _enterButton.onClick.AddListener(() => InputEnterLine());
        _enterButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => InputEnterLine());

        _spaceButton.onClick.AddListener(() => InputSpaceSymbol());
        _spaceButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => InputSpaceSymbol());

        _symbolModeButton.onClick.AddListener(() => SwitchInputMode());
        _symbolModeButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => SwitchInputMode());
    }

    private void InputSymbol(UISymbolButton symbolButton)
    {
        char c = _firstSymbolInputMode ? symbolButton.FirstSymbol : symbolButton.SecondSymbol;

        _inputField.text += c;
    }

    private void ExpandPanel(UIExpandButton button)
    {
        foreach (var btn in _expandButtons)
        {
            if (btn.IsActive)
            {
                btn.Close();
            }
        }

        button.Expand();
    }

    public void RemoveLastSymbol()
    {
        int l = _inputField.text.Length;

        if (l > 0)
        {
            _inputField.text = _inputField.text.Remove(l - 1);
        }
    }

    public void InputEnterLine()
    {
        _inputField.text += "\n";
    }

    public void InputSpaceSymbol()
    {
        _inputField.text += " ";
    }

    public void SwitchInputMode()
    {
        _firstSymbolInputMode = !_firstSymbolInputMode;
    }

    public void OpenFile(FileData file)
    {
        if (file != null)
        {
            _inputField.text = AppManager.Instance.LoadFile(file.Path);
        }
        else
        {
            _inputField.text = string.Empty;
        }

        _currentFile = file;
    }

    public void SaveFile()
    {
        AppManager.Instance.SaveFile(_currentFile, _inputField.text);
    }
}
