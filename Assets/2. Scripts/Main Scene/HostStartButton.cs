using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public class HostStartButton : NetworkBehaviour
{
    [SerializeField] private Button m_start_button;
    [SerializeField] private SceneRef m_game_scene_ref;
    [SerializeField] private SceneRef m_current_scene_ref;

    public override void Spawned()
    {
        Debug.Log($"start_button spawned {Runner}/{Runner.IsServer}");
        if (Runner.IsServer)
        {
            m_start_button.gameObject.SetActive(true);
            m_start_button.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            m_start_button.gameObject.SetActive(false);
        }
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("게임 시작 버튼 클릭됨");
        CheckAllPlayersRole();
        StartCoroutine(WaitAndSaveAllRoles());
    }

    private void CheckAllPlayersRole()
    {
        var networkPlayers = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        foreach (var player in networkPlayers)
        {
            Debug.Log($"플레이어 {player.Object.InputAuthority.PlayerId}의 선택된 캐릭터 이미지 번호는  {player.m_player_character_sprite_index} 이고 이름은 {player.m_player_name} 입니다.");
        }
    }

    private IEnumerator WaitAndSaveAllRoles()
    {
        yield return new WaitUntil(() =>
        {
            var roles = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
            // Debug.Log($"roles{roles}, roles.Length{roles.Length}, Runner.ActivePlayers.Count{Runner.ActivePlayers.Count()}, roles.All(r => r.m_player_role != 0){roles.All(r => r.m_player_role != 0)}");
            return roles.Length >= Runner.ActivePlayers.Count();
        });

        var allPlayers = FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
        var list = new List<PlayerGameData>();

        foreach (var player in allPlayers)
        {
            list.Add(new PlayerGameData
            {
                m_player_id = player.Object.InputAuthority.PlayerId,
                m_name = player.m_player_name,
                m_character_sprite_index = player.m_player_character_sprite_index
            });
        }

        // 1. 호스트가 로컬에 저장
        PlayerGameDataManager.SavePlayerGameDatas(list);
        Debug.Log("[Host] 역할 정보를 저장하고 클라이언트에게 전송합니다.");

        // 2. RPC로 JSON 문자열 전달
        string json = JsonUtility.ToJson(new PlayerGameDataListWrapper(list), true);
        RPC_SendRoleListToClientsViaJson(json);

        // 3. 씬 전환
        if (Runner.SceneManager != null)
        {
            Runner.LoadScene("Play", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError("SceneManager가 없습니다! 씬 이동 실패");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SendRoleListToClientsViaJson(string json)
    {
        if (!Runner.IsServer)
        {
            Debug.Log("[Client] 호스트로부터 역할 JSON 수신 → 저장 시작");
            var list = JsonUtility.FromJson<PlayerGameDataListWrapper>(json).players;
            PlayerGameDataManager.SavePlayerGameDatas(list);
        }
    }
}
