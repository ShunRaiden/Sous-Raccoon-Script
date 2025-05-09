using SousRaccoon.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace SousRaccoon.Setting
{
    public class LanguageSetting : MonoBehaviour
    {
        public TMP_Text languageText; // Text �����ʴ���������
        private int languageIndex = 0; // Index �ͧ���һѨ�غѹ
        private const string LanguagePrefKey = "SelectedLanguage"; // Key ����Ѻ�纤������� PlayerPrefs

        [SerializeField] Button plusButton;
        [SerializeField] Button minusButton;

        private void Start()
        {
            // ��Ŵ������ҷ���ºѹ�֡���
            languageIndex = PlayerPrefs.GetInt(LanguagePrefKey, GetCurrentLanguageIndex());

            UpdateLanguage();
        }

        public void Init()
        {
            Dispose();

            // ��Ŵ������ҷ���ºѹ�֡���
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

        // �ѧ��ѹ����Ѻ����¹����ҶѴ�
        public void NextLanguage()
        {
            languageIndex = (languageIndex + 1) % LocalizationSettings.AvailableLocales.Locales.Count;
            UpdateLanguage();
        }

        // �ѧ��ѹ����Ѻ��͹��Ѻ����ҡ�͹˹��
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

            languageText.text = selectedLocale.LocaleName; // �ѻവ UI

            // �ѹ�֡����������� PlayerPrefs
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
