using SousRaccoon.Manager;
using UnityEngine;

namespace SousRaccoon.CameraMove
{
    public class ZoomZone : MonoBehaviour
    {
        [SerializeField] int indexZone;
        public Vector3 zoomOffset; // ��˹���� Y ��� Z ����Ѻ��� Zoom
        public Vector2 limitXOffset;
        public Vector2 limitZOffset;
        private CameraZoom cameraZoom; // ����ҧ�ԧ�֧ CameraZoom
        private CameraMovement cameraMovement;

        private void Start()
        {
            cameraZoom = FindObjectOfType<CameraZoom>(); // �� CameraZoom ��§��������
            cameraMovement = FindObjectOfType<CameraMovement>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && cameraZoom != null)
            {
                StageManager.instance.OnChangeSizeUIEvent(indexZone);
                cameraZoom.StartZoom(zoomOffset); // ���¡�� StartZoom � CameraZoom
                cameraMovement.SetLimit(limitXOffset, limitZOffset);
            }
        }
    }
}
