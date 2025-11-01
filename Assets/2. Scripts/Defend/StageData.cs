// 파일명: StageData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageData_01", menuName = "TowerDefense/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("Stage Config")]
    [Tooltip("이 스테이지의 플레이어 총 체력")]
    public int StageHealth = 20; // 예시: 20
    [Tooltip("이 스테이지를 구성하는 웨이브 목록 (순서대로 실행됨)")]
    public List<WaveData> waves;
}