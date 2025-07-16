using System;
using System.Collections;
using UnityEngine;
using hacker;

public class ButtonPatternManager : MonoBehaviour
{
    public string patternCode = "4A2M7z";
    public PatternStep pattern;

    private int currentCount = 0;
    public bool isComplete = false;
    private Coroutine checkCoroutine;
    private bool isInteractable = true;
    private hacker.Button button;
    public void ApplyPatternCode()
    {
        pattern = PatternCodec.CodeToUniquePattern(patternCode);
        isComplete = false;
        currentCount = 0;
        Debug.Log($"[ButtonPattern] 코드:{patternCode} → {PatternToString(pattern)}");
    }


    // 버튼에서 클릭 호출 (Click 패턴일 때만 성공)
    public void OnButtonClick()
    {
        if (!isInteractable) return;
        if (isComplete) return;
        if (pattern.Type == PatternStep.InputType.Click)
        {
            currentCount++;
            Debug.Log($"[ButtonPattern] 클릭 {currentCount}/{pattern.Count}");
            OnInputEvent();
        }
        else
        {
            Debug.Log("[ButtonPattern] 실패: 이 패턴은 Click이 아님.");
            HackerCounter.Instance.AddStrike();
            FailSafe();
            ResetPattern();
        }
    }

    public void OnButtonHold(float holdTime)
    {
        if (!isInteractable) return;
        if (isComplete) return;
        if (pattern.Type == PatternStep.InputType.Hold)
        {
            float min = pattern.MinHoldTime - pattern.Tolerance;
            float max = pattern.MinHoldTime + pattern.Tolerance;
            if (holdTime >= min && holdTime <= max)
            {
                currentCount++;
                Debug.Log($"[ButtonPattern] 홀드 {currentCount}/{pattern.Count} ({holdTime:F2}s)");
                OnInputEvent();
            }
            else
            {
                Debug.Log($"[ButtonPattern] 실패: {holdTime:F2}s, 허용:{min:F2}~{max:F2}s");
                HackerCounter.Instance.AddStrike();
                FailSafe();
                ResetPattern();
            }
        }
        else
        {
            Debug.Log("[ButtonPattern] 실패: 이 패턴은 Hold가 아님.");
            HackerCounter.Instance.AddStrike();
            FailSafe();
            ResetPattern();
        }
    }
    private void OnInputEvent()
    {
        // 성공 즉시
        if (currentCount == pattern.Count)
        {
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
                checkCoroutine = null;
            }
            isComplete = true;
            HackerCounter.Instance.AddComplete();
            Debug.Log("[ButtonPattern] 패턴 입력 성공!(즉시)");
        }
        else
        {
            // 아직 성공 전이면, 2초 후 검사 코루틴 재시작
            if (checkCoroutine != null)
                StopCoroutine(checkCoroutine);
            checkCoroutine = StartCoroutine(CheckCompleteAfterDelay());
        }
    }
    public System.Collections.IEnumerator CheckCompleteAfterDelay()
    {
        yield return new WaitForSeconds(2.0f);
        if (!isComplete && currentCount < pattern.Count)
        {
            Debug.Log($"[ButtonPattern] 패턴 미완료! (입력:{currentCount}/{pattern.Count}) - STRIKE!");
            HackerCounter.Instance.AddStrike();
            FailSafe();
            ResetPattern();

        }
        checkCoroutine = null;
    }

    private void ResetPattern()
    {
        currentCount = 0;
        isComplete = false;
        Debug.Log($"[ButtonPattern] 패턴 리셋. 현재 Strike: {HackerCounter.strikeCount}");
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
            button.SetAllColors(Color.gray);
        }
    }
    public void OnInputStarted()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
        }
    }

    public bool IsPatternComplete() => isComplete;

    private string PatternToString(PatternStep p)
    {
        if (p.Type == PatternStep.InputType.Click)
            return $"Click, {p.Count}번";
        else
            return $"Hold, {p.Count}번, {p.MinHoldTime}초, 오차:{p.Tolerance}";
    }
    private IEnumerator FailSafe()
    {
        isInteractable = false;
        button.SetAllColors(Color.red);
        yield return new WaitForSeconds(1.0f);
        isInteractable = true;
    }
}
public class PatternStep
{
    public enum InputType { Click, Hold }
    public InputType Type;
    public int Count;
    public float MinHoldTime;
    public float Tolerance;
    public PatternStep(InputType type, int count, float minHoldTime = 0f, float tolerance = 0.5f)
    {
        Type = type;
        Count = count;
        MinHoldTime = minHoldTime;
        Tolerance = tolerance;
    }
}

// Base62 디코딩
public static class Base62Util
{
    private const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
    public static ulong FromBase62(string s)
    {
        ulong num = 0;
        foreach (var c in s)
            num = num * 62 + (ulong)chars.IndexOf(c);
        return num;
    }
}

// 단일 유일 패턴 생성
public static class PatternCodec
{
    public static PatternStep CodeToUniquePattern(string code)
    {
        ulong seed = Base62Util.FromBase62(code);
        var rand = new System.Random((int)(seed & 0x7FFFFFFF));
        bool isClick = rand.Next(2) == 0;
        if (isClick)
        {
            int count = rand.Next(1, 8); // 1~7
            Debug.Log($"[패턴생성] Click, {count}번");
            return new PatternStep(PatternStep.InputType.Click, count);
        }
        else
        {
            // 2~7초 중 랜덤 (정수)
            int time = rand.Next(2, 8);

            int maxCount;
            if (time == 7) maxCount = 2;     // 1~2회
            else if (time == 6) maxCount = 3;     // 1~3회
            else if (time == 5) maxCount = 4;     // 1~4회
            else if (time == 4) maxCount = 5;     // 1~5회
            else maxCount = 7;     // 2~3초는 1~7회

            int count = rand.Next(1, maxCount + 1);
            Debug.Log($"[패턴생성] Hold, {count}번, {time}초, 오차:0.5");
            return new PatternStep(PatternStep.InputType.Hold, count, time, 0.5f);
        }
    }
}