using SousRaccoon.Data;
using SousRaccoon.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UIPerkInfoSlot : MonoBehaviour
    {
        public Button perkButton;

        [SerializeField] TMP_Text perkLevel;
        [SerializeField] Image perkIcon;

        private Sprite perkSprite;
        private LocalizedString perkName;
        private LocalizedString perkDescription;
        private string levelPerk;
        private UIPerkInfoListPanel perkInfoListPanel;

        private void OnDestroy()
        {
            perkButton.onClick.RemoveAllListeners();
        }

        public void SetUpPerkInfo(string levelPerk, UIPerkInfoListPanel panel, ShopMerchantItemDataBase perkData)
        {
            perkButton.onClick.RemoveAllListeners();

            perkLevel.text = levelPerk;
            perkIcon.sprite = perkData.icon;
            perkSprite = perkData.icon;
            perkName = perkData.perkDisplayName;
            perkDescription = perkData.description;
            perkInfoListPanel = panel;
            this.levelPerk = levelPerk;

            // ล้าง Event Listener เก่าออกก่อนป้องกันการซ้อนทับ
            perkButton.onClick.RemoveAllListeners();
            perkButton.onClick.AddListener(OpenDescription);

            perkButton.onClick.AddListener(AudioManager.instance.PlaySoundButtonClick);
        }

        private void OpenDescription()
        {
            perkInfoListPanel.OpenDiscriptionPerkInfo(perkSprite, perkName, perkDescription, levelPerk);
        }
    }
}
