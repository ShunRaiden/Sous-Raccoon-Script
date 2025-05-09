using UnityEngine;

namespace SousRaccoon.UI
{
    public class UICredits : MonoBehaviour
    {
        [SerializeField] RectTransform content;
        [SerializeField] Vector2 startPos;
        [SerializeField] Vector2 endPos;
        [SerializeField] float speed;

        private void OnEnable()
        {
            content.anchoredPosition = startPos;
        }

        void Update()
        {
            // ��Ǩ�ͺ��� content �ѧ���֧���˹� endPos �������
            if (content.anchoredPosition.y < endPos.y)
            {
                // ���� � ����͹ content ŧ������� �
                content.anchoredPosition = Vector2.MoveTowards(
                    content.anchoredPosition,
                    endPos,
                    speed * Time.deltaTime
                );
            }
        }
    }
}