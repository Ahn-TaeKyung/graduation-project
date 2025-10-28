using System.Collections.Generic;
using UnityEngine;

public static class PlayerSyncHelper
{
    public static void SaveAllPlayersFromSyncedData()
    {
        // 클라이언트도 전체 NetworkPlayer 접근 가능 (동기화 전제)
        var allPlayers = Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None);
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

        PlayerGameDataManager.SavePlayerGameDatas(list);
        Debug.Log("[PlayerSyncHelper] 클라이언트가 동기화된 전체 역할을 저장함");
    }
}
