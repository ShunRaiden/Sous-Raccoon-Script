using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Setting
{
    public class AntiAliasingSetting : MonoBehaviour
    {
        public TMP_Text antiAliasingText;
        private int[] antiAliasingLevels = { 0, 2, 4, 8 }; // 0 = ปิด, 2x, 4x, 8x
        private int currentAAIndex = 0;

        [SerializeField] Button plusButton;
        [SerializeField] Button minusButton;

        public void Init()
        {
            Dispose();

            // ค้นหาค่าปัจจุบันของ Anti-Aliasing ที่กำลังใช้งาน
            currentAAIndex = FindCurrentAAIndex();
            UpdateAAText();

            plusButton.onClick.AddListener(IncreaseAA);
            minusButton.onClick.AddListener(DecreaseAA);
        }

        public void Dispose()
        {
            plusButton.onClick.RemoveAllListeners();
            minusButton.onClick.RemoveAllListeners();
        }

        public void IncreaseAA()
        {
            if (currentAAIndex < antiAliasingLevels.Length - 1)
            {
                currentAAIndex++;
                ApplyAA();
            }
        }

        public void DecreaseAA()
        {
            if (currentAAIndex > 0)
            {
                currentAAIndex--;
                ApplyAA();
            }
        }

        private void ApplyAA()
        {
            QualitySettings.antiAliasing = antiAliasingLevels[currentAAIndex];
            UpdateAAText();
        }

        private void UpdateAAText()
        {
            antiAliasingText.text = antiAliasingLevels[currentAAIndex] == 0 ? "Off" : antiAliasingLevels[currentAAIndex] + "x";
        }

        private int FindCurrentAAIndex()
        {
            int currentAA = QualitySettings.antiAliasing;
            for (int i = 0; i < antiAliasingLevels.Length; i++)
            {
                if (antiAliasingLevels[i] == currentAA)
                {
                    return i;
                }
            }
            return 0; // ถ้าไม่พบให้ใช้ค่าเริ่มต้นเป็นปิด
        }
    }
}
