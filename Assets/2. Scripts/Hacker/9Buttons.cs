using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NineButtons : MonoBehaviour
{
    public float pressDistance = 0.7f;
    public Vector3 pressDirection = Vector3.back;
    public int[] answerPattern = new int[9];
    public int[] userInput = new int[9];

    public GameObject[] buttons = new GameObject[9];   // 0~8 버튼
    public GameObject confirmButton;                   // 9번: 확인
    public string patternCode = "4A2M7z";

    private Vector3[] originalPositions;
    private Vector3 originalConfirm;
    private bool isInteractable = true;
    public bool isComplete = false;

    void Awake()
    {
        originalPositions = new Vector3[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
            originalPositions[i] = buttons[i].transform.localPosition;
        originalConfirm = confirmButton.transform.localPosition;
    }

    void Start()
    {
        if (confirmButton != null)
            confirmButton.GetComponent<Renderer>().material.color = Color.green;
        GenerateRandomPattern();
        ResetUserInput();
        int[] pattern = GridPatternCodec.CodeToPattern9(patternCode);
        string grid = "";
        for (int i = 0; i < 9; i++)
        {
            grid += pattern[i] + " ";
            if ((i + 1) % 3 == 0) grid += "\n";
        }
        Debug.Log($"Pattern Grid:\n{grid}");
    }

    // ---- 버튼 눌림/릴리즈/확인 처리 ----
    public void OnButtonPress(int idx)
    {
        if (!isInteractable) return;
        if (idx == 9)
        {
            if (confirmButton != null)
            {
                confirmButton.transform.localPosition += pressDirection * pressDistance;
                confirmButton.GetComponent<Renderer>().material.color = Color.blue;
            }
        }
        else if (idx >= 0 && idx < buttons.Length)
        {
            buttons[idx].transform.localPosition = originalPositions[idx] + pressDirection * pressDistance;
            buttons[idx].GetComponent<Renderer>().material.color = Color.blue;
        }
    }

    public void OnButtonRelease(int idx)
    {
        if (!isInteractable) return;
        if (idx == 9)
        {
            OnConfirmClick();
            return;
        }

        if (idx < 0 || idx >= buttons.Length) return;

        buttons[idx].transform.localPosition = originalPositions[idx];
        userInput[idx] = 1 - userInput[idx];
        buttons[idx].GetComponent<Renderer>().material.color = userInput[idx] == 1 ? Color.white : Color.gray;
    }

    public void OnConfirmClick()
    {
        if (!isInteractable) return;
        for (int i = 0; i < 9; i++)
        {
            if (userInput[i] != answerPattern[i])
            {
                confirmButton.transform.localPosition = originalConfirm;
                confirmButton.GetComponent<Renderer>().material.color = Color.yellow;
                Debug.Log("틀렸습니다!");
                HackerCounter.Instance.AddStrike();
                // 실패 처리: 모두 빨간색 → 1초 후 초기화
                StartCoroutine(ShowFailAndReset());
                return;
            }
        }
        Debug.Log("정답!");
        confirmButton.transform.localPosition = originalConfirm;
        for (int i = 0; i < 9; i++)
        {
            var col = buttons[i].GetComponent<Collider>();
            if (col != null) col.enabled = false;
            col.GetComponent<Renderer>().material.color = Color.green;
        }
        if (confirmButton != null)
        {
            var col = confirmButton.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            col.GetComponent<Renderer>().material.color = Color.green;
        }
        HackerCounter.Instance.AddComplete();
        isComplete = true;
        // 성공 처리
    }

    public void ResetUserInput()
    {
        for (int i = 0; i < 9; i++)
        {
            userInput[i] = 0;
            if (buttons[i] != null)
            {
                buttons[i].GetComponent<Renderer>().material.color = Color.gray;
                buttons[i].transform.localPosition = originalPositions[i];
            }
        }
    }

    private IEnumerator ShowFailAndReset()
    {
        isInteractable = false;
        for (int i = 0; i < 9; i++)
        {
            if (buttons[i] != null)
                buttons[i].GetComponent<Renderer>().material.color = Color.red;
        }
        yield return new WaitForSeconds(1.0f);
        ResetUserInput();
        isInteractable = true;
    }

    void GenerateRandomPattern()
    {
        Array.Clear(answerPattern, 0, 9);
        int count = UnityEngine.Random.Range(1, 10);
        int[] idxs = new int[9];
        for (int i = 0; i < 9; i++) idxs[i] = i;
        for (int i = 8; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int tmp = idxs[i]; idxs[i] = idxs[j]; idxs[j] = tmp;
        }
        for (int i = 0; i < count; i++)
            answerPattern[idxs[i]] = 1;

        string code = GridPatternCodec.PatternToCode9(answerPattern);
        patternCode = code;
        Debug.Log($"[정답 코드(6자리)] {code}");
    }
}
public static class GridPatternCodec
{
    private const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    // 9비트 패턴(0~511)을 6자리 Base62로
    public static string PatternToCode9(int[] pattern)
    {
        // 패턴을 9비트 정수로 변환
        int val = 0;
        for (int i = 0; i < 9; i++)
        {
            if (pattern[i] != 0)
                val |= (1 << i);
        }
        // int → ulong(=uint) → 6자리 Base62로 변환
        string code = "";
        ulong num = (ulong)val;
        for (int i = 0; i < 6; i++)
        {
            code = chars[(int)(num % 62)] + code;
            num /= 62;
        }
        return code;
    }

    // 6자리 Base62 → 9비트 패턴(배열 반환)
    public static int[] CodeToPattern9(string code)
    {
        // Base62 to int
        ulong num = 0;
        foreach (var c in code)
            num = num * 62 + (ulong)chars.IndexOf(c);

        int[] pattern = new int[9];
        for (int i = 0; i < 9; i++)
        {
            pattern[i] = ((num & (1ul << i)) != 0) ? 1 : 0;
        }
        return pattern;
    }
}