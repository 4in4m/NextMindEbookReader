using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIExpandButton : MonoBehaviour
{
    [SerializeField] private GameObject expandPanel;

    public bool IsActive { get; private set; }

    private void Awake()
    {
        Close();
    }

    public void Expand()
    {
        IsActive = true;

        GetComponent<Image>().enabled = false;
        expandPanel.SetActive(true);
    }

    public void Close()
    {
        IsActive = false;

        GetComponent<Image>().enabled = true;
        expandPanel.SetActive(false);
    }
}
