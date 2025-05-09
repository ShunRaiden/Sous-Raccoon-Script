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

        // ���� List ����Ѻ��ʡԹ��Тͧ���觷��Ŵ��ͤ
        public List<bool> UnlockedSkins = new List<bool>();
        public List<bool> UnlockedDecorations = new List<bool>();

        public void AddDataSkins(int skinCount)
        {
            while (UnlockedSkins.Count < skinCount)
            {
                UnlockedSkins.Add(false); // ��� false ���֧�ӹǹ����ͧ���
            }

            UnlockedSkins[0] = true; // ��������� ��� index 0 �� true ����
        }

        public void AddDecrationSkin(int decorationCount)
        {
            while (UnlockedDecorations.Count < decorationCount)
            {
                UnlockedDecorations.Add(false); // ��� false ���֧�ӹǹ����ͧ���
            }

            UnlockedDecorations[0] = true; // ��������� ��� index 0 �� true ����
        }
    }
}
