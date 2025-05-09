using System;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UIRandomMapPanel : MonoBehaviour
    {
        public event Action onOldMapButtonClickEvent;
        public event Action onNewMapButtonClickEvent;

        [SerializeField] GameObject content;

        [SerializeField] Image oldMapIcon;
        [SerializeField] Image oldMapRewardIcon;
        [SerializeField] TMP_Text oldMapRewardText;

        [SerializeField] Image debuffStageIcon;
        [SerializeField] LocalizeStringEvent debuffStageText;

        [SerializeField] Image newMapIcon;
        [SerializeField] Image newMapRewardIcon;
        [SerializeField] TMP_Text newMapRewardText;

        [SerializeField] Button oldMapButton;
        [SerializeField] Button newMapButton;

        private void Start()
        {
            oldMapButton.onClick.AddListener(OnOldMapButtonClick);
            newMapButton.onClick.AddListener(OnNewMapButtonClick);

            onOldMapButtonClickEvent += ClosePanel;
            onNewMapButtonClickEvent += ClosePanel;
        }

        private void OnDestroy()
        {
            oldMapButton.onClick.RemoveAllListeners();
            newMapButton.onClick.RemoveAllListeners();
        }

        public void SetUpPanel(Sprite oldIcon,
                               Sprite oldRewardIcon,
                               string oldRewardText,
                               Sprite newIcon,
                               Sprite newRewardIcon,
                               string newRewardText,
                               LocalizedString debuffText,
                               Sprite debuffIcon = null)
        {
            oldMapIcon.sprite = oldIcon;
            oldMapRewardIcon.sprite = oldRewardIcon;
            oldMapRewardText.text = oldRewardText;

            newMapIcon.sprite = newIcon;
            newMapRewardIcon.sprite = newRewardIcon;
            newMapRewardText.text = newRewardText;

            if (debuffIcon != null)
                debuffStageIcon.sprite = debuffIcon;
            else
                debuffStageIcon.gameObject.SetActive(false);

            debuffStageText.StringReference = debuffText;

            content.SetActive(true);
        }

        public void ClosePanel()
        {
            content.SetActive(false);
        }

        public void OnOldMapButtonClick()
        {
            onOldMapButtonClickEvent?.Invoke();
        }

        public void OnNewMapButtonClick()
        {
            onNewMapButtonClickEvent?.Invoke();
        }
    }
}