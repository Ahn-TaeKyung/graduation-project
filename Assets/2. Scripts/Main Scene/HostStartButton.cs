using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

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
        SaveAllRolesBeforeSceneLoad();
        if (Runner.IsServer)
        {
            // 씬 이동
            if (Runner.SceneManager != null)
            {
                Debug.Log($"SceneChange{m_game_scene_ref}/ {Runner.SceneManager}");
                Debug.Log("=== [Runner 상태 디버그 시작] ===");

                Debug.Log($"IsRunning: {Runner.IsRunning}");
                Debug.Log($"IsServer: {Runner.IsServer}");
                Debug.Log($"IsClient: {Runner.IsClient}");
                Debug.Log($"GameMode: {Runner.GameMode}");
                Debug.Log($"ProvideInput: {Runner.ProvideInput}");
                Debug.Log($"LocalPlayer: {Runner.LocalPlayer}");
                Debug.Log($"Player Count: {Runner.ActivePlayers.Count()}");
                Debug.Log($"SceneManager 존재 여부: {Runner.SceneManager != null}");
                if (Runner.SessionInfo != null)
                {
                    Debug.Log($"Session Name: {Runner.SessionInfo.Name}");
                    Debug.Log($"Session Region: {Runner.SessionInfo.Region}");
                }

                Debug.Log("=== [Runner 상태 디버그 끝] ===");
                // Runner.SceneManager.LoadScene(m_game_scene_ref, new NetworkLoadSceneParameters());
                Runner.LoadScene("Play", LoadSceneMode.Single);
                // RPC_LoadGameScene()
                // RPC_UnLoadGameScene();
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
    public void SaveAllRolesBeforeSceneLoad()
    {
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

        RoleDataManager.SaveRoles(list);
    }
}
