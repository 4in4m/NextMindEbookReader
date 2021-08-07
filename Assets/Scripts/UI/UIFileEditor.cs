using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NextMind.NeuroTags;
using UnityEngine.Events;
using System;
using UnityEngine.EventSystems;

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

        public delegate void OnButtonClicked();

        public OnButtonClicked onButtonClicked;

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

            UnityAction saveFile = () =>
            {
                AppManager.Instance.SaveFile(_currentFile, _inputField.text);
                uIManager.OpenMainMenu();
            };

            _saveButton.onClick.AddListener(saveFile);
            _saveButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(saveFile);

            _closeButton.onClick.AddListener(() => uIManager.OpenMainMenu());
            _closeButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => uIManager.OpenMainMenu());

            _backspaceButton.onClick.AddListener(() => RemoveLastSymbol());
            _backspaceButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => RemoveLastSymbol());

            _enterButton.onClick.AddListener(() => InputEnterLine());
            _enterButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => InputEnterLine());

            _spaceButton.onClick.AddListener(() => InputSpaceSymbol());
            _spaceButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => InputSpaceSymbol());

            _symbolModeButton.onClick.AddListener(() => SwitchInputMode());
            _symbolModeButton.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() => SwitchInputMode());

            onButtonClicked += FocusOnInputPanel;
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
                        expandButton.transform.GetChild(0).gameObject.SetActive(true);
                        DisplayControls(expandButton);
                    });

                    button.GetComponentInChildren<NeuroTag>().onTriggered.AddListener(() =>
                    {
                        InputSymbol(button);
                        expandButton.Close();
                        expandButton.transform.GetChild(0).gameObject.SetActive(true);
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

            if (_inputField.caretPosition - _inputField.selectionAnchorPosition != 0)
            {
                RemoveLastSymbol();
            }

            string text = _inputField.text;
            int pos = _inputField.caretPosition < _inputField.selectionAnchorPosition ? _inputField.caretPosition : _inputField.selectionAnchorPosition;

            _inputField.text = text.Insert(pos, c.ToString());

            _inputField.caretPosition = pos + 1;

            onButtonClicked?.Invoke();
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
            button.transform.GetChild(0).gameObject.SetActive(false);

            onButtonClicked?.Invoke();
        }

        public void RemoveLastSymbol()
        {
            int pos = _inputField.caretPosition < _inputField.selectionAnchorPosition ? _inputField.caretPosition : _inputField.selectionAnchorPosition;
            int removeCount = 0;
            

            if (pos > 0)
            {
                int length = _inputField.text.Length;
                int caretPos = _inputField.caretPosition;

                removeCount = Mathf.Abs(_inputField.caretPosition - _inputField.selectionAnchorPosition);
                removeCount = Mathf.Max(1, removeCount);

                pos = removeCount > 1 ? pos : pos - 1;

                _inputField.text = _inputField.text.Remove(pos, removeCount);

                if (removeCount == 1)
                {
                    if (caretPos < length)
                    {
                        _inputField.caretPosition--;
                    }
                }
                else
                {
                    if (_inputField.caretPosition > _inputField.selectionAnchorPosition)
                    {
                        _inputField.caretPosition = _inputField.selectionAnchorPosition;
                    }
                }

                _inputField.selectionAnchorPosition = _inputField.caretPosition;

                _inputField.ForceLabelUpdate();
            }

            onButtonClicked?.Invoke();
        }

        public void InputEnterLine()
        {
            _inputField.text = _inputField.text.Insert(_inputField.caretPosition, "\n");
            _inputField.caretPosition++;

            onButtonClicked?.Invoke();
        }

        public void InputSpaceSymbol()
        {
            _inputField.text = _inputField.text.Insert(_inputField.caretPosition, " ");
            _inputField.caretPosition++;

            onButtonClicked?.Invoke();
        }

        public void SwitchInputMode()
        {
            _firstSymbolInputMode = !_firstSymbolInputMode;

            foreach (var btn in _expandButtons)
            {
                btn.SetSymbolsMode(!_firstSymbolInputMode);
            }

            onButtonClicked?.Invoke();
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

        private void FocusOnInputPanel()
        {
            _inputField.ActivateInputField();
        }
    }
}
