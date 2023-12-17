using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] public GameObject playerPrefab;

    public PlayerController Player { get; private set; }

    private void Start()
    {
        if (PlayerController.Instance != null)
            Player = PlayerController.Instance;
        else
            Player = Instantiate(playerPrefab, transform.position, Quaternion.identity).GetComponent<PlayerController>();
    }
}