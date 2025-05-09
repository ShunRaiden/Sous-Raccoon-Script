using UnityEngine;

namespace SousRaccoon.Lobby
{
    public class DoorBookLobby : MonoBehaviour
    {
        [SerializeField] LobbyBook lobbyBook;

        private void Start()
        {
            lobbyBook = FindObjectOfType<LobbyBook>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                lobbyBook.OpenLobbyPanel(other.GetComponent<Player.PlayerInputManager>(), false);
            }
        }
    }
}
