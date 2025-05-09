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
            // ตรวจสอบว่า content ยังไม่ถึงตำแหน่ง endPos หรือไม่
            if (content.anchoredPosition.y < endPos.y)
            {
                // ค่อย ๆ เลื่อน content ลงไปเรื่อย ๆ
                content.anchoredPosition = Vector2.MoveTowards(
                    content.anchoredPosition,
                    endPos,
                    speed * Time.deltaTime
                );
            }
        }
    }
}