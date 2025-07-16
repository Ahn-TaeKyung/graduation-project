using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ManualEntry
{
    public string moduleName;
    [TextArea(3, 10)]
    public string manualText;
}

[CreateAssetMenu(menuName = "Terminal/Manual Database")]
public class ManualDatabase : ScriptableObject
{
    public List<ManualEntry> entries;

    public string GetManualText(string moduleName)
    {
        var entry = entries.FirstOrDefault(e => e.moduleName.ToLower() == moduleName.ToLower());
        return entry != null ? entry.manualText : null;
    }

    public List<string> GetModuleNames()
    {
        return entries.Select(e => e.moduleName).ToList();
    }
}
