﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NextMind.NeuroTags;

namespace EBookReader
{
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
            UIManager uIManager = transform.root.GetComponent<UIManager>();

            foreach (var btn in _expandButtons)
            {
                btn.GetComponent<Button>().onClick.AddListener(() => ExpandPanel(btn));
                btn.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => ExpandPanel(btn));

                btn.onExpanded += HideControls;
                btn.onClosed += DisplayControls;
            }

            _saveButton.onClick.AddListener(() => SaveFile());
            _saveButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => SaveFile());

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

        private void Start()
        {
            foreach (var expandButton in _expandButtons)
            {
                foreach (var button in expandButton.SymbolButtons)
                {
                    button.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        InputSymbol(button);
                        expandButton.Close();
                        DisplayControls(expandButton);
                    });

                    button.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() =>
                    {
                        InputSymbol(button);
                        expandButton.Close();
                        DisplayControls(expandButton);
                    });
                }
            }
        }

        private void DisplayControls(UIExpandButton pressedButton)
        {
            _saveButton.gameObject.SetActive(true);
            _closeButton.gameObject.SetActive(true);

            _backspaceButton.gameObject.SetActive(true);
            _spaceButton.gameObject.SetActive(true);
            _enterButton.gameObject.SetActive(true);
            _symbolModeButton.gameObject.SetActive(true);

            foreach (var item in _expandButtons)
            {
                if (item != pressedButton)
                {
                    item.gameObject.SetActive(true);
                }
            }
        }

        private void HideControls(UIExpandButton pressedButton)
        {
            _saveButton.gameObject.SetActive(false);
            _closeButton.gameObject.SetActive(false);

            _backspaceButton.gameObject.SetActive(false);
            _spaceButton.gameObject.SetActive(false);
            _enterButton.gameObject.SetActive(false);
            _symbolModeButton.gameObject.SetActive(false);

            foreach (var item in _expandButtons)
            {
                if (item != pressedButton)
                {
                    item.gameObject.SetActive(false);
                }
            }
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
}
