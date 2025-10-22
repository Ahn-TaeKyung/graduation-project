using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerGameData
{
    public int m_player_id;   // 플레이어 식별자 (PlayerRef.PlayerId 등)
    public string m_name;       // 역할 int 값 (RoleType enum int 변환)
    public int m_character_sprite_index;
}

[System.Serializable]
public class PlayerGameDataListWrapper
{
    public List<PlayerGameData> players;

    public PlayerGameDataListWrapper(List<PlayerGameData> nameList)
    {
        players = nameList;
    }

    public List<PlayerGameData> ToList()
    {
        return players;
    }
}
