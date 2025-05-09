using SousRaccoon.Player;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Lobby
{
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        public float swingForce = 10f; // ปรับแรงแกว่งตามที่ต้องการ

        public float pushForce = 5f; // กำหนดแรงผลัก

        public List<GameObject> dummyModel;

        [SerializeField] bool isHitBox;

        private void Start()
        {
            var num = Random.Range(0, dummyModel.Count);
            dummyModel[num].SetActive(true);
        }

        // ฟังก์ชันเรียกใช้เมื่อ dummy ถูกโจมตี
        public void Hit(Vector3 hitDirection)
        {
            // เพิ่มแรงให้กับ Rigidbody เพื่อให้ dummy แกว่งไปตามทิศทางที่โดนตี
            rb.AddForce(hitDirection.normalized * swingForce, ForceMode.Impulse);
        }

        private void OnTriggerStay(Collider hit)
        {
            if (isHitBox) return;

            if (hit.CompareTag("Player"))
            {
                var charCon = hit.GetComponent<PlayerInputManager>();

                if (rb != null && !rb.isKinematic)
                {
                    // ตรวจสอบความเร็วของตัวละคร
                    if (charCon.moveAmount != 0) // ต้องเคลื่อนที่มากกว่า 0.1 หน่วยถึงจะส่งแรง
                    {
                        Vector3 pushDir = new Vector3(charCon.movementInput.x, 0, charCon.movementInput.y);
                        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}

