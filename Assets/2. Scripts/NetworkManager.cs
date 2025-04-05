using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }
    private NetworkRunner m_network_runner;
    
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
        }
        else
        {
            Debug.LogError($"방 참가 실패: {result.ShutdownReason}");
        }
    }
}
