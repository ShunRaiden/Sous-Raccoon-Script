using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class OffScreenMarker : MonoBehaviour
{
    public Sprite iconSprite;
    public GameObject markerPrefab;
    private RectTransform markerUI;
    bool canShow;

    void OnEnable()
    {
        var uiManager = OffScreenUIManager.Instance;
        if (uiManager && markerPrefab)
        {
            markerUI = Instantiate(markerPrefab, uiManager.markerParent).GetComponent<RectTransform>();
            markerUI.GetComponent<Image>().sprite = iconSprite;
            markerUI.gameObject.SetActive(false);
            canShow = false;
            StartCoroutine(DelaySpawnIcon());
        }
    }
    private void OnDisable()
    {
        if (markerUI != null)
            Destroy(markerUI.gameObject);
    }

    void Update()
    {
        if (markerUI && canShow)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            bool isOffScreen = screenPos.z < 0 || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.y < 0 || screenPos.y > Screen.height;
            markerUI.gameObject.SetActive(isOffScreen);

            if (isOffScreen)
            {
                Vector3 clampedPos = screenPos;
                clampedPos.x = Mathf.Clamp(clampedPos.x, 50, Screen.width - 50);
                clampedPos.y = Mathf.Clamp(clampedPos.y, 50, Screen.height - 50);
                markerUI.position = clampedPos;
            }
        }
    }

    IEnumerator DelaySpawnIcon()
    {
        yield return new WaitForSeconds(0.2f);
        canShow = true;
    }

    private void OnDestroy()
    {
        if (markerUI != null)
            Destroy(markerUI.gameObject);
    }
}