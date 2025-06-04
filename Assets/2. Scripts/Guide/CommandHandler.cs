using TMPro;
using UnityEngine;
using UnityEngine.UI; // ScrollRect 제어용

public class CommandHandler : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField inputField;
    public TextMeshProUGUI outputText;
    public ScrollRect scrollRect; // 👈 ScrollView 스크롤 제어용

    [Header("Manual Database")]
    public ManualDatabase manualDB;

    private void Start()
    {
        inputField.onSubmit.AddListener(HandleCommand);
        inputField.ActivateInputField(); // 커서 자동 활성화
    }

    void HandleCommand(string command)
    {
        command = command.Trim().ToLower();

        if (command == "help")
        {
            PrintToOutput("Commands:\n- open [module]\n- list\n- clear");
        }
        else if (command == "list")
        {
            PrintToOutput("Available modules:\n" + string.Join("\n", manualDB.GetModuleNames()));
        }
        else if (command.StartsWith("open "))
        {
            string module = command.Substring(5);
            string result = manualDB.GetManualText(module);
            if (result != null)
                PrintToOutput($"[{module.ToUpper()} Manual]\n{result}");
            else
                PrintToOutput("No such module.");
        }
        else if (command == "clear")
        {
            outputText.text = "";
        }
        else
        {
            PrintToOutput("Unknown command. Type 'help' for a list.");
        }

        inputField.text = "";
        inputField.ActivateInputField();
    }

    void PrintToOutput(string msg)
    {
        outputText.text += $"\n> {msg}";

        // 👇 스크롤을 항상 아래로 유지
        Canvas.ForceUpdateCanvases(); // UI 강제 업데이트
        scrollRect.verticalNormalizedPosition = 0f; // 0이면 맨 아래
        Canvas.ForceUpdateCanvases();
    }
}
