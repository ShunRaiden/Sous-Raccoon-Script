using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Setting
{
    public class ScreenModeSetting : MonoBehaviour
    {
        public TMP_Text screenModeText; // Text ���к͡ʶҹ�˹�Ҩ�
        private int screenModeIndex = 0; // Index ���ͺ͡�����Ѩ�غѹ

        // �շ����� 3 ����: Full Screen, Windowed, Borderless Window (��������)
        private readonly string[] screenModes = { "Full Screen", "Window Mode" };

        [SerializeField] Button plusButton;
        [SerializeField] Button minusButton;

        public void Init()
        {
            Dispose();

            UpdateScreenMode(); // �ѻവ��ͤ���ʶҹ����������е�駤������˹�Ҩ�

            plusButton.onClick.AddListener(NextScreenMode);
            minusButton.onClick.AddListener(PreviousScreenMode);
        }

        public void Dispose()
        {
            plusButton.onClick.RemoveListener(NextScreenMode);
            minusButton.onClick.RemoveListener(PreviousScreenMode);
        }

        // �ѧ��ѹ����Ѻ仢�ҧ˹�� (Next)
        public void NextScreenMode()
        {
            screenModeIndex = (screenModeIndex + 1) % screenModes.Length; // ���� index ���ǹ�ٻ��Ѻ价�� 0 ����ͤú 3 ����
            UpdateScreenMode(); // �ѻവ����˹�Ҩ�
        }

        // �ѧ��ѹ����Ѻ��͹��Ѻ (Previous)
        public void PreviousScreenMode()
        {
            screenModeIndex--;
            if (screenModeIndex < 0) // ��� index ��ӡ��� 0 ���������ش����
            {
                screenModeIndex = screenModes.Length - 1; // ��Ѻ价�������ش����
            }
            UpdateScreenMode(); // �ѻവ����˹�Ҩ�
        }

        private void UpdateScreenMode()
        {
            switch (screenModeIndex)
            {
                case 0: // Full Screen
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 1: // Windowed Mode (�բͺ)
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
            }
            screenModeText.text = screenModes[screenModeIndex]; // �ѻവ��ͤ�������ʴ�� UI
        }
    }
}
