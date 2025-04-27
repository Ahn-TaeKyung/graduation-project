using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class HostStartButton : NetworkBehaviour
{
    [SerializeField] private Button m_start_button;
    [SerializeField] private SceneRef m_game_scene_ref; // 전환할 게임 씬 이름
    [SerializeField] private SceneRef m_current_scene_ref; 

    public override void Spawned()
    {
        Debug.Log($"start_button spawned{Runner}/{Runner.IsServer}");
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

        // (선택) 플레이어들이 역할 선택했는지 확인할 수도 있음
        CheckAllPlayersRole();
        if(Runner.IsServer)
        {
            // 씬 이동
            if (Runner.SceneManager != null)
            {
                Debug.Log($"SceneChange{m_game_scene_ref}/ {Runner.SceneManager}");   
                RPC_LoadGameScene();
                RPC_UnLoadGameScene();
            }
            else
            {
                Debug.LogError("SceneManager가 없습니다! 씬 이동 실패");
            }
        }
    }

    private void CheckAllPlayersRole()
    {
        var networkRoles = FindObjectsByType<NetworkRole>(FindObjectsSortMode.None);

        foreach (var role in networkRoles)
        {
            Debug.Log($"플레이어 {role.Object.InputAuthority.PlayerId}의 선택된 역할은 {role.m_player_role}입니다.");
        }
    }

        // RPC: 모든 클라이언트에게 씬을 로드하라는 명령을 전달
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_LoadGameScene()
    {
        // 서버에서 씬을 로드한 후 클라이언트들에 씬 로드 요청
        Runner.SceneManager.LoadScene(m_game_scene_ref, new NetworkLoadSceneParameters());
    }

        // RPC: 모든 클라이언트에게 씬을 로드하라는 명령을 전달
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RPC_UnLoadGameScene()
    {
        // 서버에서 씬을 로드한 후 클라이언트들에 씬 로드 요청
       Runner.SceneManager.UnloadScene(m_current_scene_ref);
    }
}
