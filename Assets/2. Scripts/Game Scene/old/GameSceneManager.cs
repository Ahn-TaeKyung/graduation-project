using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GameSceneManager : NetworkBehaviour
{
    
    [Header("Network & Prefabs")]
    [SerializeField] private NetworkPrefabRef playerPrefab;   // Player Prefab 등록
    [SerializeField] private Transform[] spawnPoints;         // 스폰 위치 배열

    // 씬에 들어온 플레이어와 NetworkObject 매핑
    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

    public static GameSceneManager Instance { get; private set; }

    public Camera m_camera_defender;
    public Camera m_camera_smith;

    public GameObject defenseCanvas;
    public GameObject smithCanvas;

    private string m_my_name;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void Spawned()
    {
        // NetworkRunner 가져오기
        NetworkRunner runner = Runner;
        if (runner == null)
        {
            Debug.LogError("[GameSceneManager] NetworkRunner가 존재하지 않습니다!");
            return;
        }

        // 씬 진입 시 Room에 있는 모든 플레이어 스폰
        if (Object.HasStateAuthority)
        {
            // (로그: GameSceneManager.Spawned () (at ...GameSceneManager.cs:48))
            // 당신의 코드 48라인 근처에 이 로직이 있을 것입니다.
            
            // 예시: 모든 플레이어를 스폰시키는 로직
            foreach (var player in Runner.ActivePlayers)
            {
                SpawnPlayer(player); // (로그: GameSceneManager.SpawnPlayer ... :121)
            }
        }

        // Host/Client 추가 입장 처리

        base.Spawned();

        // 역할 정보 로드 후 내 역할 저장
        var playerGameDatas = PlayerGameDataManager.LoadPlayerGameDatas();

        int myId = NetworkRunner.GetRunnerForGameObject(gameObject).LocalPlayer.PlayerId;
        foreach (var playerGameData in playerGameDatas)
        {
            if (playerGameData.m_player_id == myId)
            {
                m_my_name = playerGameData.m_name;
                break;
            }
        }
    }
    public string GetMyName()
    {
        return m_my_name;
    }

    public void SetupById()
    {
        m_camera_defender.enabled = false;
        m_camera_smith.enabled = false;

        defenseCanvas.SetActive(false);
        smithCanvas.SetActive(false);

        // switch (m_my_role)
        // {
        //     case RoleType.Defender:
        //         // Debug.Log("디펜더 셋업 완료");
        //         // m_camera_defender.gameObject.SetActive(true);
        //         // m_camera_defender.enabled = true;
        //         // defenseCanvas.SetActive(true);
        //         Debug.Log("해커2 셋업 완료");
                m_camera_smith.gameObject.SetActive(true);
                m_camera_smith.enabled = true;
                smithCanvas.SetActive(true);
        //         break;
        //     case RoleType.Smith:
        //         Debug.Log("해커 셋업 완료");
        //         m_camera_smith.gameObject.SetActive(true);
        //         m_camera_smith.enabled = true;
        //         smithCanvas.SetActive(true);
        //         break;
        //     default:
        //         Debug.LogWarning("알 수 없는 역할");
        //         break;
        // }

        Debug.Log($"[GameSceneManager] 역할 셋업 완료");
    }

    private void SpawnPlayer(PlayerRef player)
    {
        if (playerPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[GameSceneManager] PlayerPrefab 또는 SpawnPoints가 설정되지 않았습니다!");
            return;
        }

        // 스폰 위치 결정
        int spawnIndex = player.RawEncoded % spawnPoints.Length;
        Vector3 spawnPos = spawnPoints[spawnIndex].position;
        Quaternion spawnRot = spawnPoints[spawnIndex].rotation;

        // NetworkObject 생성 (Authority = player)
        NetworkObject playerObj = Runner.Spawn(playerPrefab, spawnPos, spawnRot, player);
        if (playerObj == null)
        {
            Debug.LogError("[GameSceneManager] Player Spawn 실패!");
            return;
        }

        // Spawn 완료 후 Dictionary에 저장
        spawnedPlayers[player] = playerObj;

        // 자기 캐릭터면 카메라 세팅
        // if (playerObj.HasInputAuthority)
        // {
        //     Camera.main.transform.SetParent(playerObj.transform);
        //     Camera.main.transform.localPosition = new Vector3(0f, 10f, -8f);
        //     Camera.main.transform.localEulerAngles = new Vector3(45f, 0f, 0f);
        // }
    }
}
