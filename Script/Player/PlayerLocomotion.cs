using SousRaccoon.CameraMove;
using UnityEngine;

namespace SousRaccoon.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerLocomotion : MonoBehaviour
    {
        PlayerAnimatorManager playerAnimatorManager;
        PlayerAudioManager playerAudioManager;
        PlayerInputManager inputManager;
        PlayerManager playerManager;
        PlayerVFX playerVFX;

        public CharacterController characterController;

        [SerializeField] Transform cameraObject;

        Vector3 moveDirection;
        Vector3 playerVelocity;

        [Header("Control")]
        public bool isAirMovement;

        [Header("Movement Flags")]
        public bool isGrounded;
        public bool isJumping;
        public bool isRolling = false;

        [Header("Movement Speed")]
        public float movementSpeed = 7;
        public float movementSpeedRunStage;
        public float currentSpeed;
        //public float movementCombatSpeed = 5;
        public float rotationSpeed = 15;
        public float slopeSpeedMultiplier = 1.5f; // New parameter to adjust speed on slopes

        [Header("Rolling Settings")]
        public float runStageRollSpeed;
        [SerializeField] private float rollSpeed = 20f; // Speed for the dash
        [SerializeField] private float rollDuration = 0.2f; // Duration of the dash
        public float rollCooldown = 2f; // Cooldown period for the dash
        public float rollCooldownRunStage;
        private float rollTimer = 0f;
        private float rollCooldownTimer = 0f;

        [Header("Falling")]
        public float inAirTimer;
        public float gravityIntensity = -15;
        public float rayCastHeightOffset;
        public float maxDistance;
        public float radiusRaySize;
        public LayerMask groundLayer;

        [Header("Jump Speeds")]
        public float jumpHeight = 3;

        bool vfxJumpCheck = true;

        private void Awake()
        {
            var camMove = FindAnyObjectByType<CameraMovement>();
            camMove.playerTransform = gameObject.transform;
            camMove.targetTransform = gameObject.transform;
            cameraObject = camMove.transform;

            playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            characterController = GetComponent<CharacterController>();
            playerAudioManager = GetComponent<PlayerAudioManager>();
            inputManager = GetComponent<PlayerInputManager>();
            playerManager = GetComponent<PlayerManager>();
            playerVFX = GetComponent<PlayerVFX>();
        }

        public void HandleAllMovement()
        {
            if (isRolling)
            {
                RollMovement();
            }

            // Reduce cooldown timer
            if (rollCooldownTimer > 0)
            {
                rollCooldownTimer -= Time.deltaTime;
            }

            HandleFallingAndLanding();

            if (!isGrounded && !isAirMovement) return;

            HandleMovement();
            HandleRotation();
        }

        private void HandleMovement()
        {
            if (inputManager.isStunning || !inputManager.isCanInput || inputManager.cantMoveStage)
            {
                StopMove();
                return;
            }

            // ¤Ó¹Ç³·ÔÈ·Ò§¡ÒÃà¤Å×èÍ¹·ÕèµÒÁ¡ÒÃËÁØ¹¢Í§¡ÅéÍ§áÅÐ¡ÒÃ»éÍ¹¢éÍÁÙÅ¨Ò¡¼ÙéàÅè¹
            moveDirection = cameraObject.forward * inputManager.verticalInput;
            moveDirection += cameraObject.right * inputManager.horizontalInput;
            moveDirection.Normalize();

            // ÃÑº¢éÍÁÙÅ normal ¢Í§¾×é¹
            Vector3 groundNormal = GetGroundNormal();

            // »ÃÑº¤ÇÒÁàÃçÇ¡ÒÃà¤Å×èÍ¹·ÕèµÒÁÁØÁ¢Í§ÅÒ´
            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            float adjustedSpeed = currentSpeed + movementSpeedRunStage;

            /*if (inputManager.isUseWeapon) // If Use Weapon Change to movementCombatSpeed
                adjustedSpeed = movementCombatSpeed;*/

            if (slopeAngle > characterController.slopeLimit) // ¶éÒ¨Ó¹Ç¹ÁØÁÅÒ´à¡Ô¹¡ÇèÒ¢Õ´¨Ó¡Ñ´
            {
                adjustedSpeed *= slopeSpeedMultiplier; // à¾ÔèÁ¤ÇÒÁàÃçÇàÁ×èÍÍÂÙèº¹ÅÒ´
            }

            moveDirection *= adjustedSpeed; // ¤Ù³´éÇÂ¤ÇÒÁàÃçÇ·Õè»ÃÑºÊÓËÃÑº¡ÒÃà¤Å×èÍ¹·Õèº¹ÅÒ´

            // ãªéáÃ§â¹éÁ¶èÇ§
            if (isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f; // ÃÕà«çµ Y velocity ¶éÒ·ÓãËéµÔ´¾×é¹
            }

            // à¤Å×èÍ¹·ÕèÍÂèÒ§ÃÒºÃ×è¹º¹ÅÒ´â´Â¡ÒÃ»ÃÑº¤ÇÒÁàÃçÇãËéÊÍ´¤ÅéÍ§¡Ñº¾×é¹
            if (isGrounded)
            {
                Vector3 alignedMoveDirection = Vector3.ProjectOnPlane(moveDirection, groundNormal);
                characterController.Move(alignedMoveDirection * Time.deltaTime);
            }
            else
            {
                characterController.Move(moveDirection * Time.deltaTime);
            }

            // ãªéáÃ§â¹éÁ¶èÇ§´éÇÂµ¹àÍ§
            playerVelocity.y += gravityIntensity * Time.deltaTime;
            characterController.Move(playerVelocity * Time.deltaTime);

            if (inputManager.moveAmount != 0 && isGrounded)
            {
                playerVFX.OnWalkVFX();
            }
            else
            {
                playerVFX.OnStopWalkVFX();
            }
        }

        public void StopMove()
        {
            playerVelocity = Vector3.zero;
            playerAudioManager.StopRunSFX();
            playerVFX.OnStopWalkVFX();
        }

        private void HandleRotation()
        {
            Vector3 targetDirection = Vector3.zero;

            targetDirection = cameraObject.forward * inputManager.verticalInput;
            targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
            targetDirection.Normalize();
            targetDirection.y = 0;

            if (targetDirection == Vector3.zero)
            {
                targetDirection = transform.forward;
            }

            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            transform.rotation = playerRotation;
        }

        private void HandleFallingAndLanding()
        {
            RaycastHit hit;
            Vector3 rayCastOrigin = transform.position + Vector3.up * rayCastHeightOffset;

            if (!isGrounded && !isJumping)
            {
                inAirTimer += Time.deltaTime;
            }

            if (Physics.SphereCast(rayCastOrigin, radiusRaySize, Vector3.down, out hit, maxDistance, groundLayer))
            {
                inAirTimer = 0;
                isGrounded = true;
                playerVFX.SetMarker(isGrounded);

                if (!vfxJumpCheck)
                {
                    vfxJumpCheck = true;
                    playerVFX.SpawnLandingDust();
                }

                // Adjust character position to avoid floating
                float groundDistance = hit.distance;
                if (groundDistance > 0.1f) // Allow a small buffer
                {
                    characterController.Move(Vector3.down * (groundDistance - 0.1f));
                }
            }
            else
            {
                isGrounded = false;
                vfxJumpCheck = false;
                playerVFX.SetMarker(isGrounded);
            }
        }

        private Vector3 GetGroundNormal()
        {
            RaycastHit hit;
            Vector3 rayCastOrigin = transform.position + Vector3.up * rayCastHeightOffset; // Adjust origin to avoid ground clipping

            if (Physics.SphereCast(rayCastOrigin, .2f, Vector3.down, out hit, maxDistance, groundLayer))
            {
                return hit.normal;
            }

            return Vector3.up; // Default normal if no ground is detected
        }

        public void HandleJumping()
        {
            if (isGrounded)
            {
                if (!isRolling)
                {
                    playerAnimatorManager.Jump();
                }

                float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
                playerVelocity.y = jumpingVelocity;
                characterController.Move(playerVelocity * Time.deltaTime);

                playerAudioManager.PlayJumpSFX();
            }
        }

        public void HandleRolling()
        {
            // Check if the character is grounded and not currently dashing or on cooldown
            if (!isGrounded || isRolling || rollCooldownTimer > 0) return;

            // Start the dash
            isRolling = true;
            //isCanMove = false;

            playerAudioManager.PlayRollingSFX();

            rollTimer = rollDuration;
            rollCooldownTimer = rollCooldown - rollCooldownRunStage;
            playerAnimatorManager.Rolling();
        }

        private void RollMovement()
        {
            if (rollTimer > 0)
            {
                // Move the character forward
                Vector3 dashDirection = transform.forward * (rollSpeed + runStageRollSpeed);
                characterController.Move(dashDirection * Time.deltaTime);
                rollTimer -= Time.deltaTime;
            }
            else
            {
                // End dash
                isRolling = false;
                //isCanMove = true;
            }

            playerAnimatorManager.SetBoolRolling(isRolling);
        }

        public void SetSpeedBuff(float buffValue)
        {
            currentSpeed = movementSpeed + buffValue;
        }

        public void ResetSpeed()
        {
            currentSpeed = movementSpeed;
        }

        public void SetPlayerPosition(Transform newTransform)
        {
            characterController.enabled = false; // ปิด CharacterController ก่อนเพื่อให้เราสามารถตั้งค่าตำแหน่งโดยไม่ชนกับฟิสิกส์
            transform.position = newTransform.position;
            transform.rotation = newTransform.rotation;
            characterController.enabled = true; // เปิด CharacterController กลับมา
        }
    }
}
