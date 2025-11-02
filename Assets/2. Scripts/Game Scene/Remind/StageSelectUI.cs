// 파일명: StageSelectUI.cs (Polling 방식 최종본)
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Fusion; // NetworkRunner를 찾기 위해 추가

// [수정] 맵 캔버스가 켜지는/꺼지는 시점을 GameStateManager로부터 받음
public class StageSelectUI : MonoBehaviour, IGameReadyListener
{
    [Header("UI References")]
    [SerializeField] private RectTransform m_player_icon;
    [SerializeField] private Animator m_player_icon_animator;
    [SerializeField] private RectTransform[] m_icon_path;
    [SerializeField] private Button[] m_select_buttons;
    [SerializeField] private GameObject m_stage_select_check_UI;
    [SerializeField] private Button m_button_Yes;
    [SerializeField] private Button m_button_No;
    [SerializeField] private TMP_Text m_popup_text;
    
    // [로컬]
    private int m_current_index_local; // 호스트가 팝업에 띄울 스테이지 인덱스
    private int m_now_button_index_local = 0; // 아이콘의 현재 '표시' 위치
    private List<int> m_path_list = new List<int>();
    private int m_current_path_index = 0;
    private bool m_is_icon_moving = false;
    private float m_icon_move_speed = 250f;
    private bool m_can_select = true;
    private bool _isHost = false;
    private NetworkRunner _runner;

    // [수정] Start()에서 리스너 등록
    void Start()
    {
        // 씬 로드 시 GameStateManager를 찾아서 리스너 등록
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RegisterListener((IGameReadyListener)this);
        }

        m_button_Yes.onClick.AddListener(StageSelectYes);
        m_button_No.onClick.AddListener(StageSelectNo);
        m_stage_select_check_UI.SetActive(false);

        _runner = FindObjectOfType<NetworkRunner>();
        if (_runner != null)
        {
            _isHost = _runner.IsServer; // IsHost 대신 IsServer
        }
        
