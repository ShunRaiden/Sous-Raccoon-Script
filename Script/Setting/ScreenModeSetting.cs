using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SousRaccoon.Setting
{
    public class ScreenModeSetting : MonoBehaviour
    {
        public TMP_Text screenModeText; // Text ที่จะบอกสถานะหน้าจอ
        private int screenModeIndex = 0; // Index เพื่อบอกโหมดปัจจุบัน

        // มีทั้งหมด 3 โหมด: Full Screen, Windowed, Borderless Window (ไม่เต็มจอ)
        private readonly string[] screenModes = { "Full Screen", "Window Mode" };

        [SerializeField] Button plusButton;
        [SerializeField] Button minusButton;

        public void Init()
        {
            Dispose();

            UpdateScreenMode(); // อัปเดตข้อความสถานะเริ่มต้นและตั้งค่าโหมดหน้าจอ

            plusButton.onClick.AddListener(NextScreenMode);
            minusButton.onClick.AddListener(PreviousScreenMode);
        }

        public void Dispose()
        {
            plusButton.onClick.RemoveListener(NextScreenMode);
            minusButton.onClick.RemoveListener(PreviousScreenMode);
        }

        // ฟังก์ชันสำหรับไปข้างหน้า (Next)
        public void NextScreenMode()
        {
            screenModeIndex = (screenModeIndex + 1) % screenModes.Length; // เพิ่ม index และวนลูปกลับไปที่ 0 เมื่อครบ 3 โหมด
            UpdateScreenMode(); // อัปเดตโหมดหน้าจอ
        }

        // ฟังก์ชันสำหรับย้อนกลับ (Previous)
        public void PreviousScreenMode()
        {
            screenModeIndex--;
            if (screenModeIndex < 0) // ถ้า index ต่ำกว่า 0 ให้ไปโหมดสุดท้าย
            {
                screenModeIndex = screenModes.Length - 1; // กลับไปที่โหมดสุดท้าย
            }
            UpdateScreenMode(); // อัปเดตโหมดหน้าจอ
        }

        private void UpdateScreenMode()
        {
            switch (screenModeIndex)
            {
                case 0: // Full Screen
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 1: // Windowed Mode (มีขอบ)
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
            }
            screenModeText.text = screenModes[screenModeIndex]; // อัปเดตข้อความที่แสดงใน UI
        }
    }
}
