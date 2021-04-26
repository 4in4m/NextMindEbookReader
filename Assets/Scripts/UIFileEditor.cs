using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIFileEditor : MonoBehaviour
{
    [SerializeField] private AppManager appManager;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private RectTransform symbolsButtonsParent;
    [SerializeField] private UIExpandButton[] expandButtons;

    private bool firstSymbolInputMode = true;

    private FileData currentFile;

    private void Awake()
    {
        appManager = FindObjectOfType<AppManager>();
    }

    void Start()
    {
        UISymbolButton[] buttons = Resources.FindObjectsOfTypeAll(typeof(UISymbolButton)) as UISymbolButton[];

        foreach (var button in buttons)
        {
            button.GetComponent<Button>().onClick.AddListener(() => InputSymbol(button));
        }

        foreach (var btn in expandButtons)
        {
            btn.GetComponent<Button>().onClick.AddListener(() => ExpandPanel(btn));
        }
    }

    private void InputSymbol(UISymbolButton symbolButton)
    {
        char c = firstSymbolInputMode ? symbolButton.FirstSymbol : symbolButton.SecondSymbol;

        inputField.text += c;
    }

    public void RemoveLastSymbol()
    {
        int l = inputField.text.Length;

        if (l > 0)
        {
            inputField.text = inputField.text.Remove(l - 1);
        }
    }

    public void InputEnterLine()
    {
        inputField.text += "\n";
    }

    public void InputSpaceSymbol()
    {
        inputField.text += " ";
    }

    public void SwitchInputMode()
    {
        firstSymbolInputMode = !firstSymbolInputMode;
    }

    private void ExpandPanel(UIExpandButton button)
    {
        foreach (var btn in expandButtons)
        {
            if (btn.IsActive)
            {
                btn.Close();
            }
        }

        button.Expand();
    }

    public void OpenFile(FileData file)
    {
        if(file != null)
        {
            inputField.text = appManager.LoadFile(file.path);
        } else
        {
            inputField.text = string.Empty;
        }

        currentFile = file;
    }

    public void SaveFile()
    {
        appManager.SaveFile(currentFile, inputField.text);
    }
}
