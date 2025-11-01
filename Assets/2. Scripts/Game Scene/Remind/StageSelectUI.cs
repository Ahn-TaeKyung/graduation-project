// 파일명: StageSelectUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// 이 스크립트는 로컬 UI만 제어합니다. NetworkBehaviour가 아닙니다.
public class StageSelectUI : MonoBehaviour, IGameReadyListener
{
    [Header("UI References")]
    [SerializeField] private RectTransform m_player_icon;
    // [SerializeField] private Animator m_player_icon_animator;
    [SerializeField] private RectTransform[] m_icon_path;
    [SerializeField] private Button[] m_select_buttons;
    [SerializeField] private GameObject m_stage_select_check_UI; // "예/아니오" 팝업
    [SerializeField] private Button m_button_Yes;
    [SerializeField] private Button m_button_No;
    [SerializeField] private TMP_Text m_popup_text;
    
    // [Header("Stage Sprites")]
    // (당신의 StageManager.cs에 있던 스프라이트 참조들...)

    private int m_current_index; // 현재 플레이어가 *선택한* 스테이지
    private int m_now_button_index = 0; // 아이콘의 현재 *위치*
    private List<int> m_path_list = new List<int>();
    private int m_current_path_index = 0;
    private bool m_is_icon_moving = false;
    private float m_icon_move_speed = 250f;
    
    private bool _isHost = false;

    void Start()
    {
        // 씬이 로드될 때, GameStateManager가 스폰될 때까지 기다렸다가 권한을 확인합니다.
        // GameStateManager의 IGameReadyListener를 활용하여 안전하게 초기화합니다.
        if (GameStateManager.Instance != null)
        {
            // GameStateManager에 리스너로 등록
            GameStateManager.Instance.RegisterListener((IGameReadyListener)this);
        }

        // "예", "아니오" 버튼에 리스너 연결
        m_button_Yes.onClick.AddListener(StageSelectYes);
        m_button_No.onClick.AddListener(StageSelectNo);

        m_stage_select_check_UI.SetActive(false);
    }
    
    // GameStateManager가 Spawned되고 Ready 상태가 되었을 때 호출됨
    public void OnGameReady()
    {
        // 호스트인지 확인
        if (GameStateManager.Instance.Object.HasStateAuthority)
        {
            _isHost = true;
            Debug.Log("[StageSelectUI] 당신은 Host입니다. 스테이지 선택이 가능합니다.");
            InitializeButtons_Host();
        }
        else
        {
            _isHost = false;
            Debug.Log("[StageSelectUI] 당신은 Client입니다. 스테이지 선택이 비활성화됩니다.");
            InitializeButtons_Client();
        }
        
        // 맵 UI 패널을 활성화
        gameObject.SetActive(true);
    }
    
    // 호스트일 때: 버튼에 리스너를 추가하고 상호작용 가능하게 함
    private void InitializeButtons_Host()
    {
        for (int i = 0; i < m_select_buttons.Length; i++)
        {
            int stageIndex = i; // 클로저 문제 방지
            m_select_buttons[i].interactable = true; // TODO: SaveManager 로직으로 변경
            m_select_buttons[i].onClick.AddListener(() => StageSelect(stageIndex));
            // (TODO: SaveManager.Instance.Player.m_max_clear_stage에 따른 스프라이트 변경 로직)
        }
    }

    // 클라이언트일 때: 모든 버튼을 비활성화
    private void InitializeButtons_Client()
    {
        for (int i = 0; i < m_select_buttons.Length; i++)
        {
            m_select_buttons[i].interactable = false;
        }
    }

    // (당신의 StageManager.cs에서 가져온 로직)
    public void StageSelect(int stage_index)
    {
        if (!_isHost || m_is_icon_moving) return; // 호스트가 아니거나 아이콘이 움직이는 중이면 무시
        
        m_current_index = stage_index;
        m_popup_text.text = $"스테이지 {m_current_index + 1}";
        
        // (당신의 경로 계산 로직)
        int button_index = stage_index * 2; // (예시 로직, 필요시 수정)
        if(stage_index == 9) button_index = stage_index * 2 - 1;
        
        ClickedButtonPath(button_index);
    }

    // (당신의 StageManager.cs에서 가져온 로직)
    public void StageSelectYes()
    {
        if (!_isHost) return;

        m_stage_select_check_UI.SetActive(false);
        // SoundManager.Instance.PlayEffect("ui_map_go");
        
        // [핵심] GameStateManager의 HostSelectStage 함수 호출
        GameStateManager.Instance.HostSelectStage(m_current_index);
        
        // 맵 UI 숨기기
        gameObject.SetActive(false);
    }

    // (당신의 StageManager.cs에서 가져온 로직)
    public void StageSelectNo()
    {
        m_stage_select_check_UI.SetActive(false);
        // SoundManager.Instance.PlayEffect("ui_map_go");
    }

    // (이하 당신의 StageManager.cs에 있던 아이콘 이동 로직)
    // (StartCoroutine, ClickedButtonPath, MoveIconCorutine 등...)
    
    public void ClickedButtonPath(int button_index)
    {                                         
        m_path_list.Clear();
        m_current_path_index = 0; 

        if (m_now_button_index < button_index)
        {
            for (int i = m_now_button_index + 1; i <= button_index; i++) m_path_list.Add(i);
        }
        else if (m_now_button_index > button_index)
        {
            for (int i = m_now_button_index - 1; i >= button_index; i--) m_path_list.Add(i);
        }
        else
        {
            m_path_list.Add(m_now_button_index);
        }
        m_is_icon_moving = true;
        m_now_button_index = button_index;
        StartCoroutine(MoveIconCorutine());
    }

    private IEnumerator MoveIconCorutine()
    {
        // SoundManager.Instance.PlayEffect("ui_map_walk");
        
        while (m_is_icon_moving && m_path_list.Count > 0)
        {
            int target_index = m_path_list[m_current_path_index]; 
            Vector2 target_pos = m_icon_path[target_index].GetComponent<RectTransform>().anchoredPosition;

            while (Vector2.Distance(m_player_icon.anchoredPosition, target_pos) > 0.1f)
            {
                m_player_icon.anchoredPosition = Vector2.MoveTowards(m_player_icon.anchoredPosition, target_pos, m_icon_move_speed * Time.deltaTime);
                // (애니메이터 로직)
                yield return null; 
            }
            
            if (m_current_path_index < m_path_list.Count - 1)
            {
                m_current_path_index++;
            }
            else
            {
                m_is_icon_moving = false;
                // (애니메이터 로직)
                m_stage_select_check_UI.SetActive(true); // 팝업 띄우기
                // SoundManager.Instance.StopEffect();
            }
        }
    }
}