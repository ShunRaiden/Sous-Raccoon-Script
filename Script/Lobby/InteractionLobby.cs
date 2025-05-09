using SousRaccoon.Manager;
using SousRaccoon.Player;
using System.Collections;
using UnityEngine;

namespace SousRaccoon.Lobby
{
    public class InteractionLobby : MonoBehaviour
    {
        public string nameInteract;
        public bool isStartInteraction;
        public bool canExit = false;

        [SerializeField] float startAnimationTiming;
        [SerializeField] float endAnimationTiming;
        [SerializeField] float fadeCamTiming;

        [SerializeField] GameObject fadePanel;

        [SerializeField] GameObject camLock;
        [SerializeField] MirrorCam mirrorCam;

        [SerializeField] Transform pivot;

        [SerializeField] bool hasLoop;

        PlayerVFX playerVFX;
        PlayerLocomotion playerLocomotion;
        PlayerAnimatorManager playerAnimatorManager;
        Player.PlayerInputManager playerInputManager;
        HighlightObject highlight;

        private void Start()
        {
            highlight = GetComponent<HighlightObject>();
        }

        private void Update()
        {
            if (canExit && Input.anyKey && hasLoop)
            {
                OnExitInteraction();
            }
        }

        public void OnStartInteraction()
        {
            if (playerInputManager == null)
                playerInputManager = FindObjectOfType<Player.PlayerInputManager>();

            if (playerLocomotion == null)
                playerLocomotion = FindObjectOfType<PlayerLocomotion>();

            if (playerAnimatorManager == null)
                playerAnimatorManager = FindObjectOfType<PlayerAnimatorManager>();

            if (playerVFX == null)
                playerVFX = FindObjectOfType<PlayerVFX>();

            StartCoroutine(StartingInteraction());
        }

        public void OnExitInteraction()
        {
            StartCoroutine(ExitingInteraction());
        }

        IEnumerator StartingInteraction()
        {
            AudioManager.instance.PlayOneShotSFX("GetItem");
            playerVFX.forcedHideMarker = true;
            playerVFX.raccoonMarker.SetActive(false);
            highlight.HideHighlight();
            playerInputManager.SetMoveStage(true);
            fadePanel.SetActive(true);
            playerInputManager.ResetAllInputs();
            playerInputManager.ResetAction();

            if (mirrorCam != null)
                mirrorCam.OnChangeTargerDir(camLock.transform);

            yield return new WaitForSeconds(fadeCamTiming);
            playerLocomotion.SetPlayerPosition(pivot);
            camLock.SetActive(true);
            playerAnimatorManager.PlayLobbyInteractionAnimation($"Start{nameInteract}", false);
            yield return new WaitForSeconds(startAnimationTiming);
            fadePanel.SetActive(false);
            canExit = true;

            if (!hasLoop)
            {
                fadePanel.SetActive(true);
                yield return new WaitForSeconds(fadeCamTiming);

                if (mirrorCam != null)
                    mirrorCam.OnResetCam();

                playerAnimatorManager.ExitInteractionAnimation();
                playerVFX.forcedHideMarker = false;
                playerVFX.raccoonMarker.SetActive(true);
                camLock.SetActive(false);
                fadePanel.SetActive(false);
                playerInputManager.ResetAllInputs();
                playerInputManager.SetMoveStage(false);
            }
        }

        IEnumerator ExitingInteraction()
        {
            canExit = false;
            playerAnimatorManager.PlayLobbyInteractionAnimation($"Exit{nameInteract}", true);
            yield return new WaitForSeconds(endAnimationTiming);
            fadePanel.SetActive(true);
            yield return new WaitForSeconds(fadeCamTiming);

            if (mirrorCam != null)
                mirrorCam.OnResetCam();

            playerVFX.forcedHideMarker = false;
            playerVFX.raccoonMarker.SetActive(true);
            camLock.SetActive(false);
            fadePanel.SetActive(false);
            playerInputManager.ResetAllInputs();
            playerInputManager.SetMoveStage(false);
        }
    }
}