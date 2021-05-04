﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIExpandButton : MonoBehaviour
{
    [SerializeField] private GameObject _expandPanel;

    [SerializeField] private RectTransform _symbolButtonsParent;

    public bool IsActive { get; private set; }

    public UISymbolButton[] SymbolButtons { get; private set; }

    public delegate void OnTriggered(UIExpandButton button);
    public OnTriggered onExpanded;
    public OnTriggered onClosed;

    private void Awake()
    {
        Close();

        SymbolButtons = _symbolButtonsParent.GetComponentsInChildren<UISymbolButton>();
    }

    public void Expand()
    {
        IsActive = true;

        GetComponent<Image>().enabled = false;
        _expandPanel.SetActive(true);

        onExpanded?.Invoke(this);
    }

    public void Close()
    {
        IsActive = false;

        GetComponent<Image>().enabled = true;
        _expandPanel.SetActive(false);

        onClosed?.Invoke(this);
    }
}
