using System.Collections.Generic;
using UnityEngine;

public static class RoleSyncHelper
{
    public static void SaveAllRolesFromSyncedData()
    {
        // 클라이언트도 전체 NetworkRole 접근 가능 (동기화 전제)
        var allRoles = Object.FindObjectsByType<NetworkRole>(FindObjectsSortMode.None);
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
        Debug.Log("[RoleSyncHelper] 클라이언트가 동기화된 전체 역할을 저장함");
    }
}
