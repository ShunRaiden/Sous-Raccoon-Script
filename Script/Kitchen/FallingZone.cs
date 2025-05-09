using SousRaccoon.CameraMove;
using SousRaccoon.Manager;
using SousRaccoon.Player;
using TMPro;
using UnityEngine;

namespace SousRaccoon.Kitchen
{
    public class FallingZone : MonoBehaviour
    {
        [SerializeField] CameraMovement cameraMovement;
        [SerializeField] PlayerLocomotion playerLocomotion;

        [SerializeField] float timeRespawn;
        [SerializeField] float currentRespawnTime;

        [SerializeField] Transform spawnPoint;

        [SerializeField] GameObject countdownBG;
        [SerializeField] TMP_Text countdownText;

        bool isRespawning;

        // Update is called once per frame
        void Update()
        {
            if (isRespawning)
            {
                currentRespawnTime -= Time.deltaTime * 2f;

                countdownText.text = Mathf.CeilToInt(currentRespawnTime).ToString();

                if (currentRespawnTime <= 0)
                {
                    countdownBG.SetActive(false);

                    isRespawning = false;

                    playerLocomotion.SetPlayerPosition(spawnPoint);

                    if (!StageManager.instance.isGameLose)
                    {
                        cameraMovement.targetTransform = cameraMovement.playerTransform;
                    }
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (!StageManager.instance.isGameLose)
                {
                    cameraMovement.targetTransform = spawnPoint.transform;
                }

                playerLocomotion = other.GetComponent<PlayerLocomotion>();

                currentRespawnTime = timeRespawn;

                countdownBG.SetActive(true);
                isRespawning = true;
            }
        }
    }
}

