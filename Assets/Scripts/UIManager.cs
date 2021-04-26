using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    [SerializeField] private AppManager appManager;

    [Header("Books")]

    [SerializeField] private RectTransform booksListParent;

    [SerializeField] private GameObject booksListPanel;

    [SerializeField] private GameObject bookContentPanel;

    [SerializeField] private TMP_Text bookContentText;

    [SerializeField] private Button booksButton;

    [Header("Files")]

    [SerializeField] private RectTransform filesListParent;

    [SerializeField] private GameObject filesListPanel;

    [SerializeField] private UIFileEditor fileEditor;

    private GameObject[] booksSlots;
    private GameObject[] filesSlots;
    private GameObject openedPanel;
    private Button pressedButton;

    void Start()
    {
        appManager.booksListChanged += UpdateBooksListUI;
        appManager.filesListChanged += UpdateFilesListUI;

        booksSlots = new GameObject[booksListParent.childCount];
        for (int i = 0; i < booksSlots.Length; i++)
        {
            booksSlots[i] = booksListParent.GetChild(i).gameObject;
        }

        filesSlots = new GameObject[filesListParent.childCount];
        for (int i = 0; i < filesSlots.Length; i++)
        {
            filesSlots[i] = filesListParent.GetChild(i).gameObject;
        }

        UpdateBooksListUI();
        UpdateFilesListUI();

        OpenPanel(booksListPanel);
        pressedButton = booksButton;
        pressedButton.interactable = false;
    }

    public void OpenImportPanel()
    {
        appManager.ShowLoadDialog();
    }

    public void OpenPanel(GameObject panel)
    {
        if (openedPanel != null)
        {
            openedPanel.SetActive(false);
            pressedButton.interactable = true;
        }

        openedPanel = panel;
        
        pressedButton = EventSystem.current.currentSelectedGameObject?.GetComponent<Button>();
        if (pressedButton != null)
        {
            pressedButton.interactable = false;
        }

        openedPanel.SetActive(true);
    }

    private void UpdateBooksListUI()
    {
        for (int i = 0; i < booksSlots.Length; i++)
        {
            if(appManager.booksData == null || i >= appManager.booksData.books.Count || appManager.booksData.books[i] == null || appManager.booksData.books[i].path == string.Empty)
            {
                booksSlots[i].SetActive(false);
                continue;
            }

            GameObject slot = booksSlots[i];
            int slotIndex = slot.transform.GetSiblingIndex();

            slot.SetActive(true);
            slot.GetComponentInChildren<TMP_Text>().text = appManager.booksData.books[i].name;

            Button[] buttons = slot.GetComponentsInChildren<Button>();

            foreach (var btn in buttons)
            {
                btn.onClick.RemoveAllListeners();
            }

            buttons[0].onClick.AddListener(() => OpenBook(slotIndex));
            buttons[1].onClick.AddListener(() => RemoveBook(slotIndex));
        }
    }

    private void UpdateFilesListUI()
    {
        for (int i = 0; i < booksSlots.Length; i++)
        {
            if (appManager.filesData == null || i >= appManager.filesData.files.Count || appManager.filesData.files[i] == null || appManager.filesData.files[i].path == string.Empty)
            {
                filesSlots[i].SetActive(false);
                continue;
            }

            GameObject slot = filesSlots[i];
            int slotIndex = slot.transform.GetSiblingIndex();

            slot.SetActive(true);
            slot.GetComponentInChildren<TMP_Text>().text = appManager.filesData.files[i].name;

            Button[] buttons = slot.GetComponentsInChildren<Button>();

            foreach (var btn in buttons)
            {
                btn.onClick.RemoveAllListeners();
            }

            buttons[0].onClick.AddListener(() => OpenFile(slotIndex));
            buttons[1].onClick.AddListener(() => RemoveFile(slotIndex));
        }
    }

    public void OpenBook(int index)
    {
        BookData book = appManager.GetBook(index);

        OpenPanel(bookContentPanel);

        string text = appManager.LoadFile(book.path);

        ViewBookText(text);
    }

    public void RemoveBook(int index)
    {
        appManager.RemoveBook(index);
        UpdateBooksListUI();
    }

    public void RemoveFile(int index)
    {
        appManager.RemoveFile(index);
        UpdateFilesListUI();
    }

    public void OpenFile(int index)
    {
        FileData file = appManager.GetFile(index);

        fileEditor.OpenFile(file);

        OpenPanel(fileEditor.gameObject);
    }

    public void SaveFile()
    {
        fileEditor.SaveFile();

        OpenPanel(filesListPanel);
    }

    public void NextPage()
    {
        if (bookContentText.pageToDisplay >= bookContentText.textInfo.pageCount)
        {
            appManager.curCharIndex += 16383;
            string newText = appManager.curText.Substring(appManager.curCharIndex, 16383);
            bookContentText.text = newText;
            bookContentText.pageToDisplay = 1;
        } else
        {
            bookContentText.pageToDisplay++;
        }
    }

    public void PrevPage()
    {
        if (bookContentText.pageToDisplay > 1)
        {
            bookContentText.pageToDisplay--;
        }
        else if (appManager.curCharIndex >= 16383)
        {
            appManager.curCharIndex -= 16383;
            string newText = appManager.curText.Substring(appManager.curCharIndex, 16383);
            bookContentText.text = newText;
            bookContentText.ForceMeshUpdate();
            bookContentText.pageToDisplay = bookContentText.textInfo.pageCount;
        }
    }

    private void ViewBookText(string _text)
    {
        appManager.curText = _text;

        string text = _text.Substring(0, 16383);
        bookContentText.text = text;
        bookContentText.ForceMeshUpdate();

        Debug.Log(bookContentText.firstOverflowCharacterIndex);
    }
}
