using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour, IGameStartListener
{
    public static GameManager Instance;

    public int gold = 20;

    [Header("UI")]
    public TMP_Text goldText;
    public UnityEngine.UI.Button drawTowerButton;
    public TMP_Text buttonText;
    public TMP_Text waveText;
    public TMP_Text timeText;

    [Header("Tower")]
    public TowerInventory inventory;
    public List<TowerData> lv1Towers;

    private Coroutine gameTimeCoroutine;
    private float elapsed = 0f;
    private bool isTimePaused = false;

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

        // GameStateManager에 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener(this);
        }
        else
        {
            Debug.LogWarning("[GameManager] GameStateManager 인스턴스가 없습니다.");
        }
    }
    public void StartButtonClicked()
    {
        GameStateManager.Instance.ChangeState(GameState.Start);
    }

    public void OnGameStart()
    {
        gameTimeCoroutine = StartCoroutine(UpdateGameTime());
    }

    IEnumerator UpdateGameTime()
    {
        while (true)
        {
            if (!isTimePaused)
            {
                elapsed += Time.deltaTime;

                if (timeText != null)
                    timeText.text = $"<color=black>Time: {elapsed:F1}s</color>";
            }

            yield return null;
        }
    }

    public void UpdateWaveUI(int wave)
    {
        if (waveText != null)
            waveText.text = $"<color=black>Wave: {wave}</color>";
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
            goldText.text = $"<color=#E4AF31>Gold: {gold}</color>";
    }

    void UpdateDrawButtonState()
    {
        if (drawTowerButton != null)
            drawTowerButton.interactable = gold >= 10;

        if (buttonText != null)
        {
            if (gold >= 10)
                buttonText.text = "<color=#338CF1>DrawTower(10G)</color>";
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
    
    public void PauseGameTime()
    {
        isTimePaused = true;
    }

    public void ResumeGameTime()
    {
        isTimePaused = false;
    }

    public void ResetGameTime()
    {
        elapsed = 0f;
        if (timeText != null)
            timeText.text = "<color=black>Time: 0.0s</color>";
    }
}
