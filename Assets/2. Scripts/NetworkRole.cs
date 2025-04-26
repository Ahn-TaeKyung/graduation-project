using Fusion;

public class NetworkRole : NetworkBehaviour
{
    // [Networked]
    // public RoleType PlayerRole { get; set; } = RoleType.None;

    [Networked]
    public RoleType m_player_role { get; private set; }
    public void SetPlayerRole(RoleType role)
    {
        if (HasInputAuthority) // 본인만 변경 가능
        {
            RPC_SetPlayerRole(role);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerRole(RoleType role)
    {
        m_player_role = role;
    }
}
