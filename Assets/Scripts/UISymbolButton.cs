using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UISymbolButton : MonoBehaviour
{
    [SerializeField] private char firstSymbol;
    [SerializeField] private char secondSymbol;

    [SerializeField] private TMP_Text firstSymbolText;
    [SerializeField] private TMP_Text secondSymbolText;

    public char FirstSymbol { get => firstSymbol; }
    public char SecondSymbol { get => secondSymbol; }

    private void Awake()
    {
        name = firstSymbol.ToString();

        firstSymbolText.text = firstSymbol.ToString();
        secondSymbolText.text = secondSymbol.ToString();
    }
}
