using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections.Generic;

public class RoleSelectUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text m_player_name;
    [SerializeField] private UnityEngine.UI.Button m_left_button;
    [SerializeField] private UnityEngine.UI.Button m_right_button;

    public Sprite[] m_player_character_sprites;
    private NetworkPlayer m_network_role;

    public override void Spawned()
    {
        m_network_role = GetComponent<NetworkPlayer>();

        // m_roles = (RoleType[])System.Enum.GetValues(typeof(RoleType));

        // UpdateRoleText();

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
            currentIndex = m_player_character_sprites.Length - 1;

        m_network_role.SetPlayerCharacterSpriteIndex(currentIndex);
    }

    private void SelectRight()
    {
        int currentIndex = GetCurrentRoleIndex();
        currentIndex++;
        if (currentIndex >= m_player_character_sprites.Length)
            currentIndex = 0;

        m_network_role.SetPlayerCharacterSpriteIndex(currentIndex);
    }

    private int GetCurrentRoleIndex()
    {
        Sprite currentCharacterSprite = m_player_character_sprites[m_network_role.m_player_character_sprite_index];
        for (int i = 0; i < m_player_character_sprites.Length; i++)
        {
            if (m_player_character_sprites[i] == currentCharacterSprite)
                return i;
        }
        return 0;
    }

    private void UpdateRoleText()
    {
        // m_role_text.text = m_network_role.m_player_role.ToString();
        m_network_role.SetPlayerName(m_player_name.text);
    }
}