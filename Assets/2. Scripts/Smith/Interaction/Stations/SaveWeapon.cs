using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq; // [신규] FirstOrDefault를 사용하기 위해 추가

[Serializable]
public class SavedWeapon
{
    public string itemType; // "Sword" / "Bow"
    public int count;       // [신규] count 추가
    // playerName, time 제거
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

        try // [추가] 이전 버전의 JSON 파일과 형식이 달라 깨질 수 있으므로 예외 처리
        {
            var json = File.ReadAllText(FilePath);
            return JsonUtility.FromJson<SavedWeaponData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveWeapon] Load failed (might be old format): {e.Message}. Creating new data.");
            return new SavedWeaponData(); // 문제가 생기면 새 데이터 반환
        }
    }

    public static void Save(SavedWeaponData data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(FilePath, json);
#if UNITY_EDITOR
        Debug.Log($"[SaveWeapon] Saved to: {FilePath}");
#endif
    }

    // [수정] Add 로직 전체 변경 (playerName 파라미터 제거)
    public static void Add(string itemType)
    {
        var data = Load();

        // 1. 해당 아이템 타입의 항목을 찾습니다.
        var existingEntry = data.items.FirstOrDefault(item => item.itemType == itemType);

        if (existingEntry != null)
        {
            // 2. 이미 항목이 있으면, count만 1 증가시킵니다.
            existingEntry.count++;
        }
        else
        {
            // 3. 항목이 없으면, count 1로 새 항목을 추가합니다.
            data.items.Add(new SavedWeapon
            {
                itemType = itemType,
                count = 1
            });
        }

        // 4. 변경된 데이터를 저장합니다.
        Save(data);
    }
}