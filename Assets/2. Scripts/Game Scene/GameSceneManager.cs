using Fusion;
using UnityEngine;

public class GameSceneManager : NetworkBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    public Camera m_camera_defender;
    public Camera m_camera_hacker;
    public Camera m_camera_guide;

    public GameObject defenseCanvas;
    public GameObject hackerCanvas;
    public GameObject guideCanvas;

    private RoleType m_my_role;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void Spawned()
    {
        base.Spawned();

        // 역할 정보 로드 후 내 역할 저장
        var roles = RoleDataManager.LoadRoles();

        int myId = NetworkRunner.GetRunnerForGameObject(gameObject).LocalPlayer.PlayerId;
        foreach (var role in roles)
        {
            if (role.m_player_id == myId)
            {
                m_my_role = role.m_role;
                break;
            }
        }
    }
    public RoleType GetMyRole()
    {
        return m_my_role;
    }

    public void SetupByRole()
    {
        m_camera_defender.enabled = false;
        m_camera_hacker.enabled = false;
        m_camera_guide.enabled = false;

        defenseCanvas.SetActive(false);
        hackerCanvas.SetActive(false);
        guideCanvas.SetActive(false);

        switch (m_my_role)
        {
            case RoleType.Defender:
                Debug.Log("디펜더 셋업 완료");
                m_camera_defender.gameObject.SetActive(true);
                m_camera_defender.enabled = true;
                defenseCanvas.SetActive(true);
                break;
            case RoleType.Hacker:
                Debug.Log("해커 셋업 완료");
                m_camera_hacker.gameObject.SetActive(true);
                m_camera_hacker.enabled = true;
                hackerCanvas.SetActive(true);
                break;
            case RoleType.Guide:
                Debug.Log("가이드 셋업 완료");
                m_camera_guide.gameObject.SetActive(true);
                m_camera_guide.enabled = true;
                guideCanvas.SetActive(true);
                break;
            default:
                Debug.LogWarning("알 수 없는 역할");
                break;
        }

        Debug.Log($"[GameSceneManager] 역할 셋업 완료: {m_my_role}");
    }
}
