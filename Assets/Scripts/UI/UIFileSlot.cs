using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using NextMind.NeuroTags;

namespace EBookReader
{
    public class UIFileSlot : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;

        [SerializeField] private Image _coverImage;

        [SerializeField] private Button _openButton;

        public void Init(Sprite coverSprite, string title, UnityAction open)
        {
            _titleText.text = title;

            _coverImage.sprite = coverSprite;

            _openButton.onClick.RemoveAllListeners();
            _openButton.onClick.AddListener(open);

            var neuroTag = _openButton.GetComponentInChildren<NeuroTag>();

            neuroTag.onTriggered.RemoveAllListeners();
            neuroTag.onTriggered.AddListener(open);

            gameObject.SetActive(true);
        }
    }
}