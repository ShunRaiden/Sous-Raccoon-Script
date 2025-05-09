using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Setting
{
    public class ResolutionSetting : MonoBehaviour
    {
        public TMP_Text resolutionText;
        private Resolution[] customResolutions;
        private int currentResolutionIndex = 0;

        [SerializeField] Button plusButton;
        [SerializeField] Button minusButton;

        public void Init()
        {
            Dispose();

            // กำหนดความละเอียดที่ตายตัว
            customResolutions = new Resolution[]
            {
                new Resolution { width = 1024, height = 576 },
                new Resolution { width = 1280, height = 720 },
                new Resolution { width = 1366, height = 768 },
                new Resolution { width = 1600, height = 900 },
                new Resolution { width = 1920, height = 1080 },
            };

            currentResolutionIndex = FindCurrentResolutionIndex();
            UpdateResolutionText();

            plusButton.onClick.AddListener(IncreaseResolution);
            minusButton.onClick.AddListener(DecreaseResolution);
        }

        public void Dispose()
        {
            plusButton.onClick.RemoveListener(IncreaseResolution);
            minusButton.onClick.RemoveListener(DecreaseResolution);
        }

        public void IncreaseResolution()
        {
            if (currentResolutionIndex < customResolutions.Length - 1)
            {
                currentResolutionIndex++;
                ApplyResolution();
            }
        }

        public void DecreaseResolution()
        {
            if (currentResolutionIndex > 0)
            {
                currentResolutionIndex--;
                ApplyResolution();
            }
        }

        private void ApplyResolution()
        {
            Resolution resolution = customResolutions[currentResolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
            UpdateResolutionText();
        }

        private void UpdateResolutionText()
        {
            resolutionText.text = customResolutions[currentResolutionIndex].width + "x" + customResolutions[currentResolutionIndex].height;
        }

        private int FindCurrentResolutionIndex()
        {
            for (int i = 0; i < customResolutions.Length; i++)
            {
                if (customResolutions[i].width == Screen.width && customResolutions[i].height == Screen.height)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
