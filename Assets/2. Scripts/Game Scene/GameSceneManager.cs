using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class GameSceneManager : NetworkBehaviour
{
    public Camera m_camera_defender;
    public Camera m_camera_hacker;
    public Camera m_camera_guide;
    public GameObject defenseCanvas;
    public GameObject hackerCanvas;
    public GameObject guideCanvas;

    private RoleType m_my_role;

    public override void Spawned()
    {
        List<RoleData> roles = RoleDataManager.LoadRoles();

        int myId = NetworkRunner.GetRunnerForGameObject(gameObject).LocalPlayer.PlayerId;
        foreach (var role in roles)
        {
            if (role.m_player_id == myId)
            {
                m_my_role = role.m_role;
                break;
            }
        }
        AssignCameraToPlayer(m_my_role);
    }

    private void AssignCameraToPlayer(RoleType role)
    {
        m_camera_defender.enabled = false;
        m_camera_hacker.enabled = false;
        m_camera_guide.enabled = false;

        switch (role)
        {
            case RoleType.Defender:
                m_camera_defender.enabled = true;
                defenseCanvas.SetActive(true);
                break;
            case RoleType.Hacker:
                m_camera_hacker.enabled = true;
                hackerCanvas.SetActive(true);
                break;
            case RoleType.Guide:
                m_camera_guide.enabled = true;
                guideCanvas.SetActive(true);
                break;
            default:
                Debug.LogWarning("알 수 없는 역할");
                defenseCanvas.SetActive(false);
                hackerCanvas.SetActive(false);
                guideCanvas.SetActive(false);
                break;
        }

        Debug.Log($"[GameSceneInitializer] 내 역할: {role} → 카메라 활성화됨 / canvas 활성화됨");
    }
}