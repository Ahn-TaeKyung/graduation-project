using System.Collections.Generic;

[System.Serializable]
public class RoleData
{
    public int m_player_id;   // 플레이어 식별자 (PlayerRef.PlayerId 등)
    public RoleType m_role;       // 역할 int 값 (RoleType enum int 변환)
}

[System.Serializable]
public class RoleDataListWrapper
{
    public List<RoleData> roles;

    public RoleDataListWrapper(List<RoleData> roleList)
    {
        roles = roleList;
    }

    public List<RoleData> ToList()
    {
        return roles;
    }
}
