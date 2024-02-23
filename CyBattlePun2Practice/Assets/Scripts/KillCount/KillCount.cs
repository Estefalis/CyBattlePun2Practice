using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillCount : MonoBehaviour
{
    [SerializeField] private GameObject m_namesBackground;
    [SerializeField] private Transform m_killCountBackground;

    [SerializeField] private TMP_Text[] m_playerNames;
    [SerializeField] private TMP_Text[] m_killAmounts;
    [SerializeField] private GameObject[] m_killCountChildren;

    [Header("OnGameWon")]
    [SerializeField] private Transform m_winnerBackground;
    [SerializeField] private TMP_Text m_winnerText;
    [SerializeField] private TMP_Text m_escapeMessage;

    [SerializeField] private List<Kills> m_highestKills = new();
    private Player[] m_sortPlayerList;

    private bool m_killCountVisible = false;
    private bool m_matchRuns = false;

    private void Awake()
    {
        m_winnerBackground.gameObject.SetActive(false);
        m_escapeMessage.gameObject.SetActive(false);
        m_killCountBackground.gameObject.SetActive(false);

        CustomTimer.KillCountMatchRuns += MatchRunsNow;
        CustomTimer.KillCountMatchEnds += MatchEndsNow;
        CustomTimer.NoRespawnMatchRuns += MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds += MatchEndsNow;
        AvatarColor.LastPlayerLeftOnNoRespawn += EscapeHint;
    }

    private void OnDisable()
    {
        CustomTimer.KillCountMatchRuns -= MatchRunsNow;
        CustomTimer.KillCountMatchEnds -= MatchEndsNow;
        CustomTimer.NoRespawnMatchRuns -= MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds -= MatchEndsNow;
        AvatarColor.LastPlayerLeftOnNoRespawn -= EscapeHint;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && m_matchRuns)
        {
            m_killCountVisible = !m_killCountVisible;
            ToggleKillListVisibility(m_killCountVisible);
        }
    }

    private void EscapeHint()
    {
        m_escapeMessage.gameObject.SetActive(true);
    }

    private void ToggleKillListVisibility(bool _killCountVisible)
    {
        m_killCountBackground.gameObject.SetActive(_killCountVisible);

        switch (_killCountVisible)
        {
            case true:
            {
                m_sortPlayerList = PhotonNetwork.PlayerList; //Je nach Player in Arena.
                m_highestKills.Clear();

                TMP_Text[] playerListArray = m_namesBackground.GetComponent<HUDNameHealth>().Names;
                uint[] currentKillCount = m_namesBackground.GetComponent<HUDNameHealth>().CurrentKillCount;

                for (int i = 0; i < m_playerNames.Length; i++)
                {
                    //TMP_Text in Canvas/NamesBackground GameObject.
                    m_highestKills.Add(new Kills(playerListArray[i].text, (int)currentKillCount[i]));   //Immer 6 Entries.
                }

                if (m_highestKills.Count > 0)
                    m_highestKills.Sort();  //return other.m_kills - m_kills; in Kills.cs. (KILLS!!! <(~.^)")

                m_winnerText.text = $"{m_highestKills[0].m_playerName} won!";

                for (int i = 0; i < m_playerNames.Length; i++)
                {
                    m_playerNames[i].text = m_highestKills[i].m_playerName;
                    m_killAmounts[i].text = $"{m_highestKills[i].m_kills}";
                }

                for (int i = 0; i < m_highestKills.Count; i++)
                {
                    for (int j = 0; j < m_sortPlayerList.Length; j++)
                    {
                        if (m_highestKills[i].m_playerName != m_sortPlayerList[j].NickName)
                        {
                            m_killCountChildren[i].SetActive(false);
                        }
                    }
                }

                for (int i = 0; i < m_highestKills.Count; i++)
                {
                    for (int j = 0; j < m_sortPlayerList.Length; j++)
                    {
                        if (m_highestKills[i].m_playerName == m_sortPlayerList[j].NickName)
                        {
                            m_killCountChildren[i].SetActive(true);
                        }
                    }
                }

                break;
            }
            case false:
            {
                for (int i = 0; i < m_highestKills.Count; i++)
                {
                    m_killCountChildren[i].SetActive(true);
                }

                break;
            }
        }
    }

    private void MatchRunsNow(int _eMatchMode)
    {
        switch (_eMatchMode)
        {
            case (int)EMatchMode.KillCount:
            case (int)EMatchMode.NoRespawn:
            {
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
            case (int)EMatchMode.KillCount:
            {
                m_winnerBackground.gameObject.SetActive(true);
                m_escapeMessage.gameObject.SetActive(true);
                m_matchRuns = false;
                ToggleKillListVisibility(true);
                break;
            }
            case (int)EMatchMode.NoRespawn:
            {
                //TODO: End conditions for NoRespawn Match.
                break;
            }
            default:
                break;
        }
    }
}