using SousRaccoon.CameraMove;
using System.Collections.Generic;
using UnityEngine;

namespace SousRaccoon.Player
{
    public class PlayerCutOffWallSystem : MonoBehaviour
    {
        private static readonly int POSITION_ID = Shader.PropertyToID("_Position");
        private static readonly int SIZE_ID = Shader.PropertyToID("_Size");
        private static readonly int INSIDE_EDGE_ID = Shader.PropertyToID("_InsideEdgeSmooth");
        private static readonly int OUTSIDE_EDGE_ID = Shader.PropertyToID("_OutsideEdgeSmooth");

        [SerializeField] private List<Material> targetMaterials;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private float cutOffScale;
        [SerializeField][Range(0, 1)] private float smoothInsideRate;
        [SerializeField][Range(0, 1)] private float smoothOutsideRate;

        private void Start()
        {
            if (mainCamera == null)
            {
                var cam = FindAnyObjectByType<CameraZoom>();
                if (cam) mainCamera = cam.GetComponent<Camera>();
            }

            UpdateViewportPosition();
        }

        private void Update()
        {
            if (mainCamera == null) return;

            Vector3 direction = mainCamera.transform.position - transform.position;
            bool isBlocked = Physics.Raycast(transform.position, direction.normalized, out _, 3000, layerMask); //เช็คว่ามันบังหรือไม่ด้วย Raycast
            float targetSize = isBlocked ? cutOffScale : 0; //นำค่าที่เช็คมาใส่ค่าโดยถ้าบังให้ targetSize = cutOffScale ถ้าไม่ก็ = 0

            foreach (var mat in targetMaterials)
            {
                mat.SetFloat(SIZE_ID, targetSize); //Set All Materials โดยการวนลูป
            }

            if (targetSize > 0)
            {
                UpdateViewportPosition(); //Update ตัว CutOff Postion
            }
        }

        private void UpdateViewportPosition()
        {
            Vector3 viewPos = mainCamera.WorldToViewportPoint(transform.position);
            foreach (var mat in targetMaterials)
            {
                mat.SetVector(POSITION_ID, viewPos);
                mat.SetFloat(INSIDE_EDGE_ID, smoothInsideRate);
                mat.SetFloat(OUTSIDE_EDGE_ID, smoothOutsideRate);
            }
        }
    }
}