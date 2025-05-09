using SousRaccoon.Manager;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace SousRaccoon.UI
{
    public class CanvasConst : MonoBehaviour
    {
        [SerializeField] RectTransform targetBG;
        [SerializeField]
        Vector3[] targetScales =
            {
                new Vector3(0.007f, 0.007f, 0.007f),
                new Vector3(0.0085f, 0.0085f, 0.0085f),
                new Vector3(0.01f, 0.01f, 0.01f)
            };
        float duration = 0.25f; // ��������㹡������¹ scale
        private Coroutine currentCoroutine;

        [SerializeField] RotationConstraint rotationConstraint;
        // Start is called before the first frame update
        void Start()
        {
            var targetObject = FindAnyObjectByType<PivotCanvas>();

            // ���ҧ ConstraintSource �������
            ConstraintSource source = new ConstraintSource();

            // ��駤�� source object �� targetObject
            source.sourceTransform = targetObject.transform;

            // ��駤�ҹ��˹ѡ (Weight) �ͧ�����ع 1 = 100%
            source.weight = 1.0f;

            // ���� Source ŧ� RotationConstraint
            rotationConstraint.AddSource(source);

            if (targetBG != null && StageManager.instance != null)
                StageManager.instance.OnPlayerDetectZoomZoneEvent += OnChangeUISize;
        }

        private void OnDestroy()
        {
            if (targetBG != null && StageManager.instance != null)
                StageManager.instance.OnPlayerDetectZoomZoneEvent -= OnChangeUISize;
        }

        public void OnChangeUISize(int index)
        {
            if (targetBG != null)
            {
                if (index < 0 || index >= targetScales.Length) return;

                if (currentCoroutine != null)
                {
                    StopCoroutine(currentCoroutine);
                }

                currentCoroutine = StartCoroutine(ScaleToTarget(targetScales[index]));
            }
        }

        IEnumerator ScaleToTarget(Vector3 target)
        {
            Vector3 startScale = targetBG.localScale;
            float time = 0f;

            while (time < duration)
            {
                float t = time / duration;
                targetBG.localScale = Vector3.Lerp(startScale, target, t);
                time += Time.deltaTime;
                yield return null;
            }

            targetBG.localScale = target;
            currentCoroutine = null;
        }
    }
}

