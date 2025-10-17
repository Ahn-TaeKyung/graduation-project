using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class RoleDataManager
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "roles.json");

    public static void SaveRoles(List<RoleData> roleList)
    {
        var wrapper = new RoleDataListWrapper(roleList);
        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(FilePath, json);
        Debug.Log($"[RoleDataManager] 역할 정보 저장됨: {FilePath}");
    }

    public static List<RoleData> LoadRoles()
    {
        if (!File.Exists(FilePath))
        {
            Debug.LogWarning($"[RoleDataManager] 역할 정보 파일이 존재하지 않습니다: {FilePath}");
            return new List<RoleData>();
        }

        string json = File.ReadAllText(FilePath);
        var wrapper = JsonUtility.FromJson<RoleDataListWrapper>(json);
        Debug.Log($"[RoleDataManager] 역할 정보 로드됨");
        return wrapper.ToList();
    }
}
