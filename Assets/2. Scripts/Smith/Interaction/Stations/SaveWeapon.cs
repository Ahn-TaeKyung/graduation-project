using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class SavedWeapon
{
    public string itemType;     // "Sword" / "Bow"
    public string playerName;   // 누가 넣었는지
    public string time;         // 언제 넣었는지
}

[Serializable]
public class SavedWeaponData
{
    public List<SavedWeapon> items = new List<SavedWeapon>();
}

public static class SaveWeapon
{
    // 네가 원하는 파일명
    private static string FilePath => Path.Combine(Application.persistentDataPath, "saveWeapon.json");

    public static SavedWeaponData Load()
    {
        if (!File.Exists(FilePath))
            return new SavedWeaponData();

        var json = File.ReadAllText(FilePath);
        return JsonUtility.FromJson<SavedWeaponData>(json);
    }

    public static void Save(SavedWeaponData data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
#if UNITY_EDITOR
        Debug.Log($"[SaveWeapon] Saved to: {FilePath}");
#endif
    }

    public static void Add(string itemType, string playerName = "")
    {
        var data = Load();
        data.items.Add(new SavedWeapon
        {
            itemType = itemType,
            playerName = playerName,
            time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });
        Save(data);
    }
}
