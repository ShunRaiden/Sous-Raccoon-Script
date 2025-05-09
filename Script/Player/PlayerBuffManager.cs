using SousRaccoon.Data.Item;
using SousRaccoon.Kitchen;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Player
{
    public class PlayerBuffManager : MonoBehaviour
    {
        private PlayerCombatSystem combatSystem;
        private PlayerHealingDanceSystem healingDanceSystem;
        private PlayerLocomotion locomotion;
        private PlayerKitchenAction kitchenAction;

        private UseItemGenerator useItemGenerator;

        // Dictionary ����Ѻ������ Buff ��������
        private Dictionary<UseItemSO.BuffType, float> activeBuffs = new Dictionary<UseItemSO.BuffType, float>();

        // Dictionary ����Ѻ�� MaxTime �ͧ Buff
        private Dictionary<UseItemSO.BuffType, float> maxBuffTimes = new Dictionary<UseItemSO.BuffType, float>();

        private void Start()
        {
            combatSystem = GetComponent<PlayerCombatSystem>();
            healingDanceSystem = GetComponent<PlayerHealingDanceSystem>();
            locomotion = GetComponent<PlayerLocomotion>();
            kitchenAction = GetComponent<PlayerKitchenAction>();

            useItemGenerator = FindAnyObjectByType<UseItemGenerator>();
        }

        private void Update()
        {
            List<UseItemSO.BuffType> expiredBuffs = new List<UseItemSO.BuffType>();

            foreach (var buffType in new List<UseItemSO.BuffType>(activeBuffs.Keys))
            {
                activeBuffs[buffType] -= Time.deltaTime;

                if (maxBuffTimes.ContainsKey(buffType))
                {
                    useItemGenerator.OutPutBuff(Mathf.Clamp01(activeBuffs[buffType] / maxBuffTimes[buffType]));
                }

                // ����� Buff �������
                if (activeBuffs[buffType] <= 0)
                {
                    if (!expiredBuffs.Contains(buffType))  // ��ͧ�ѹ������¡ FinishBuff ���
                    {
                        expiredBuffs.Add(buffType);
                    }
                }
            }

            foreach (var buffType in expiredBuffs)
            {
                ResetStats(buffType);
                activeBuffs.Remove(buffType);
                maxBuffTimes.Remove(buffType);
            }
        }

        public void ApplyBuff(UseItemSO.BuffType buffType, float duration, int buffIntValue = 0, float buffFloatValue = 0f)
        {
            // ź Buff �Ѩ�غѹ�������ʶԵԡ�͹������ Buff ����
            if (activeBuffs.Count > 0)
            {
                var existingBuffType = new List<UseItemSO.BuffType>(activeBuffs.Keys)[0];
                ResetStats(existingBuffType);
                activeBuffs.Clear();
                maxBuffTimes.Clear();
            }

            // ���� Buff ����� Dictionary
            activeBuffs[buffType] = duration;
            maxBuffTimes[buffType] = duration;

            useItemGenerator.SpawnBuff();

            // ��駤�� Buff ���������
            switch (buffType)
            {
                case UseItemSO.BuffType.Speed:
                    locomotion.SetSpeedBuff(buffFloatValue);
                    break;
                case UseItemSO.BuffType.AttackSpeed:
                    combatSystem.SetAttackSpeedBuff(buffFloatValue);
                    break;
                case UseItemSO.BuffType.AttackDamage:
                    combatSystem.SetAttackDamageBuff(buffIntValue);
                    break;
                case UseItemSO.BuffType.HealRange:
                    healingDanceSystem.SetHealRangeBuff(buffFloatValue);
                    break;
                case UseItemSO.BuffType.HealRate:
                    healingDanceSystem.SetHealRateBuff(buffFloatValue);
                    break;
            }
        }



        private void ResetStats(UseItemSO.BuffType buffType)
        {
            useItemGenerator.FinishBuff();

            // Reset ��� Buff ����Ѻ���ʶҹо�鹰ҹ
            switch (buffType)
            {
                case UseItemSO.BuffType.Speed:
                    locomotion.ResetSpeed();
                    break;
                case UseItemSO.BuffType.AttackSpeed:
                    combatSystem.ResetAttackSpeed();
                    break;
                case UseItemSO.BuffType.AttackDamage:
                    combatSystem.ResetAttackDamage();
                    break;
                case UseItemSO.BuffType.HealRange:
                    healingDanceSystem.ResetHealRange();
                    break;
                case UseItemSO.BuffType.HealRate:
                    healingDanceSystem.ResetHealRate();
                    break;
            }
        }
    }
}
