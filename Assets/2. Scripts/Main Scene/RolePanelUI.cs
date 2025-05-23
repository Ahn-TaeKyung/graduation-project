using Fusion;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RolePanelUI : MonoBehaviour
{
    public Button m_left_button;
    public Button m_right_button;
    public TextMeshProUGUI m_role_text;

    private PlayerRef m_my_player;
    private int m_current_role = 0;
    private string[] m_role_names = { "방어자", "해체자", "매뉴얼 가이드" };

    private void Start()
    {
        m_my_player = NetworkRunner.GetRunnerForGameObject(gameObject).LocalPlayer;

        m_left_button.onClick.AddListener(OnLeftClick);
        m_right_button.onClick.AddListener(OnRightClick);

        UpdateRoleText();
    }

    private void OnLeftClick()
    {
        m_current_role = (m_current_role + m_role_names.Length - 1) % m_role_names.Length;
        RoomManager.Instance.SetRole(m_my_player, m_current_role);
        UpdateRoleText();
    }

    private void OnRightClick()
    {
        m_current_role = (m_current_role + 1) % m_role_names.Length;
        RoomManager.Instance.SetRole(m_my_player, m_current_role);
        UpdateRoleText();
    }

    private void UpdateRoleText()
    {
        m_role_text.text = m_role_names[m_current_role];
    }
}
