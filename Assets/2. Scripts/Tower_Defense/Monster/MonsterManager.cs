using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance;

    private List<GameObject> monsters = new List<GameObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Register(GameObject monster)
    {
        monsters.Add(monster);
    }

    public void Unregister(GameObject monster)
    {
        monsters.Remove(monster);
    }

    // 요청한 몬스터 바로 앞 몬스터 가져오기
    public GameObject GetPreviousMonster(MonsterMovement requester)
    {
        int index = monsters.FindIndex(m => m == requester.gameObject);
        if (index > 0)
        {
            return monsters[index - 1];
        }
        return null;
    }
}