        // [수정] GameStateManager가 켜주므로 Start에서 끄지 않습니다.
        // gameObject.SetActive(false);
    }
    
    private void OnDestroy()
    {
        // 씬 전환 시 리스너 해제
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.UnregisterListener((IGameReadyListener)this);
        }
    }
    
    // GameStateManager가 'Ready' 상태가 될 때 호출됨
    public void OnGameReady()
    {
        // GameStateManager가 스폰되었으므로, 정확한 권한 확인
        if (GameStateManager.Instance.Object.IsValid)
        {
            _isHost = GameStateManager.Instance.Object.HasStateAuthority;
        }

        if (_isHost)
        {
            Debug.Log("[StageSelectUI] 당신은 Host입니다. 스테이지 선택이 가능합니다.");
            InitializeButtons_Host();
        }
        else
        {
            Debug.Log("[StageSelectUI] 당신은 Client입니다. 스테이지 선택이 비활성화됩니다.");
            InitializeButtons_Client();
        }
        
        // GameStateManager의 현재 아이콘 위치로 아이콘 강제 이동 (재시작 시 0으로)
        // int networkedIconIndex = GameStateManager.Instance.CurrentIconIndex;
        // UpdateIconPositionImmediate(networkedIconIndex);
        // m_now_button_index_local = networkedIconIndex;
        m_can_select = true;
        m_is_icon_moving = false;
        
        // [참고] 이 캔버스는 GameStateManager가 이미 켠 상태입니다.
    }
    
    // [수정] Polling을 위해 Update() 사용
    private void Update()
    {
        // 이 캔버스가 비활성화되어 있거나 (GameStateManager가 껐음),
        // GameStateManager가 없거나, 스폰되지 않았으면 실행 안 함
        if (!gameObject.activeInHierarchy || GameStateManager.Instance == null) return;
        if (GameStateManager.Instance.Object == null)
        {
            return;
        }
        if (!GameStateManager.Instance.Object.IsValid)
        {
            return;
        }

        // --- Polling: 아이콘 위치 동기화 ---
        int networkedIconIndex = GameStateManager.Instance.CurrentIconIndex;

        // 로컬 아이콘 위치와 네트워크 아이콘 위치가 다르고,
        // 현재 아이콘이 움직이는 중이 아닐 때 (클라이언트만 해당)
        if (networkedIconIndex != m_now_button_index_local && !m_is_icon_moving)
        {
            // 클라이언트가 호스트의 움직임을 따라가도록 애니메이션 시작
            Debug.Log($"[StageSelectUI] (Polling) 아이콘 위치 동기화: {m_now_button_index_local} -> {networkedIconIndex}");
            StartLocalIconAnimation(m_now_button_index_local, networkedIconIndex);
            m_now_button_index_local = networkedIconIndex; // 로컬 인덱스를 네트워크 값으로 즉시 갱신
        }
    }
    
    private void InitializeButtons_Host()
    {
        for (int i = 0; i < m_select_buttons.Length; i++)
        {
            int stageIndex = i; // 클로저 문제 방지
            m_select_buttons[i].interactable = true; 
            m_select_buttons[i].onClick.RemoveAllListeners();
            m_select_buttons[i].onClick.AddListener(() => StageSelect_Host(stageIndex));
        }
    }

    private void InitializeButtons_Client()
    {
        for (int i = 0; i < m_select_buttons.Length; i++)
        {
            m_select_buttons[i].interactable = false;
        }
    }

    // 1. 호스트만 이 함수를 호출 (클릭 시)
    public void StageSelect_Host(int stage_index)
    {
        if (!_isHost || m_is_icon_moving || !m_can_select) return; 
        
        m_current_index_local = stage_index; 
        m_popup_text.text = $"스테이지 {m_current_index_local}";
        
        int button_index = stage_index * 2; 
        if(stage_index == 9) button_index = stage_index * 2 - 1;
        
        // [핵심 수정] 호스트가 GameStateManager의 CurrentIconIndex를 *즉시* 변경합니다.
        // 그러면 Host/Client 모두의 Update() Polling 로직이 이 변경을 감지하고 애니메이션을 시작합니다.
        if (GameStateManager.Instance != null && GameStateManager.Instance.Object.HasStateAuthority)
        {
             // Host가 네트워크 변수를 직접 변경
             GameStateManager.Instance.CurrentIconIndex = button_index;
        }
    }
    
    // 2. "예" 버튼 (호스트만 클릭 가능)
    public void StageSelectYes()
    {
        if (!_isHost) return;
        m_stage_select_check_UI.SetActive(false);
        
        // GameStateManager에 스테이지 인덱스와 '아이콘의 최종 위치'를 전달
        GameStateManager.Instance.HostSelectStage(m_current_index_local, m_now_button_index_local);
    }

    public void StageSelectNo()
    {
        m_stage_select_check_UI.SetActive(false);
        m_can_select = true; // 선택 취소
    }

    // 3. (로컬) 아이콘 애니메이션 실행
    private void StartLocalIconAnimation(int from_button_index, int to_button_index)
    {
        if (m_is_icon_moving)
        {
            StopCoroutine("MoveIconCorutine");
        }

        m_path_list.Clear();
        m_current_path_index = 0; 

        if (from_button_index < to_button_index) // 정방향
        {
            for (int i = from_button_index + 1; i <= to_button_index; i++) m_path_list.Add(i);
        }
        else if (from_button_index > to_button_index) // 역방향
        {
            for (int i = from_button_index - 1; i >= to_button_index; i--) m_path_list.Add(i);
        }
        else // 같은 위치 (애니메이션 필요 없음)
        {
             UpdateIconPositionImmediate(from_button_index);
             return;
        }

        m_is_icon_moving = true;
        StartCoroutine(MoveIconCorutine());
    }

    private IEnumerator MoveIconCorutine()
    {
        m_can_select = false;

        while (m_is_icon_moving && m_path_list.Count > 0)
        {
            if (m_current_path_index < 0 || m_current_path_index >= m_path_list.Count)
            { m_is_icon_moving = false; yield break; }

            int target_index = m_path_list[m_current_path_index]; 
            
            if (target_index < 0 || target_index >= m_icon_path.Length)
            { m_is_icon_moving = false; yield break; }
            
            Vector2 target_pos = m_icon_path[target_index].anchoredPosition;

            while (Vector2.Distance(m_player_icon.anchoredPosition, target_pos) > 0.1f)
            {
                m_player_icon.anchoredPosition = Vector2.MoveTowards(m_player_icon.anchoredPosition, target_pos, m_icon_move_speed * Time.deltaTime);
                if (m_player_icon_animator)
                {
                    m_player_icon_animator.SetBool("IsMove", true);
                    m_player_icon_animator.SetFloat("DirX", (target_pos - m_player_icon.anchoredPosition).normalized.x);
                    m_player_icon_animator.SetFloat("DirY", (target_pos - m_player_icon.anchoredPosition).normalized.y);
                }
                yield return null; 
            }
            
            if (m_current_path_index < m_path_list.Count - 1)
            {
                m_current_path_index++;
            }
            else
            {
                m_is_icon_moving = false;
                if (m_player_icon_animator)
                {
                    m_player_icon_animator.SetBool("IsMove", false);
                    m_player_icon_animator.SetFloat("DirX", 0f);
                    m_player_icon_animator.SetFloat("DirY", -1f);
                }
                
                // [수정] 호스트이고, 같은 버튼을 클릭한게 아닐때만 팝업.
                if (_isHost && m_path_list.Count > 0) 
                {
                    m_stage_select_check_UI.SetActive(true);
                }
                m_can_select = true;
            }
        }
    }
    
    private void UpdateIconPositionImmediate(int button_index)
    {
        if (button_index < 0 || button_index >= m_icon_path.Length) return;
        
        Vector2 target_pos = m_icon_path[button_index].GetComponent<RectTransform>().anchoredPosition;
        m_player_icon.anchoredPosition = target_pos;
    }
}