using UnityEngine;
namespace SousRaccoon.Lobby
{
    public class MirrorCam : MonoBehaviour
    {
        [SerializeField] Transform targetDir;
        [SerializeField] Transform playerCam;
        [SerializeField] Transform mirorCam;
        [SerializeField] float minAngle = -45f; // �������ش
        [SerializeField] float maxAngle = 45f;  // ����٧�ش

        private void Start()
        {
            OnResetCam();
        }

        private void Update()
        {
            var posY = new Vector3(transform.position.x, targetDir.position.y, transform.position.z);
            var side1 = targetDir.transform.position - posY;
            var side2 = transform.forward;
            float angle = Vector3.SignedAngle(side1, side2, Vector3.up);

            // �ӡѴ����������㹪�ǧ minAngle �֧ maxAngle
            float clampedAngle = Mathf.Clamp(angle, minAngle, maxAngle);

            mirorCam.localEulerAngles = new Vector3(0, clampedAngle, 0);
        }

        public void OnChangeTargerDir(Transform newTarget)
        {
            targetDir = newTarget;
        }

        public void OnResetCam()
        {
            targetDir = playerCam;
        }
    }
}