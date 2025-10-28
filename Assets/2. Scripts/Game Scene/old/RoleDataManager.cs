using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class PlayerGameDataManager
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "players.json");

    public static void SavePlayerGameDatas(List<PlayerGameData> playerGameDatasList)
    {
        var wrapper = new PlayerGameDataListWrapper(playerGameDatasList);
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"[PlayerGameDataManager] 역할 정보 저장됨: {FilePath}");
    }

    public static List<PlayerGameData> LoadPlayerGameDatas()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning($"[PlayerGameDataManager] 역할 정보 파일이 존재하지 않습니다: {FilePath}");
            return new List<PlayerGameData>();
        }

        string json = File.ReadAllText(FilePath);
        var wrapper = JsonUtility.FromJson<PlayerGameDataListWrapper>(json);
        Debug.Log($"[PlayerGameDataManager] 역할 정보 로드됨");
        return wrapper.ToList();
    }
}
