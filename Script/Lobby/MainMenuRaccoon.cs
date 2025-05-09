using UnityEngine;

public class MainMenuRaccoon : MonoBehaviour
{
    [SerializeField] int animationIndex;
    [SerializeField] float minRandomNumber;
    [SerializeField] float maxRandomNumber;

    Animator animator;

    float timeCount = 0;
    float timeDuration;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        timeCount += Time.deltaTime;

        if (timeCount > timeDuration)
        {
            RandomNumber();
            animator.SetInteger("Index", Random.Range(0, animationIndex));
            timeCount = 0;
        }
    }

    public void RandomNumber()
    {
        timeDuration = Random.Range(minRandomNumber, maxRandomNumber);
    }
}