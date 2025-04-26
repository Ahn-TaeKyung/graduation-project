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
    private int m_current_role_index = 0;
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

    private void SelectLeft()
    {
        m_current_role_index--;
        if (m_current_role_index < 0)
            m_current_role_index = m_roles.Length - 1;

        UpdateRole();
    }

    private void SelectRight()
    {
        m_current_role_index++;
        if (m_current_role_index >= m_roles.Length)
            m_current_role_index = 0;

        UpdateRole();
    }

    private void UpdateRole()
    {
        m_network_role.SetPlayerRole(m_roles[m_current_role_index]);
        UpdateRoleText();
    }

    private void UpdateRoleText()
    {
        m_role_text.text = m_roles[m_current_role_index].ToString();
    }
}
