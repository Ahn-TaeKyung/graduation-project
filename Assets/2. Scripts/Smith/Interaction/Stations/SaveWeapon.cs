using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[Serializable]
public class SavedWeapon
{
    public string itemType; // "Sword" or "Bow"
    public int count;
}

[Serializable]
public class SavedWeaponData
{
    public List<SavedWeapon> items = new List<SavedWeapon>();
}

public static class SaveWeapon
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "saveWeapon.json");

    public static SavedWeaponData Load()
    {
        if (!File.Exists(FilePath))
            return new SavedWeaponData();

        try
        {
            var json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<SavedWeaponData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveWeapon] Load failed: {e.Message}");
            return new SavedWeaponData();
        }
    }

    public static void Save(SavedWeaponData data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
#if UNITY_EDITOR
        Debug.Log($"[SaveWeapon] Saved to {FilePath}");
#endif
    }

    public static void Add(string itemType)
    {
        var data = Load();
        var entry = data.items.FirstOrDefault(i => i.itemType == itemType);

        if (entry != null)
            entry.count++;
        else
            data.items.Add(new SavedWeapon { itemType = itemType, count = 1 });

        Save(data);
    }
}
