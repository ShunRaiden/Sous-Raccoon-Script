using SousRaccoon.Manager;
using UnityEngine;

namespace SousRaccoon.CameraMove
{
    public class ZoomZone : MonoBehaviour
    {
        [SerializeField] int indexZone;
        public Vector3 zoomOffset; // กำหนดค่า Y และ Z สำหรับการ Zoom
        public Vector2 limitXOffset;
        public Vector2 limitZOffset;
        private CameraZoom cameraZoom; // เก็บอ้างอิงถึง CameraZoom
        private CameraMovement cameraMovement;

        private void Start()
        {
            cameraZoom = FindObjectOfType<CameraZoom>(); // หา CameraZoom เพียงครั้งเดียว
            cameraMovement = FindObjectOfType<CameraMovement>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && cameraZoom != null)
            {
                StageManager.instance.OnChangeSizeUIEvent(indexZone);
                cameraZoom.StartZoom(zoomOffset); // เรียกใช้ StartZoom ใน CameraZoom
                cameraMovement.SetLimit(limitXOffset, limitZOffset);
            }
        }
    }
}
