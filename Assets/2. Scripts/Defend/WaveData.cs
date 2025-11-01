// 파일명: WaveData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WaveData_01", menuName = "TowerDefense/Wave Data")]
public class WaveData : ScriptableObject
{
    [Tooltip("이 웨이브를 구성하는 적 그룹 목록")]
    public List<WaveSpawnGroup> spawnGroups;
    
    [Tooltip("이 웨이브가 완전히 끝난 후, 다음 웨이브가 시작되기까지의 대기 시간")]
    public float timeAfterWave = 10.0f;
}