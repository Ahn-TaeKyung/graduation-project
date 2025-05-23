using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        // 스폰 위치 임의 지정
        Vector3 spawnPosition = new Vector3(0, 1, 0);
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }
}