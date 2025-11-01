// 파일명: StageData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "StageData_01", menuName = "TowerDefense/Stage Data")]
public class StageData : ScriptableObject
{
    [Tooltip("이 스테이지를 구성하는 웨이브 목록 (순서대로 실행됨)")]
    public List<WaveData> waves;
}