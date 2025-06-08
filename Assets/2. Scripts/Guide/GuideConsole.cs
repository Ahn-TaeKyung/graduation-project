using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GuideConsole : MonoBehaviour, IGameEndListener
{
    public GameObject consolePanel;
    public TMP_InputField consoleInput;
    public ScrollRect consoleScrollRect;
    public GameObject consoleLinePrefab; // TMP_Text 프리팹

    public GameObject m_end_canvas;
    private PatternStep pattern;
    private int[] pattern9;
    private bool isConsoleVisible = true;
    private string[] parts;

    void Start()
    {
        consolePanel.SetActive(isConsoleVisible);
        consoleInput.onSubmit.AddListener(OnInputSubmit);
        // GameStateManager에 자신을 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("[MonsterSpawner] GameStateManager 인스턴스가 없습니다.");
        }
    }

    public void OnGameEnd()
    {
        m_end_canvas.SetActive(true);
    }
    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && isConsoleVisible)
        {
            ToggleConsole();
        }
    }

    public void ToggleConsole()
    {
        isConsoleVisible = !isConsoleVisible;
        consolePanel.SetActive(isConsoleVisible);

        if (isConsoleVisible)
        {
            consoleInput.text = "";
            consoleInput.ActivateInputField();
        }
    }

    void SubmitCommand()
    {
        string command = consoleInput.text;

        if (!string.IsNullOrWhiteSpace(command))
        {
            string prefix = ">" + '\u00A0';
            string cleanCommand = command.Replace("\r", "").Replace("\n", "").Trim();
            LogToConsole(prefix + cleanCommand);

            if (command == "/clear")
            {
                ClearConsole();
            }
            else if (command.StartsWith("/button"))
            {
                parts = command.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                {
                    // /button → 전체 설명 출력
                    LogToConsole(" Button with 6-lengthCode, Example Usage : /Button <Code> \n =========================\n if code is correct, a sentence will showup.\n press the button with correct sequence.\n if your answer is correct, it solved.");
                }
                else if (parts.Length == 2)
                {
                    string buttonCode = parts[1];

                    if (parts[1].Length == 6)
                    {
                        pattern = PatternCodec.CodeToUniquePattern(buttonCode);
                        if (pattern.Type == PatternStep.InputType.Click)
                            LogToConsole($" The gentle Click breeze carried the scent of {pattern.Count} blooming jasmine through the quiet garden.");
                        else
                            LogToConsole($" She closed her eyes hold and listened to the soothing {pattern.Count} sound of the {pattern.MinHoldTime} waves crashing against the shore.");
                    }
                    else
                    {
                        LogToConsole($"  Unknown button code: {buttonCode}");
                    }
                }
                else
                {
                    LogToConsole("  Usage: /button [code]");
                }
            }
            else if(command.StartsWith("/9button"))
            {
                parts = command.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                {
                    // /button → 전체 설명 출력
                    LogToConsole(" 9Buttons with 6-lengthCode, Example Usage : /9Button <Code>\n =========================\n if code is correct, a sentence will showup.\n press the button with correct pattern.\n then press the bottom right button.\n if your answer is correct, it solved.");
                }
                else if (parts.Length == 2)
                {
                    string buttonCode = parts[1];

                    if (parts[1].Length == 6)
                    {
                        pattern9 = GridPatternCodec.CodeToPattern9(buttonCode);
                        if (pattern9 != null)
                            LogToConsole($" Set switch {pattern9[0]} is down, initialize mode to {pattern9[1]}, enable flag {pattern9[2]}, reset counter to {pattern9[3]}, enable  LED {pattern9[4]}, disable alarm {pattern9[5]}, configure port {pattern9[6]}, lock state {pattern9[7]}, and finalize with checksum bit {pattern9[8]}.");
                    }
                    else
                    {
                        LogToConsole($" Unknown button code: {buttonCode}");
                    }
                }
                else
                {
                    LogToConsole("  Usage: /9button [code]");
                }
            }
            else if(command.StartsWith("/help")||command.StartsWith("/?"))
            {
                LogToConsole(" /button\t\tButton with 6-lengthCode\n /9buttons\t\t9Buttons with 6-lengthCode");
            }
            else
            {
                LogToConsole(" command not found.\n type \"/?\" or \"/help\" for more information.");
            }
            consoleInput.text = "";
            StartCoroutine(ReFocusInputField());
           
        }
    }

    private void OnInputSubmit(string text)
    {
        if (!isConsoleVisible) return;
        SubmitCommand();
    }

    public void LogToConsole(string message)
    {
        GameObject newLine = Instantiate(consoleLinePrefab, consoleScrollRect.content);
        TMP_Text textComponent = newLine.GetComponent<TMP_Text>();
        textComponent.text = message;

        StartCoroutine(ScrollToBottomNextFrame());
    }

    private System.Collections.IEnumerator ReFocusInputField()
    {
        yield return null;
        consoleInput.ActivateInputField();
    }

    private System.Collections.IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        yield return null;

        LayoutRebuilder.ForceRebuildLayoutImmediate(consoleScrollRect.content);

        Canvas.ForceUpdateCanvases();
        consoleScrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearConsole()
    {
        foreach (Transform child in consoleScrollRect.content)
        {
            Destroy(child.gameObject);
        }
    }
}