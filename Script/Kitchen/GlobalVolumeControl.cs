using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace SousRaccoon.Manager
{
    public class GlobalVolumeControl : MonoBehaviour
    {
        public Volume[] volumes;  // ��� Volume Profile ������
        public float transitionTime = 2f;

        private int currentIndex = 0;
        private Coroutine transitionCoroutine;

        public void StartBlendTo(int newIndex)
        {
            if (newIndex == currentIndex || newIndex >= volumes.Length) return;

            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);

            transitionCoroutine = StartCoroutine(BlendVolumes(currentIndex, newIndex));
        }

        private IEnumerator BlendVolumes(int fromIndex, int toIndex)
        {
            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.deltaTime / transitionTime;
                volumes[fromIndex].weight = Mathf.Lerp(1f, 0f, timer); // Ŵ�ͧ���
                volumes[toIndex].weight = Mathf.Lerp(0f, 1f, timer); // �����ͧ����
                yield return null;
            }

            volumes[fromIndex].weight = 0f;  // �Դ�ѹ���
            volumes[toIndex].weight = 1f;  // �Դ�ѹ����������
            currentIndex = toIndex;
        }
    }
}

