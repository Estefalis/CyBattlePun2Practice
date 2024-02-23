using Photon.Pun;
using UnityEngine;

public class OurAimLookAt : MonoBehaviour
{
    private GameObject m_lookAtObject;

    private bool m_isDead = false;
    private bool m_matchRuns = false;
    private bool m_respawnIsBlocked = false;

    private PhotonView m_photonView;

    private void Awake()
    {
        CustomTimer.KillCountMatchRuns += MatchRunsNow;
        CustomTimer.KillCountMatchEnds += MatchEndsNow;
        CustomTimer.TeamBattleMatchRuns += MatchRunsNow;
        CustomTimer.TeamBattleMatchEnds += MatchEndsNow;
        CustomTimer.NoRespawnMatchRuns += MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds += MatchEndsNow;
        m_photonView = GetComponentInParent<PhotonView>();
        AvatarColor.IsDead += WhenCharacterDies;
        AvatarColor.IsAliveAgain += WhenCharacterRespawns;
    }

    private void OnDisable()
    {
        CustomTimer.KillCountMatchRuns -= MatchRunsNow;
        CustomTimer.KillCountMatchEnds -= MatchEndsNow;
        CustomTimer.TeamBattleMatchRuns -= MatchRunsNow;
        CustomTimer.TeamBattleMatchEnds -= MatchEndsNow;
        CustomTimer.NoRespawnMatchRuns -= MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds -= MatchEndsNow;

        AvatarColor.IsDead -= WhenCharacterDies;
        AvatarColor.IsAliveAgain -= WhenCharacterRespawns;
    }

    private void Start()
    {
        m_lookAtObject = GameObject.Find("Targeting");
    }

    private void FixedUpdate()
    {
        if (!m_isDead && m_matchRuns)
        {
            if (m_photonView.IsMine)
                this.transform.position = m_lookAtObject.transform.position;
        }
    }

    private void MatchRunsNow(int _eMatchMode)
    {
        switch (_eMatchMode)
        {
            case (int)EMatchMode.NoRespawn:
            {
                m_respawnIsBlocked = true;
                m_matchRuns = true;
                break;
            }
            default:
            {
                m_matchRuns = true;
                break;
            }
        }
    }

    private void MatchEndsNow(int _eMatchMode)
    {
        switch (_eMatchMode)
        {
            case (int)EMatchMode.NoRespawn:
            {
                break;
            }
            default:
            {
                m_matchRuns = false;
                break;
            }
        }
    }

    private void WhenCharacterDies(bool _charIsDead, int _viewID)
    {
        if (m_photonView.ViewID == _viewID && !m_respawnIsBlocked)
            m_isDead = _charIsDead;
    }

    private void WhenCharacterRespawns(bool _charIsDead, int _viewID)
    {
        if (m_photonView.ViewID == _viewID)
        {
            m_isDead = _charIsDead;
        }
    }
}