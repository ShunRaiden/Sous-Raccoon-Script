using System.Collections;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class FallingObject : MonoBehaviour
    {
        [SerializeField] FallingModel model;
        [SerializeField] float fallingSpeed;
        [SerializeField] float waitSpawnDown;

        [SerializeField] GameObject dangerZoneWarning;
        [SerializeField] LayerMask groundLayer; // ������������Ǩ�ͺ���

        public void SetFallingObject(float damageCustomer, int damagePlayer)
        {
            model.SetUpFallingModel(this.gameObject, fallingSpeed, damageCustomer, damagePlayer, groundLayer);
        }

        private void Start()
        {
            StartCoroutine(OnSpawn());
        }

        IEnumerator OnSpawn()
        {
            // �ԧ Raycast ŧ����� Spawn Danger Zone Warning
            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
            {
                // ���ҧ Danger Zone Warning �ç�ش��� Raycast �����
                Instantiate(dangerZoneWarning, hit.point, Quaternion.identity, transform);
            }

            yield return new WaitForSeconds(waitSpawnDown);

            model.startFalling = true;
        }
    }
}

