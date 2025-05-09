using UnityEngine;
using UnityEngine.InputSystem;

namespace SousRaccoon.CameraMove
{
    public class LobbyCameraZoom : CameraZoom
    {
        private const string REBINE_KEY = "rebinds";

        PlayerInput playerInput;
        Player.PlayerInputManager playerInputManager;

        [Header("Zoom")]
        public GameObject startCam;

        public float newSpeedZoomIn;
        public float newSpeedOffset;

        public Vector3 zoomOffset; // กำหนดค่า Y และ Z สำหรับการ Zoom

        public Vector2 newlimitXOffset;
        public Vector2 newlimitZOffset;

        [Header("Reset")]
        public float newSpeedZoomOut;
        public float defaultSpeedOffset;

        public Vector3 defaultOffset;

        public Vector2 defaultlimitXOffset;
        public Vector2 defaultlimitZOffset;


        [SerializeField] private CameraMovement cameraMovement;

        bool isZoom;

        private void OnEnable()
        {
            if (playerInput == null)
            {
                playerInput = new PlayerInput();

                LoadRebinds();

                // Zoom
                playerInput.PlayerMovement.Dancing.started += i => HandleDance();

                // Reset
                playerInput.PlayerMovement.Movement.performed += i => ResetZoom();
                playerInput.PlayerMovement.Jump.started += i => ResetZoom();
                playerInput.PlayerMovement.Rolling.started += i => ResetZoom();
                playerInput.PlayerMovement.Attack.started += i => ResetZoom();
                playerInput.PlayerMovement.Interact.started += i => ResetZoom();
                playerInput.PlayerMovement.Interact.canceled += i => ResetZoom();
            }

            playerInput.Enable();
        }

        private void OnDisable()
        {
            playerInput.Disable();
        }

        public void LoadRebinds()
        {
            if (PlayerPrefs.HasKey(REBINE_KEY))
            {
                string rebinds = PlayerPrefs.GetString(REBINE_KEY);
                playerInput.LoadBindingOverridesFromJson(rebinds);
            }
        }

        private void HandleDance()
        {
            playerInputManager = FindObjectOfType<Player.PlayerInputManager>();

            if (!isZoom && !playerInputManager.isPauseGame && !startCam.activeSelf)
            {
                isZoom = true;
                smoothSpeed = newSpeedZoomIn;
                StartZoom(zoomOffset);
                cameraMovement.smoothSpeed = newSpeedOffset;
                cameraMovement.SetLimit(newlimitXOffset, newlimitZOffset);
            }

        }

        private void ResetZoom()
        {
            if (isZoom && !playerInputManager.isPauseGame && !startCam.activeSelf)
            {
                isZoom = false;
                smoothSpeed = newSpeedZoomOut;
                StartZoom(defaultOffset);
                cameraMovement.smoothSpeed = defaultSpeedOffset;
                cameraMovement.SetLimit(defaultlimitXOffset, defaultlimitZOffset);
            }
        }
    }
}
