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
        Debug.Log($"[ButtonPattern] �ڵ�:{patternCode} �� {PatternToString(pattern)}");
    }


    // ��ư���� Ŭ�� ȣ�� (Click ������ ���� ����)
    public void OnButtonClick()
    {
        if (!isInteractable) return;
        if (isComplete) return;
        if (pattern.Type == PatternStep.InputType.Click)
        {
            currentCount++;
            Debug.Log($"[ButtonPattern] Ŭ�� {currentCount}/{pattern.Count}");
            OnInputEvent();
        }
        else
        {
            Debug.Log("[ButtonPattern] ����: �� ������ Click�� �ƴ�.");
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
                Debug.Log($"[ButtonPattern] Ȧ�� {currentCount}/{pattern.Count} ({holdTime:F2}s)");
                OnInputEvent();
            }
            else
            {
                Debug.Log($"[ButtonPattern] ����: {holdTime:F2}s, ���:{min:F2}~{max:F2}s");
                HackerCounter.Instance.AddStrike();
                FailSafe();
                ResetPattern();
            }
        }
        else
        {
            Debug.Log("[ButtonPattern] ����: �� ������ Hold�� �ƴ�.");
            HackerCounter.Instance.AddStrike();
            FailSafe();
            ResetPattern();
        }
    }
    private void OnInputEvent()
    {
        // ���� ���
        if (currentCount == pattern.Count)
        {
            if (checkCoroutine != null)
            {
                StopCoroutine(checkCoroutine);
                checkCoroutine = null;
            }
            isComplete = true;
            HackerCounter.Instance.AddComplete();
            Debug.Log("[ButtonPattern] ���� �Է� ����!(���)");
        }
        else
        {
            // ���� ���� ���̸�, 2�� �� �˻� �ڷ�ƾ �����
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
            Debug.Log($"[ButtonPattern] ���� �̿Ϸ�! (�Է�:{currentCount}/{pattern.Count}) - STRIKE!");
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
        Debug.Log($"[ButtonPattern] ���� ����. ���� Strike: {HackerCounter.strikeCount}");
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
            return $"Click, {p.Count}��";
        else
            return $"Hold, {p.Count}��, {p.MinHoldTime}��, ����:{p.Tolerance}";
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

// Base62 ���ڵ�
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

// ���� ���� ���� ����
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
            Debug.Log($"[���ϻ���] Click, {count}��");
            return new PatternStep(PatternStep.InputType.Click, count);
        }
        else
        {
            // 2~7�� �� ���� (����)
            int time = rand.Next(2, 8);

            int maxCount;
            if (time == 7) maxCount = 2;     // 1~2ȸ
            else if (time == 6) maxCount = 3;     // 1~3ȸ
            else if (time == 5) maxCount = 4;     // 1~4ȸ
            else if (time == 4) maxCount = 5;     // 1~5ȸ
            else maxCount = 7;     // 2~3�ʴ� 1~7ȸ

            int count = rand.Next(1, maxCount + 1);
            Debug.Log($"[���ϻ���] Hold, {count}��, {time}��, ����:0.5");
            return new PatternStep(PatternStep.InputType.Hold, count, time, 0.5f);
        }
    }
}