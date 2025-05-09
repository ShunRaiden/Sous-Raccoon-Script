using System.Collections.Generic;

namespace SousRaccoon.Data
{
    [System.Serializable]
    public class PlayerSaveData
    {
        public int GameVersion;
        public const int MAX_STAGE = 2;

        public int LevelSpeed;
        public int LevelRollCooldown;
        public int LevelCombat;
        public int LevelHeal;
        public int LevelCustomer;
        public int Skin;
        public int MoneyCurrency;
        public int StageUnlock;
        public string LastTimeSave;

        public int RunComplete;

        // เพิ่ม List สำหรับเก็บสกินและของตกแต่งที่ปลดล็อค
        public List<bool> UnlockedSkins = new List<bool>();
        public List<bool> UnlockedDecorations = new List<bool>();

        public void AddDataSkins(int skinCount)
        {
            while (UnlockedSkins.Count < skinCount)
            {
                UnlockedSkins.Add(false); // เติม false จนถึงจำนวนที่ต้องการ
            }

            UnlockedSkins[0] = true; // ถ้ามีแล้ว ให้ index 0 เป็น true เสมอ
        }

        public void AddDecrationSkin(int decorationCount)
        {
            while (UnlockedDecorations.Count < decorationCount)
            {
                UnlockedDecorations.Add(false); // เติม false จนถึงจำนวนที่ต้องการ
            }

            UnlockedDecorations[0] = true; // ถ้ามีแล้ว ให้ index 0 เป็น true เสมอ
        }
    }
}
