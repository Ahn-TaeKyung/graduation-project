// 파일명: GridVisualizer.cs
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public static GridVisualizer Instance { get; private set; }
    
    // 격자를 표시할 단 하나의 평면 오브젝트
    private GameObject gridPlaneObject;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        // 씬에서 "GridVisualPlane" 오브젝트를 찾아 참조합니다.
        gridPlaneObject = GameObject.Find("GridVisualPlane");
        
        if (gridPlaneObject == null)
        {
            Debug.LogError("[GridVisualizer] 씬에서 'GridVisualPlane' 오브젝트를 찾을 수 없습니다!");
        }
        else
        {
            // 게임 시작 시 확실하게 숨김
            gridPlaneObject.SetActive(false);
        }
    }
    
    public void ShowGrid()
    {
        if (gridPlaneObject != null)
        {
            gridPlaneObject.SetActive(true);
        }
    }

    public void HideGrid()
    {
        if (gridPlaneObject != null)
        {
            gridPlaneObject.SetActive(false);
        }
    }
}