using TMPro;
using UnityEngine;

public class PatternCodeLabelSpawner : MonoBehaviour
{
    private ButtonPatternManager patternManager;
    public Transform panelTransform; // 버튼의 판(Panel) Transform (Inspector에서 할당)

    void Start()
    {
        if (patternManager == null)
            patternManager = GetComponent<ButtonPatternManager>();
        // 이미 자식에 있을 수도 있으니, 없을 때만 생성
        if (panelTransform == null) panelTransform = this.transform;

        GameObject labelObj = new GameObject("PatternCodeLabel_TMP");
        labelObj.transform.SetParent(panelTransform, false);

        // 아래(혹은 원하는 위치)로 이동
        labelObj.transform.localPosition = new Vector3(0f, -0.35f, 0.51f); // Y값은 베이스 두께·크기에 맞게 조절
        labelObj.transform.localRotation = Quaternion.identity; // 필요시 회전

        // TextMeshPro 생성 및 설정
        TextMeshPro textMeshPro = labelObj.AddComponent<TextMeshPro>();
        textMeshPro.text = patternManager.patternCode;
        textMeshPro.enableAutoSizing = true;     // ★ 자동 폰트 크기 조정 ON
        textMeshPro.fontSizeMin = 1.0f;          // 최소 폰트 크기 (예: 1)
        textMeshPro.fontSizeMax = 10.0f;         // 최대 폰트 크기 (예: 10)
        textMeshPro.color = Color.white;
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.rectTransform.sizeDelta = new Vector2(0.8f, 1);

        // 평면의 앞면을 기준으로 텍스트가 잘 보이게 회전 (필요시 조정)
        labelObj.transform.localRotation = Quaternion.Euler(0, 180, 0); // 필요하다면 주석 해제
    }
}