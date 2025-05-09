using SousRaccoon.Data;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UIStageDebuffSlot : MonoBehaviour
    {
        [SerializeField] Animator anim;
        [SerializeField] Image debuffIcon;
        [SerializeField] Sprite debuffSprite;
        [SerializeField] LocalizeStringEvent debuffNameText;
        [SerializeField] LocalizedString debuffName;
        [SerializeField] LocalizedString debuffDescription;
        private UIStageDebuffPanel debuffInfoListPanel;

        public void SetupDebuffInfoSlot(Sprite iconDebuff, UIStageDebuffPanel panel, StageDebuffDataBase dataBase)
        {
            debuffIcon.sprite = iconDebuff;
            debuffSprite = iconDebuff;
            debuffName = dataBase.debuffDisplayName;
            debuffDescription = dataBase.description;
            debuffInfoListPanel = panel;

            debuffNameText.StringReference = debuffName;
            debuffInfoListPanel.OnOpenPanelEvent += OpenDescription;
            debuffInfoListPanel.OnClosePanelEvent += CloseDescription;
        }

        private void OpenDescription()
        {
            if (anim != null)
                anim.Play("Open");
        }

        private void CloseDescription()
        {
            if (anim != null)
                anim.Play("Close");
        }
    }
}