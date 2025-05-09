using SousRaccoon.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Manager
{
    public static class SaveManager
    {
        private const int TotalSaves = 5;
        private static Dictionary<int, PlayerSaveData> saveSlots = new Dictionary<int, PlayerSaveData>();
        public const int GAME_VERSION = 64;
        public const string GAME_VERSION_TEXT = "Version 0.6.4 Early Access";

        static SaveManager()
        {
            for (int i = 1; i <= TotalSaves; i++)
            {
                saveSlots[i] = new PlayerSaveData();
            }
        }

        public static void SaveData(int slot, PlayerSaveData data)
        {
            if (data == null) return; // ตรวจสอบว่าข้อมูลไม่เป็น null

            PlayerPrefs.SetInt($"GAME_VERSION_{slot}", GAME_VERSION);
            PlayerPrefs.SetInt($"PLAYER_LEVEL_SPEED_SAVE_{slot}", data.LevelSpeed);
            PlayerPrefs.SetInt($"PLAYER_LEVEL_ROLL_COOLDOWN_SAVE_{slot}", data.LevelRollCooldown);
            PlayerPrefs.SetInt($"PLAYER_LEVEL_COMBAT_SAVE_{slot}", data.LevelCombat);
            PlayerPrefs.SetInt($"PLAYER_LEVEL_HEAL_SAVE_{slot}", data.LevelHeal);
            PlayerPrefs.SetInt($"PLAYER_LEVEL_CUSTOMER_SAVE_{slot}", data.LevelCustomer);
            PlayerPrefs.SetInt($"PLAYER_SKIN_SAVE_{slot}", data.Skin);
            PlayerPrefs.SetInt($"PLAYER_MONEY_CURRENCY_SAVE_{slot}", data.MoneyCurrency);
            PlayerPrefs.SetInt($"PLAYER_STATE_UNLOCK_SAVE_{slot}", data.StageUnlock);

            PlayerPrefs.SetInt($"RUN_COMPLETE_{slot}", data.RunComplete);

            DateTime currentTime = DateTime.Now;
            data.LastTimeSave = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            PlayerPrefs.SetString($"PLAYER_LAST_TIME_SAVE_{slot}", data.LastTimeSave);

            // ºÑ¹·Ö¡¢éÍÁÙÅÊ¡Ô¹áÅÐ¢Í§µ¡áµè§·Õè»Å´ÅçÍ¤à»ç¹ JSON
            PlayerPrefs.SetString($"PLAYER_UNLOCKED_SKINS_{slot}", JsonUtility.ToJson(new IntListWrapper { list = data.UnlockedSkins }));
            PlayerPrefs.SetString($"PLAYER_UNLOCKED_DECORATIONS_{slot}", JsonUtility.ToJson(new IntListWrapper { list = data.UnlockedDecorations }));

            string skinsJson = PlayerPrefs.GetString($"PLAYER_UNLOCKED_SKINS_{slot}", "{\"list\":[]}");
            IntListWrapper skinsWrapper = JsonUtility.FromJson<IntListWrapper>(skinsJson);
            data.UnlockedSkins = skinsWrapper?.list ?? new List<bool>();

            // Ensure index 0 is always unlocked
            if (data.UnlockedSkins.Count == 0)
            {
                data.UnlockedSkins.Add(true); // ถ้ายังไม่มีเลย ใส่อันแรกเป็น true
            }
            else
            {
                data.UnlockedSkins[0] = true; // ถ้ามีแล้ว ให้ index 0 เป็น true เสมอ
            }

            PlayerPrefs.Save();
        }

        public static PlayerSaveData LoadData(int slot)
        {
            if (!PlayerPrefs.HasKey($"PLAYER_SKIN_SAVE_{slot}")) return null;

            if (!PlayerPrefs.HasKey($"GAME_VERSION_{slot}"))
            {
                PlayerPrefs.SetInt($"GAME_VERSION_{slot}", GAME_VERSION);
            }

            PlayerSaveData data = new PlayerSaveData
            {
                GameVersion = PlayerPrefs.GetInt($"GAME_VERSION_{slot}"),
                LevelSpeed = PlayerPrefs.GetInt($"PLAYER_LEVEL_SPEED_SAVE_{slot}"),
                LevelRollCooldown = PlayerPrefs.GetInt($"PLAYER_LEVEL_ROLL_COOLDOWN_SAVE_{slot}"),
                LevelCombat = PlayerPrefs.GetInt($"PLAYER_LEVEL_COMBAT_SAVE_{slot}"),
                LevelHeal = PlayerPrefs.GetInt($"PLAYER_LEVEL_HEAL_SAVE_{slot}"),
                LevelCustomer = PlayerPrefs.GetInt($"PLAYER_LEVEL_CUSTOMER_SAVE_{slot}"),
                Skin = PlayerPrefs.GetInt($"PLAYER_SKIN_SAVE_{slot}"),
                MoneyCurrency = PlayerPrefs.GetInt($"PLAYER_MONEY_CURRENCY_SAVE_{slot}"),
                StageUnlock = PlayerPrefs.GetInt($"PLAYER_STATE_UNLOCK_SAVE_{slot}"),
                RunComplete = PlayerPrefs.GetInt($"RUN_COMPLETE_{slot}"),
                LastTimeSave = PlayerPrefs.GetString($"PLAYER_LAST_TIME_SAVE_{slot}")
            };

            if (GAME_VERSION <= data.GameVersion && data.StageUnlock > PlayerSaveData.MAX_STAGE)
            {
                data.StageUnlock = PlayerSaveData.MAX_STAGE;
            }

            // แปลง JSON เป็น List<bool> และตรวจสอบว่ามีข้อมูลหรือไม่
            string skinsJson = PlayerPrefs.GetString($"PLAYER_UNLOCKED_SKINS_{slot}", "{\"list\":[]}");
            IntListWrapper skinsWrapper = JsonUtility.FromJson<IntListWrapper>(skinsJson);
            data.UnlockedSkins = skinsWrapper?.list ?? new List<bool>();

            string decorationsJson = PlayerPrefs.GetString($"PLAYER_UNLOCKED_DECORATIONS_{slot}", "{\"list\":[]}");
            IntListWrapper decorationsWrapper = JsonUtility.FromJson<IntListWrapper>(decorationsJson);
            data.UnlockedDecorations = decorationsWrapper?.list ?? new List<bool>();

            return data;
        }

        public static void ClearSaveData(int slot)
        {
            PlayerPrefs.DeleteKey($"GAME_VERSION_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_LEVEL_SPEED_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_LEVEL_ROLL_COOLDOWN_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_LEVEL_COMBAT_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_LEVEL_HEAL_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_LEVEL_CUSTOMER_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_SKIN_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_MONEY_CURRENCY_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_STATE_UNLOCK_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_LAST_TIME_SAVE_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_UNLOCKED_SKINS_{slot}");
            PlayerPrefs.DeleteKey($"PLAYER_UNLOCKED_DECORATIONS_{slot}");
            PlayerPrefs.DeleteKey($"RUN_COMPLETE_{slot}");

            PlayerPrefs.Save();
        }
    }

    [System.Serializable]
    public class IntListWrapper
    {
        public List<bool> list;
    }
}
