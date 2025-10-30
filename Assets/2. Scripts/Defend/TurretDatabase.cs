// 파일명: TurretDatabase.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TurretDatabase : MonoBehaviour
{
    public static TurretDatabase Instance { get; private set; }
    public List<TurretDefinition> AllTurrets;
    private Dictionary<string, TurretDefinition> _turretLookup;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        _turretLookup = AllTurrets.ToDictionary(def => def.ID);
    }

    public TurretDefinition GetTurretByID(string id)
    {
        _turretLookup.TryGetValue(id, out var def);
        return def;
    }
}