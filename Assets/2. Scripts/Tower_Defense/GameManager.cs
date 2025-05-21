using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int gold = 20;  // 시작 시 20골드 지급

    [Header("UI")]
    public TMP_Text goldText;                // 골드 표시 텍스트
    public Button drawTowerButton;           // 타워 뽑기 버튼
    public TMP_Text buttonText;              // 버튼 내부 텍스트

    [Header("Tower")]
    public TowerInventory inventory;         // 인벤토리 컴포넌트 (에디터에서 연결)
    public List<TowerData> lv1Towers;        // 1레벨 타워들 (랜덤 뽑기용)

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateGoldUI();
        UpdateDrawButtonState();

        if (drawTowerButton != null)
            drawTowerButton.onClick.AddListener(OnDrawTowerButtonClicked);
    }

    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
        UpdateDrawButtonState();
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            UpdateGoldUI();
            UpdateDrawButtonState();
            return true;
        }
        return false;
    }

    void UpdateGoldUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }

    void UpdateDrawButtonState()
    {
        if (drawTowerButton != null)
            drawTowerButton.interactable = gold >= 10;

        if (buttonText != null)
        {
            if (gold >= 10)
                buttonText.text = "<color=#338CF1>타워뽑기(10G)</color>";
            else
                buttonText.text = "<color=red>Not Enough Gold</color>";
        }
    }

    void OnDrawTowerButtonClicked()
    {
        if (SpendGold(10))
        {
            TowerData randomTower = GetRandomLv1Tower();
            if (randomTower != null && inventory != null)
            {
                inventory.AddTower(randomTower);
            }
        }
    }

    TowerData GetRandomLv1Tower()
    {
        if (lv1Towers == null || lv1Towers.Count == 0) return null;
        int idx = Random.Range(0, lv1Towers.Count);
        return lv1Towers[idx];
    }
}