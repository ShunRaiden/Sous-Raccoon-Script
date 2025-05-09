using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Setting
{
    public class QualitySetting : MonoBehaviour
    {
        public TMP_Text qualityText;
        private string[] qualityLevels;
        private int currentQualityIndex;

        [SerializeField] Button plusButton;
        [SerializeField] Button minusButton;

        public void Init()
        {
            Dispose();

            qualityLevels = QualitySettings.names;

            // อ่านค่าคุณภาพปัจจุบันจาก QualitySettings
            currentQualityIndex = QualitySettings.GetQualityLevel();

            UpdateQualityText();

            plusButton.onClick.AddListener(IncreaseQuality);
            minusButton.onClick.AddListener(DecreaseQuality);
        }


        public void Dispose()
        {
            plusButton.onClick.RemoveAllListeners();
            minusButton.onClick.RemoveAllListeners();
        }

        public void IncreaseQuality()
        {
            if (currentQualityIndex < qualityLevels.Length - 1)
            {
                currentQualityIndex++;
                ApplyQuality();
            }
        }

        public void DecreaseQuality()
        {
            if (currentQualityIndex > 0)
            {
                currentQualityIndex--;
                ApplyQuality();
            }
        }

        private void ApplyQuality()
        {
            QualitySettings.SetQualityLevel(currentQualityIndex, true); // ปรับระดับ Quality และใช้การตั้งค่าใหม่ทันที
            UpdateQualityText();
        }

        private void UpdateQualityText()
        {
            qualityText.text = qualityLevels[currentQualityIndex];
        }
    }
}