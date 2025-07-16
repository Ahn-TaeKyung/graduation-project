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
        var networkRoles = FindObjectsByType<NetworkRole>(FindObjectsSortMode.None);
        foreach (var role in networkRoles)
        {
            Debug.Log($"플레이어 {role.Object.InputAuthority.PlayerId}의 선택된 역할은 {role.m_player_role}입니다.");
        }
    }

    private IEnumerator WaitAndSaveAllRoles()
    {
        yield return new WaitUntil(() =>
        {
            var roles = FindObjectsByType<NetworkRole>(FindObjectsSortMode.None);
            Debug.Log($"roles{roles}, roles.Length{roles.Length}, Runner.ActivePlayers.Count{Runner.ActivePlayers.Count()}, roles.All(r => r.m_player_role != 0){roles.All(r => r.m_player_role != 0)}");
            return roles.Length >= Runner.ActivePlayers.Count();
        });

        var allRoles = FindObjectsByType<NetworkRole>(FindObjectsSortMode.None);
        var list = new List<RoleData>();

        foreach (var role in allRoles)
        {
            list.Add(new RoleData
            {
                m_player_id = role.Object.InputAuthority.PlayerId,
                m_role = role.m_player_role
            });
        }

        // 1. 호스트가 로컬에 저장
        RoleDataManager.SaveRoles(list);
        Debug.Log("[Host] 역할 정보를 저장하고 클라이언트에게 전송합니다.");

        // 2. RPC로 JSON 문자열 전달
        string json = JsonUtility.ToJson(new RoleDataListWrapper(list), true);
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
            var list = JsonUtility.FromJson<RoleDataListWrapper>(json).roles;
            RoleDataManager.SaveRoles(list);
        }
    }
}
