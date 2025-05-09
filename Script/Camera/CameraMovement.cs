using UnityEngine;

namespace SousRaccoon.CameraMove
{
    public class CameraMovement : MonoBehaviour
    {
        public Transform targetTransform; // µÓáË¹è§¢Í§¼ÙéàÅè¹·Õè¡ÅéÍ§¨ÐµÔ´µÒÁ
        public Transform playerTransform;
        public Transform chefTransform;
        public float smoothSpeed = 0.125f; // ¤ÇÒÁàÃçÇã¹¡ÒÃàÅ×èÍ¹¡ÅéÍ§áºº¹ØèÁ¹ÇÅ
        public Vector3 offset; // ¡ÒÃàÂ×éÍ§¢Í§¡ÅéÍ§¨Ò¡¼ÙéàÅè¹

        public float minX; // ¢Íºà¢µµèÓÊØ´¢Í§¡ÒÃàÅ×èÍ¹ã¹á¡¹ X
        public float maxX; // ¢Íºà¢µÊÙ§ÊØ´¢Í§¡ÒÃàÅ×èÍ¹ã¹á¡¹ X
        public float minZ; // ¢Íºà¢µµèÓÊØ´¢Í§¡ÒÃàÅ×èÍ¹ã¹á¡¹ Z
        public float maxZ; // ¢Íºà¢µÊÙ§ÊØ´¢Í§¡ÒÃàÅ×èÍ¹ã¹á¡¹ Z

        public bool isFollow;
        public bool isFreeCam;

        void LateUpdate()
        {
            if (targetTransform == null || !isFollow) return;

            // คำนวณตำแหน่งที่ต้องการ
            Vector3 desiredPosition = targetTransform.position + offset;

            if (isFreeCam)
            {
                transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
                return;
            }

            // จำกัดตำแหน่ง X และ Z ตามที่กำหนด
            float limitedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
            float limitedZ = Mathf.Clamp(desiredPosition.z, minZ, maxZ);

            // สร้างตำแหน่งที่จำกัด
            Vector3 limitedPosition = new Vector3(limitedX, transform.position.y, limitedZ);

            // เคลื่อนที่กล้องตามตำแหน่งที่จำกัดโดยใช้อัตราความเร็วที่สัมพันธ์กับเวลา (Time.deltaTime)
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, limitedPosition, smoothSpeed * Time.deltaTime);

            transform.position = smoothedPosition;
        }

        public void SetLimit(Vector2 xLimit, Vector2 zLimit)
        {
            minX = xLimit.x;
            maxX = xLimit.y;
            minZ = zLimit.x;
            maxZ = zLimit.y;
        }

        public void LookAtChef()
        {
            targetTransform = chefTransform;
        }

        public void LookAtPlayer()
        {
            targetTransform = playerTransform;
        }

        public void LookAtTarget(Transform target)
        {
            targetTransform = target;
        }

        public void LookAtFreeCamTarget(Transform target)
        {
            targetTransform = target;
            isFreeCam = true;
        }

        public void ResetFreeCam()
        {
            isFreeCam = false;
        }

        // áÊ´§¢Íºà¢µ¡ÒÃàÅ×èÍ¹¢Í§¡ÅéÍ§ã¹ Unity Editor ´éÇÂ Gizmos
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            // ÇÒ´àÊé¹ÅéÍÁ¡ÃÍºáÊ´§¢Íºà¢µ¡ÒÃàÅ×èÍ¹¢Í§¡ÅéÍ§ã¹á¡¹ X áÅÐ Z
            Vector3 center = new Vector3((minX + maxX) / 2, transform.position.y, (minZ + maxZ) / 2);
            Vector3 size = new Vector3(maxX - minX, 0, maxZ - minZ);

            Gizmos.DrawWireCube(center, size);
        }
    }
}
