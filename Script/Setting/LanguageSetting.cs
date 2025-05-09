using SousRaccoon.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace SousRaccoon.Setting
{
    public class LanguageSetting : MonoBehaviour
    {
        public TMP_Text languageText; // Text ที่จะแสดงชื่อภาษา
        private int languageIndex = 0; // Index ของภาษาปัจจุบัน
        private const string LanguagePrefKey = "SelectedLanguage"; // Key สำหรับเก็บค่าภาษาใน PlayerPrefs

        [SerializeField] Button plusButton;
        [SerializeField] Button minusButton;

        private void Start()
        {
            // โหลดค่าภาษาที่เคยบันทึกไว้
            languageIndex = PlayerPrefs.GetInt(LanguagePrefKey, GetCurrentLanguageIndex());

            UpdateLanguage();
        }

        public void Init()
        {
            Dispose();

            // โหลดค่าภาษาที่เคยบันทึกไว้
            languageIndex = PlayerPrefs.GetInt(LanguagePrefKey, GetCurrentLanguageIndex());

            UpdateLanguage();

            plusButton.onClick.AddListener(NextLanguage);
            minusButton.onClick.AddListener(PreviousLanguage);
        }

        public void Dispose()
        {
            plusButton.onClick.RemoveListener(NextLanguage);
            minusButton.onClick.RemoveListener(PreviousLanguage);
        }

        // ฟังก์ชันสำหรับเปลี่ยนไปภาษาถัดไป
        public void NextLanguage()
        {
            languageIndex = (languageIndex + 1) % LocalizationSettings.AvailableLocales.Locales.Count;
            UpdateLanguage();
        }

        // ฟังก์ชันสำหรับย้อนกลับไปภาษาก่อนหน้า
        public void PreviousLanguage()
        {
            languageIndex--;
            if (languageIndex < 0)
            {
                languageIndex = LocalizationSettings.AvailableLocales.Locales.Count - 1;
            }
            UpdateLanguage();
        }

        private void UpdateLanguage()
        {
            var selectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
            LocalizationSettings.SelectedLocale = selectedLocale;

            languageText.text = selectedLocale.LocaleName; // อัปเดต UI

            // บันทึกค่าภาษาไว้ใน PlayerPrefs
            PlayerPrefs.SetInt(LanguagePrefKey, languageIndex);
            PlayerPrefs.Save();

            GameManager.instance.OnUpdateLanguage();
        }

        private int GetCurrentLanguageIndex()
        {
            var currentLocale = LocalizationSettings.SelectedLocale;
            return LocalizationSettings.AvailableLocales.Locales.IndexOf(currentLocale);
        }
    }
}
