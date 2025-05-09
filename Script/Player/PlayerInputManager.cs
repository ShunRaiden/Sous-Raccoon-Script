
using SousRaccoon.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SousRaccoon.Player
{
    public class PlayerInputManager : MonoBehaviour
    {
        private const string REBINE_KEY = "rebinds";

        PlayerAnimatorManager playerAnimatorManager;
        PlayerHealingDanceSystem playerHealingDance;
        PlayerKitchenAction playerKitchenAction;
        PlayerCombatSystem playerCombatSystem;
        PlayerAudioManager playerAudioManager;
        PlayerLocomotion playerLocomotion;
        PlayerVFX playerVFX;
        [HideInInspector] public PlayerInput playerInput;

        public Vector2 movementInput;
        public float moveAmount;
        public float verticalInput;
        public float horizontalInput;

        public bool jump_Input;
        public bool roll_Input;
        //public bool toggleWeapon_Input;
        public bool attack_Input;
        public bool interact_Input;
        public bool drop_Input;
        public bool dance_Input;
        public bool useItem_Input;

        public bool pause_Input;
        public bool info_Input;

        public bool isHealAction;
        public bool isInteractAction;
        public bool isUseWeapon = false;
        public bool isPauseGame = false;
        public bool cantMoveStage = false;
        public bool isCanInput = true;
        public bool isStunning = false;
        public bool isCutScene = false;

        public bool canInteract = true;

        public bool isInvisble = false;

        private void Awake()
        {
            playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            playerHealingDance = GetComponent<PlayerHealingDanceSystem>();
            playerKitchenAction = GetComponent<PlayerKitchenAction>();
            playerAudioManager = GetComponent<PlayerAudioManager>();
            playerCombatSystem = GetComponent<PlayerCombatSystem>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            playerVFX = GetComponent<PlayerVFX>();
        }

        private void OnEnable()
        {

            if (playerInput == null)
            {
                playerInput = new PlayerInput();
                LoadRebinds(); // โหลดค่าที่เคยเปลี่ยน
            }

            playerInput.PlayerGame.PauseGame.started += i => pause_Input = true;

            playerInput.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();

            playerInput.PlayerMovement.Jump.started += i => jump_Input = true;
            playerInput.PlayerMovement.Rolling.started += i => roll_Input = true;
            //playerInput.PlayerMovement.ToggleWeapon.performed += i => toggleWeapon_Input = true;
            playerInput.PlayerMovement.Attack.started += i => attack_Input = true;

            playerInput.PlayerMovement.Interact.started += i => interact_Input = true;
            playerInput.PlayerMovement.Interact.canceled += i => interact_Input = false;

            //playerInput.PlayerMovement.Drop.performed += i => drop_Input = true;
            playerInput.PlayerMovement.Dancing.started += i => dance_Input = true;

            playerInput.PlayerMovement.UseItem.started += i => useItem_Input = true;

            playerInput.PlayerMovement.OpenInfo.started += i => ToggleInfo();

            playerInput.PlayerMovement.SwapHand.started += i => HandleSwapHand();

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

        public void PlayerStopAllAction()
        {
            //Stop Move
            playerLocomotion.StopMove();
            //Stop Heal
            playerHealingDance.StopDanceHealing();
        }

        public void HandleAllInput()
        {
            if (!isCanInput) return;

            HandlePause();

            if (isPauseGame || isStunning || cantMoveStage) return;

            if (isCutScene)
            {
                StopAction();
                return;
            }

            HandleJumpingInput();
            HandleMovementInput();
            HandleRollInput();
            //HandleToggleWeapon();
            if (canInteract)
            {
                HandleInteract();
            }

            HandleAttack();
            //HandleDrop();
            HandleDance();
            HandleUseItem();
        }

        private void HandleMovementInput()
        {
            verticalInput = movementInput.y;
            horizontalInput = movementInput.x;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
            playerAnimatorManager.UpdateAnimatorValues(moveAmount);

            if (moveAmount != 0)
            {
                ResetAction();
            }

            PlayRunSound();
        }

        private void HandleJumpingInput()
        {
            if (jump_Input)
            {
                jump_Input = false;

                ResetAction();

                if (!isCutScene)
                    playerLocomotion.HandleJumping();

                if (TutorialManager.instance != null && TutorialManager.instance.isJumpCheck)
                {
                    TutorialManager.instance.jumpCheck = true;
                }
            }
        }

        private void HandleRollInput()
        {
            if (roll_Input)
            {
                roll_Input = false;

                if (playerCombatSystem.canRollDodge)
                {
                    isInvisble = true;
                }

                ResetAction();
                playerLocomotion.HandleRolling();

                if (TutorialManager.instance != null && TutorialManager.instance.isRollCheck)
                {
                    TutorialManager.instance.rollCheck = true;
                }
            }
        }

        /*private void HandleToggleWeapon()
        {
            if (toggleWeapon_Input)
            {
                toggleWeapon_Input = false;

                isUseWeapon = !isUseWeapon;
                ResetAction();
                playerAnimatorManager.SetToggleWeapon();
            }
        }*/

        private void HandleAttack()
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            if (attack_Input)
            {
                attack_Input = false;

                if (!playerAnimatorManager.isUseWeapon || playerCombatSystem.isAttackOnCooldown || playerLocomotion.isRolling) return;

                ResetAction();

                playerVFX.OnSlash();
                playerAudioManager.PlayRandomAttackSFX();
                playerCombatSystem.PerformConeAttack();
            }
        }

        private void HandleInteract()
        {
            if (interact_Input)
            {
                ResetAction();
                interact_Input = false;
                isInteractAction = true;
                playerKitchenAction.PerformInteraction();
            }
        }

        private void HandleSwapHand()
        {
            if (StageManager.instance != null && !StageManager.instance.isGamePaused)
            {
                if (playerKitchenAction.useTwoHand)
                    playerKitchenAction.PerformToggleHand();
            }
        }

        /*private void HandleDrop()
        {
            if (drop_Input)
            {
                drop_Input = false;
                playerKitchenAction.PerformGiveInteraction();
            }
        }*/

        private void HandleDance()
        {
            if (dance_Input)
            {
                ResetInteraction();
                dance_Input = false;
                isHealAction = true;

                playerHealingDance.PerformDanceHealing();
            }
        }

        private void HandleUseItem()
        {
            if (useItem_Input)
            {
                useItem_Input = false;
                isHealAction = false;
                //Paticle Use Item
                playerKitchenAction.PerformUseItem();
            }
        }

        private void HandlePause()
        {
            if (pause_Input)
            {
                pause_Input = false;

                var state = StageManager.instance;
                if (state != null)
                    state.TogglePause();

                var lobby = LobbyManager.instance;
                if (lobby != null)
                    lobby.TogglePause();

                // ล้าง Input ทั้งหมดเมื่อ Pause
                ResetAllInputs();
            }
        }

        // ฟังก์ชันสำหรับล้างค่าของ Input ทั้งหมด
        public void ResetAllInputs()
        {
            movementInput = Vector2.zero;
            moveAmount = 0;
            verticalInput = 0;
            horizontalInput = 0;
            playerAnimatorManager.StopMoveAnimation();
            playerAudioManager.ForceStopSunSFX();

            jump_Input = false;
            roll_Input = false;
            attack_Input = false;
            interact_Input = false;
            drop_Input = false;
            dance_Input = false;
            useItem_Input = false;

            HandleJumpingInput();
            HandleMovementInput();
            HandleRollInput();
            HandleInteract();
            HandleAttack();
            HandleDance();
            HandleUseItem();
        }

        public void SetMoveStage(bool condition)
        {
            cantMoveStage = condition;
            playerAnimatorManager.StopMoveAnimation();
        }

        private void PlayRunSound()
        {
            if (!playerLocomotion.isGrounded || playerLocomotion.isRolling)
            {
                playerAudioManager.StopRunSFX();
                return;
            }

            if (moveAmount != 0)
            {
                ResetAction();
                if (playerLocomotion.isGrounded)
                    playerAudioManager.PlayRunSFX();
            }
            else
            {
                playerAudioManager.StopRunSFX();
            }
        }

        private void ToggleInfo()
        {
            if (StageManager.instance != null && !StageManager.instance.isGamePaused)
            {
                StageManager.instance.ToggleInfo();
            }
        }

        public void ResetAction()
        {
            ResetHealAction();
            ResetInteraction();
        }

        public void ResetHealAction()
        {
            isHealAction = false;
            playerHealingDance.StopDanceHealing();
        }

        public void ResetInteraction()
        {
            isInteractAction = false;
            playerAnimatorManager.CleanUp(false);
        }

        private void StopAction()
        {
            ResetAllInputs();
            playerAnimatorManager.StopMoveAnimation();
            PlayRunSound();
        }
    }
}

