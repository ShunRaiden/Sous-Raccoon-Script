using SousRaccoon.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI
{
    public class UIGamePausePanel : MonoBehaviour
    {
        [SerializeField] Button settingButton;

        private void Awake()
        {
            settingButton.onClick.AddListener(OnOpenSettingPanel);
        }

        private void OnDestroy()
        {
            settingButton.onClick.RemoveAllListeners();
        }

        public void OnOpenSettingPanel()
        {
            SettingManager.instance.Init();
        }
    }
}