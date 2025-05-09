using SousRaccoon.Player;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Lobby
{
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        public float swingForce = 10f; // ��Ѻ�ç��觵������ͧ���

        public float pushForce = 5f; // ��˹��ç��ѡ

        public List<GameObject> dummyModel;

        [SerializeField] bool isHitBox;

        private void Start()
        {
            var num = Random.Range(0, dummyModel.Count);
            dummyModel[num].SetActive(true);
        }

        // �ѧ��ѹ���¡������� dummy �١����
        public void Hit(Vector3 hitDirection)
        {
            // �����ç���Ѻ Rigidbody ������� dummy ���仵����ȷҧ���ⴹ��
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
                    // ��Ǩ�ͺ�������Ǣͧ����Ф�
                    if (charCon.moveAmount != 0) // ��ͧ����͹����ҡ���� 0.1 ˹��¶֧�����ç
                    {
                        Vector3 pushDir = new Vector3(charCon.movementInput.x, 0, charCon.movementInput.y);
                        rb.AddForce(pushDir * pushForce, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}

