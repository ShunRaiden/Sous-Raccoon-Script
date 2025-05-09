using SousRaccoon.CameraMove;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Samples.RebindUI;

namespace SousRaccoon.Player
{
    public class PlayerSaveKeyBinds : MonoBehaviour
    {
        private const string REBINE_KEY = "rebinds";

        [SerializeField] PlayerInputManager _inputManager;
        [SerializeField] LobbyCameraZoom _lobbyInputManager;
        public InputActionAsset actions;

        private void Start()
        {
            LoadRebinds();
            RefreshRebindUI();
        }

        private void OnDisable()
        {
            SaveRebinds();
        }

        public void SaveRebinds()
        {
            string rebinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(REBINE_KEY, rebinds);
            PlayerPrefs.Save();

            _inputManager = FindObjectOfType<PlayerInputManager>();
            _lobbyInputManager = FindObjectOfType<LobbyCameraZoom>();

            if (_inputManager != null)
                _inputManager.LoadRebinds(); // Set ค่าให้ Player

            if (_lobbyInputManager != null)
                _lobbyInputManager.LoadRebinds(); // Set ค่าให้ Player
        }

        public void LoadRebinds()
        {
            if (PlayerPrefs.HasKey(REBINE_KEY))
            {
                string rebinds = PlayerPrefs.GetString(REBINE_KEY);
                actions.LoadBindingOverridesFromJson(rebinds);
            }
        }

        private void RefreshRebindUI()
        {
            foreach (var rebindUI in FindObjectsOfType<RebindActionUI>())
            {
                rebindUI.UpdateBindingDisplay();
            }
        }
    }
}