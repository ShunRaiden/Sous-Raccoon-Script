using System.Collections;
using UnityEngine;

namespace SousRaccoon.CameraMove
{
    public class CameraZoom : MonoBehaviour
    {
        public float smoothSpeed = 0.125f; // ��������㹡������¹��� Zoom
        private Vector3 targetPosition; // ���˹�������·���ͧ��è� Zoom �
        private Quaternion targetRotation; // �����ع������·���ͧ���
        private bool isZooming = false; // ʶҹС�� Zoom
        private bool isRotating = false; // ʶҹС����ع
        private Coroutine currentZoomCoroutine; // �红����Ţͧ Coroutine ���ӧҹ����

        private void Start()
        {
            // ��駤�� targetPosition ��� targetRotation �繵��˹��������
            targetPosition = transform.localPosition;
            targetRotation = transform.localRotation;
        }

        public void StartZoom(Vector3 zoomOffset)
        {
            // Stop the current coroutine if it's running
            if (currentZoomCoroutine != null)
            {
                StopCoroutine(currentZoomCoroutine);
            }

            // Set the target position and rotation based on the zoom offset
            targetPosition = new Vector3(0, zoomOffset.y, zoomOffset.z); // Keeping X at 0 for zoom focus
            targetRotation = Quaternion.Euler(zoomOffset.x, transform.localEulerAngles.y, transform.localEulerAngles.z);

            isZooming = true; // Set zooming flag
            isRotating = true; // Set rotating flag

            // Start the coroutine for smooth zoom and rotation
            currentZoomCoroutine = StartCoroutine(ZoomAndRotateCurrent());
        }

        IEnumerator ZoomAndRotateCurrent()
        {
            while (isZooming || isRotating)
            {
                // Smoothly move to the target position
                Vector3 newPosition = Vector3.Lerp(transform.localPosition, targetPosition, smoothSpeed * Time.deltaTime);

                // Smoothly rotate to the target rotation
                Quaternion newRotation = Quaternion.Lerp(transform.localRotation, targetRotation, smoothSpeed * Time.deltaTime);

                // Ensure the X position remains at 0
                newPosition.x = 0;

                // Apply the new position and rotation to the camera
                transform.localPosition = newPosition;
                transform.localRotation = newRotation;

                // Check if we've reached the target position
                if (Vector3.Distance(transform.localPosition, targetPosition) < 0.01f)
                {
                    isZooming = false; // Stop zooming if we're close enough to the target position
                }

                // Check if we've reached the target rotation
                if (Quaternion.Angle(transform.localRotation, targetRotation) < 0.01f)
                {
                    isRotating = false; // Stop rotating if we're close enough to the target rotation
                }

                yield return null; // Wait for the next frame
            }
        }
    }
}
