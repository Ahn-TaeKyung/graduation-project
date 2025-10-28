using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{

    [Networked]
    public string m_player_name { get; private set; }
    [Networked]
    public int m_player_character_sprite_index { get; private set; }
    public void SetPlayerName(string name)
    {
        if (HasInputAuthority) // 본인만 변경 가능
        {
            RPC_SetPlayerName(name);
        }
    }
    public void SetPlayerCharacterSpriteIndex(int spriteIndex)
    {
        if (HasInputAuthority) // 본인만 변경 가능
        {
            RPC_SetPlayerCharacterSpriteIndex(spriteIndex);
        }
        
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string name)
    {
        m_player_name = name;
    }
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerCharacterSpriteIndex(int spriteIndex)
    {
        m_player_character_sprite_index = spriteIndex;
    }
}