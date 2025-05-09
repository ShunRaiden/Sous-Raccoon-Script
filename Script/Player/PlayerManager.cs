using SousRaccoon.Data;
using SousRaccoon.Manager;
using UnityEngine;

namespace SousRaccoon.Player
{
    public class PlayerManager : MonoBehaviour
    {
        PlayerHealingDanceSystem healingDanceSystem;
        PlayerAnimatorManager animatorManager;
        PlayerLocomotion playerLocomotion;
        PlayerInputManager inputManager;
        PlayerCombatSystem combatSystem;

        public bool isInteracting;
        public bool isJumping;
        public bool isRolling;

        // Player Status
        public float PlayerSpeed { get; private set; }
        public float PlayerRollCooldown { get; private set; }
        public int PlayerDamage { get; private set; }
        public float PlayerAttackSpeed { get; private set; }
        public float PlayerHealRate { get; private set; }
        public float PlayerHealRange { get; private set; }
        public int PlayerStunRate { get; private set; }
        public float PlayerStunTimeMax { get; private set; }

        private void Awake()
        {
            healingDanceSystem = GetComponent<PlayerHealingDanceSystem>();
            animatorManager = GetComponent<PlayerAnimatorManager>();
            playerLocomotion = GetComponent<PlayerLocomotion>();
            inputManager = GetComponent<PlayerInputManager>();
            combatSystem = GetComponent<PlayerCombatSystem>();

            LoadPlayerStatus();
        }

        private void Update()
        {
            inputManager.HandleAllInput();
        }

        private void FixedUpdate()
        {
            playerLocomotion.HandleAllMovement();
        }

        private void LateUpdate()
        {
            isInteracting = animatorManager.animator.GetBool("isInteracting");
            isJumping = animatorManager.animator.GetBool("isJumping");
            isRolling = animatorManager.animator.GetBool("isRolling");
            animatorManager.animator.SetBool("isGrounded", playerLocomotion.isGrounded);
        }

        private void LoadPlayerStatus()
        {
            PlayerSaveData levelData = GameManager.instance.playerSaveData; //Load Level

            PlayerDataBase loadedData = GameManager.instance.playerDataBase; //Load Data of Level

            PlayerSpeed = loadedData.defaultPlayerSpeed.Speed + loadedData.playerSpeed[levelData.LevelSpeed].Speed;

            PlayerRollCooldown = loadedData.defaultPlayerRollCooldown.RollCooldown - loadedData.playerRollCooldown[levelData.LevelRollCooldown].RollCooldown;

            PlayerAttackSpeed = loadedData.defaultPlayerCombat.AttackSpeed + loadedData.playerCombat[levelData.LevelCombat].AttackSpeed;
            PlayerStunRate = loadedData.defaultPlayerCombat.StunRate + loadedData.playerCombat[levelData.LevelCombat].StunRate;
            PlayerStunTimeMax = loadedData.defaultPlayerCombat.StunMaxTime - loadedData.playerCombat[levelData.LevelCombat].StunMaxTime;
            PlayerDamage = loadedData.defaultPlayerCombat.Damage + loadedData.playerCombat[levelData.LevelCombat].Damage;

            PlayerHealRange = loadedData.defaultPlayerHealing.HealRange + loadedData.playerHealing[levelData.LevelHeal].HealRange;
            PlayerHealRate = loadedData.defaultPlayerHealing.HealRate + loadedData.playerHealing[levelData.LevelHeal].HealRate;

            playerLocomotion.movementSpeed = PlayerSpeed;
            playerLocomotion.currentSpeed = PlayerSpeed;

            playerLocomotion.rollCooldown = PlayerRollCooldown;

            combatSystem.stunRate = PlayerStunRate;
            combatSystem.stunTimeMax = PlayerStunTimeMax;
            combatSystem.attackSpeed = PlayerAttackSpeed;
            combatSystem.playerDamageToMonster = PlayerDamage;

            healingDanceSystem.healingRate = PlayerHealRate;
            healingDanceSystem.healingRadius = PlayerHealRange;
        }
    }
}

