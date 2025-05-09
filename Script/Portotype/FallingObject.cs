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
        [SerializeField] LayerMask groundLayer; // เลเยอร์ที่ใช้ตรวจสอบพื้น

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
            // ยิง Raycast ลงไปเพื่อ Spawn Danger Zone Warning
            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, groundLayer))
            {
                // สร้าง Danger Zone Warning ตรงจุดที่ Raycast ชนพื้น
                Instantiate(dangerZoneWarning, hit.point, Quaternion.identity, transform);
            }

            yield return new WaitForSeconds(waitSpawnDown);

            model.startFalling = true;
        }
    }
}

