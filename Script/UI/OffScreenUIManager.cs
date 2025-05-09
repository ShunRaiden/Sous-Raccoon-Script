using UnityEngine;

public class OffScreenUIManager : MonoBehaviour
{
    public static OffScreenUIManager Instance;
    public Transform markerParent;

    void Awake()
    {
        Instance = this;
    }
}