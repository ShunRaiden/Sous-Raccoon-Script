using SousRaccoon.Manager;
using SousRaccoon.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UIStageScenarioPanel : MonoBehaviour
    {
        public GameObject content;
        public Image iconPanel;
        public TMP_Text nameText;
        [SerializeField] private LocalizeStringEvent nameLocalize;
        [SerializeField] private LocalizeStringEvent descriptionLocalize;

        public Image statIcon;
        public TMP_Text statValueText;

        public Image headIcon;

        public StageScenarioManager stageScenarioManager;

        public void SetupPanel(Sprite icon, LocalizedString nameLocalizedString, LocalizedString descriptionLocalizedString, Sprite stat, string statValue, Sprite head)
        {
            nameLocalize.StringReference = nameLocalizedString;
            descriptionLocalize.StringReference = descriptionLocalizedString;

            iconPanel.sprite = icon;

            statIcon.sprite = stat;
            statValueText.text = statValue;

            headIcon.sprite = head;
        }

        public void ClosePanel()
        {
            if (!StageManager.instance.isOpenInfo)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            FindObjectOfType<PlayerInputManager>().SetMoveStage(false);
            stageScenarioManager.isShowPanel = false;
            content.SetActive(false);
        }
    }
}