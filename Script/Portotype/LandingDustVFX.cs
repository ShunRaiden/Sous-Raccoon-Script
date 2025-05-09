using UnityEngine;

public class LandingDustVFX : MonoBehaviour
{
    [SerializeField] float lifeTime = 1f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
