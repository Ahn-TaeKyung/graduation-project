using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance { get; private set; }

    // Player마다 할당된 역할 (0, 1, 2)
    public Dictionary<PlayerRef, int> m_player_roles = new Dictionary<PlayerRef, int>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetRole(PlayerRef player, int role)
    {
        if (m_player_roles.ContainsKey(player))
            m_player_roles[player] = role;
        else
            m_player_roles.Add(player, role);
    }

    public int GetRole(PlayerRef player)
    {
        if (m_player_roles.TryGetValue(player, out int role))
            return role;
        return -1; // 역할이 아직 없는 경우
    }

    public bool AreAllRolesSelected()
    {
        // 3명이 모두 역할을 정했는지 체크
        return m_player_roles.Count == 3;
    }
    public void SetPlayerRole(PlayerRef playerRef, RoleType roleType)
    {
        if (NetworkManager.Instance.m_spawn_characters.TryGetValue(playerRef, out NetworkObject playerObj))
        {
            var networkRole = playerObj.GetComponent<NetworkRole>();
            if (networkRole != null)
            {
                // networkRole.m_player_role = roleType;
            }
        }
    }
}
