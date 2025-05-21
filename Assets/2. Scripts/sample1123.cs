using UnityEngine;

public class TestClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("클릭 감지!");
        GetComponent<Renderer>().material.color = Color.red; // 시각적 피드백
    }
}