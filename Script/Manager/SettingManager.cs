using SousRaccoon.Setting;
using UnityEngine;

namespace SousRaccoon.Manager
{
    public class SettingManager : MonoBehaviour
    {

        #region Singleton
        public static SettingManager instance { get { return _instance; } }
        private static SettingManager _instance;

        private void Awake()
        {
            // if the singleton hasn't been initialized yet
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;//Avoid doing anything else
            }
            if (_instance == null)
            {
                _instance = this;
                //DontDestroyOnLoad(this.gameObject);
            }
        }
        #endregion

        [SerializeField] QualitySetting qualitySetting;
        [SerializeField] LanguageSetting languageSetting;
        [SerializeField] ResolutionSetting resolutionSetting;
        [SerializeField] ScreenModeSetting screenModeSetting;
        [SerializeField] AntiAliasingSetting antiAliasingSetting;

        public void Init()
        {
            qualitySetting.Init();
            languageSetting.Init();
            resolutionSetting.Init();
            screenModeSetting.Init();
            antiAliasingSetting.Init();
        }

        private void OnDestroy()
        {
            qualitySetting.Dispose();
            languageSetting.Dispose();
            resolutionSetting.Dispose();
            screenModeSetting.Dispose();
            antiAliasingSetting.Dispose();
        }
    }
}

