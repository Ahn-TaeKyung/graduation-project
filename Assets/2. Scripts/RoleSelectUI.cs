using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class RoleSelectUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text m_role_text;
    [SerializeField] private Button m_left_button;
    [SerializeField] private Button m_right_button;

    private RoleType[] m_roles;
    private NetworkRole m_network_role;

    public override void Spawned()
    {
        m_network_role = GetComponent<NetworkRole>();

        m_roles = (RoleType[])System.Enum.GetValues(typeof(RoleType));

        UpdateRoleText();

        m_left_button.onClick.AddListener(SelectLeft);
        m_right_button.onClick.AddListener(SelectRight);

        if (!Object.HasInputAuthority)
        {
            m_left_button.interactable = false;
            m_right_button.interactable = false;
        }
    }

    private void Update()
    {
        // 매 프레임마다 현재 역할을 텍스트에 반영 (변경 감지용)
        UpdateRoleText();
    }

    private void SelectLeft()
    {
        int currentIndex = GetCurrentRoleIndex();
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = m_roles.Length - 1;

        m_network_role.SetPlayerRole(m_roles[currentIndex]);
    }

    private void SelectRight()
    {
        int currentIndex = GetCurrentRoleIndex();
        currentIndex++;
        if (currentIndex >= m_roles.Length)
            currentIndex = 0;

        m_network_role.SetPlayerRole(m_roles[currentIndex]);
    }

    private int GetCurrentRoleIndex()
    {
        RoleType currentRole = m_network_role.m_player_role;
        for (int i = 0; i < m_roles.Length; i++)
        {
            if (m_roles[i] == currentRole)
                return i;
        }
        return 0;
    }

    private void UpdateRoleText()
    {
        m_role_text.text = m_network_role.m_player_role.ToString();
    }
}
