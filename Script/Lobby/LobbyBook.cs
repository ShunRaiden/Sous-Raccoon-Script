using SousRaccoon.Manager;
using SousRaccoon.UI.MainMenu;
using System.Collections;
using UnityEngine;

namespace SousRaccoon.Lobby
{
    public class LobbyBook : MonoBehaviour
    {
        [SerializeField] SelectionStageManager selectionStageManager;
        [SerializeField] UIUpgradeLevelPanel upgradeLevelPanel;
        [SerializeField] UIGuideBookPanel guideBookPanel;

        [SerializeField] Player.PlayerInputManager playerInputManager;

        bool toggle = false;

        Coroutine lobbyBookCoroutine;

        public bool isBookOpen = false;

        public void OpenLobbyPanel(Player.PlayerInputManager playerInput, bool isBook)
        {
            if (lobbyBookCoroutine != null) return;

            AudioManager.instance.PlayOneShotSFX("PaperSFX");

            playerInputManager = playerInput;

            if (playerInputManager != null)
            {
                playerInputManager.ResetAllInputs();
                playerInputManager.SetMoveStage(true);
            }

            if (isBook)
            {
                upgradeLevelPanel.SetupUpLevelPanel();
                toggle = true;
            }
            else
            {
                selectionStageManager.SetUpPanel();
                toggle = false;
            }

            isBookOpen = true;
        }

        public void TogglePanel()
        {
            if (toggle)
            {
                selectionStageManager.SetUpPanel();
                upgradeLevelPanel.ClosePanel();
                //guideBookPanel.ClosePanel();
                toggle = false;
            }
            else
            {
                upgradeLevelPanel.SetupUpLevelPanel();
                selectionStageManager.ClosePanel();
                //guideBookPanel.ClosePanel();
                toggle = true;
            }

            AudioManager.instance.PlayOneShotSFX("PaperSFX");
        }

        public void CloseLobbyPanel()
        {
            lobbyBookCoroutine = StartCoroutine(OnCloseLobbyPanel());
            AudioManager.instance.PlayOneShotSFX("PaperSFX");
        }

        IEnumerator OnCloseLobbyPanel()
        {
            selectionStageManager.ClosePanel();
            upgradeLevelPanel.ClosePanel();
            isBookOpen = false;

            yield return new WaitForSeconds(0.66f);

            if (playerInputManager != null)
                playerInputManager.SetMoveStage(false);

            lobbyBookCoroutine = null;
        }
    }
}