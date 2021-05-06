using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EBookReader
{
    [Serializable]
    public class ElementsGroup
    {
        public Color Color;
        public Image[] Images;
        public TMP_Text[] Texts;
    }

    public class UIColorSwitcher : MonoBehaviour
    {
        [SerializeField] private List<ElementsGroup> _firstGroup;

        [SerializeField] private List<ElementsGroup> _secondGroup;

        private List<List<ElementsGroup>> _colorGroups = new List<List<ElementsGroup>>();

        private int _curIndex;

        private void Awake()
        {
            _curIndex = PlayerPrefs.GetInt("BookViewer_ColorMode");

            _colorGroups.Add(_firstGroup);
            _colorGroups.Add(_secondGroup);

            _curIndex--;
            SwitchColor();
        }

        public void SwitchColor()
        {
            _curIndex++;

            if (_curIndex >= _colorGroups.Count)
            {
                _curIndex = 0;
            }

            for (int i = 0; i < _colorGroups[_curIndex].Count; i++)
            {
                for (int j = 0; j < _colorGroups[_curIndex][i].Images.Length; j++)
                {
                    _colorGroups[_curIndex][i].Images[j].color = _colorGroups[_curIndex][i].Color;
                }

                for (int j = 0; j < _colorGroups[_curIndex][i].Texts.Length; j++)
                {
                    _colorGroups[_curIndex][i].Texts[j].color = _colorGroups[_curIndex][i].Color;
                }
            }

            PlayerPrefs.SetInt("BookViewer_ColorMode", _curIndex);
        }
    }
}