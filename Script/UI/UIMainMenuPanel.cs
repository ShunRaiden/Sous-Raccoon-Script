using SousRaccoon.Manager;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.UI.MainMenu
{
    public class UIMainMenuPanel : MonoBehaviour
    {
        public event Action EventNewGame;

        public GameObject mainMenuContentContainer;
        public GameObject newGameContentContainer;
        public GameObject loadGameContentContainer;
        public GameObject settingContentContainer;

        public Button newGameButton;
        public Button continueGameButton;
        public Button settingButton;
        public Button QuitButton;

        public UINewGamePanel newGamePanel;
        public UILoadGamePanel loadGamePanel;

        private void Awake()
        {
            newGameButton.onClick.AddListener(OnNewGame);
            continueGameButton.onClick.AddListener(OnLoadGame);
            settingButton.onClick.AddListener(OnSetting);
            QuitButton.onClick.AddListener(OnQuit);

            newGamePanel.EventAcceptSlot += OnCloseAllPanel;
            loadGamePanel.EventAcceptSlot += OnCloseAllPanel;
        }

        private void OnDestroy()
        {
            newGameButton.onClick.RemoveAllListeners();
            continueGameButton.onClick.RemoveAllListeners();
            settingButton.onClick.RemoveAllListeners();
            QuitButton.onClick.RemoveAllListeners();
        }

        public void OnNewGame()
        {
            newGamePanel.SetDataSlot();
            newGameContentContainer.SetActive(true);
        }

        public void OnLoadGame()
        {
            loadGamePanel.SetDataSlot();
            loadGameContentContainer.SetActive(true);
        }

        public void OnSetting()
        {
            settingContentContainer.SetActive(true);
            SettingManager.instance.Init();
        }

        public void OnCloseAllPanel()
        {
            mainMenuContentContainer.SetActive(false);
            newGameContentContainer.SetActive(false);
            loadGameContentContainer.SetActive(false);
            settingContentContainer.SetActive(false);
        }

        public void BackToMianMenuUINav()
        {
            GameManager.instance.ChangeUISelectNav(newGameButton.gameObject);
        }

        public void OnQuit()
        {
            GameManager.instance.sceneManagement.QuitGame();
        }
    }
}


