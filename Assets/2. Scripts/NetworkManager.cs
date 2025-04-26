using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.UI;
using TMPro;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance { get; private set; }
    private NetworkRunner m_network_runner;
    [SerializeField] private GameObject m_main_panel;
    [SerializeField] private GameObject m_room_panel;
    public TextMeshProUGUI m_room_key;
    [SerializeField] private NetworkPrefabRef m_player_prefab;
    private Dictionary<PlayerRef, NetworkObject> m_spawn_characters = new ();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        m_network_runner = gameObject.AddComponent<NetworkRunner>();
        m_network_runner.ProvideInput = true;
        m_network_runner.AddCallbacks(this);
    }


    private string GenerateRoomID()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Random random = new System.Random();
        return new string(Enumerable.Repeat(chars, 10)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }


    public async void CreateRoom()
    {
        string roomID = GenerateRoomID();
        Debug.Log($"생성된 Room ID: {roomID}");
        
        var result = await m_network_runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,
            SessionName = roomID,
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log($"방 생성 성공! Room ID: {roomID}");
            m_room_key.text = roomID;
            m_main_panel.SetActive(false);
            m_room_panel.SetActive(true);
        }
        else
        {
            Debug.LogError($"방 생성 실패: {result.ShutdownReason}");
        }
    }


    public async void JoinRoom(string roomID)
    {
        Debug.Log($"Room ID {roomID} 에 참가 시도...");
        
        var result = await m_network_runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Client,
            SessionName = roomID
        });

        if (result.Ok)
        {
            Debug.Log("방 참가 성공!");
            m_room_key.text = roomID;
            m_main_panel.SetActive(false);
            m_room_panel.SetActive(true);
        }
        else
        {
            Debug.LogError($"방 참가 실패: {result.ShutdownReason}");
        }
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Debug.Log($"플레이어 {player} 참가 -> 캐릭터 생성");

            Vector3 spawn_position = new Vector3(UnityEngine.Random.Range(-3, 3), 0, UnityEngine.Random.Range(-3, 3));
            var network_player_object = runner.Spawn(m_player_prefab, spawn_position, Quaternion.identity, player);
            Debug.Log("spawn() 완료");
            m_spawn_characters.Add(player, network_player_object);
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        if (m_spawn_characters.TryGetValue(player, out NetworkObject networkObject))
        {
            runner.Despawn(networkObject);
            m_spawn_characters.Remove(player);
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}
