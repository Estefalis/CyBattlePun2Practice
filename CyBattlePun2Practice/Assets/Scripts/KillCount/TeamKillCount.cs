using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeamKillCount : MonoBehaviour
{
    [SerializeField] private GameObject m_namesBackground;
    [SerializeField] private Transform m_killCountBackground;

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

    private int m_TeamOneKills;
    private int m_TeamTwoKills;

    private void Awake()
    {
        m_winnerBackground.gameObject.SetActive(false);
        m_escapeMessage.gameObject.SetActive(false);
        m_killCountBackground.gameObject.SetActive(false);

        CustomTimer.TeamBattleMatchRuns += MatchRunsNow;
        CustomTimer.TeamBattleMatchEnds += MatchEndsNow;
    }

    private void OnDisable()
    {
        CustomTimer.TeamBattleMatchRuns -= MatchRunsNow;
        CustomTimer.TeamBattleMatchEnds -= MatchEndsNow;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && m_matchRuns)
        {
            m_killCountVisible = !m_killCountVisible;
            ToggleKillListVisibility(m_killCountVisible);
        }
    }

    private void ToggleKillListVisibility(bool _killCountVisible)
    {
        m_killCountBackground.gameObject.SetActive(_killCountVisible);

        switch (_killCountVisible)
        {
            case true:
            {
                m_sortPlayerList = PhotonNetwork.PlayerList;                                            //Je nach Player in Arena.
                m_highestKills.Clear();

                TMP_Text[] playerListArray = m_namesBackground.GetComponent<HUDNameHealth>().Names;
                uint[] currentKillCount = m_namesBackground.GetComponent<HUDNameHealth>().CurrentKillCount;

                List<string> namesList = new();
                List<uint> killCount = new();
                for (int i = 0; i < playerListArray.Length; i++)
                {
                    for (int j = 0; j < m_sortPlayerList.Length; j++)
                        if (playerListArray[i].text == m_sortPlayerList[j].NickName)
                        {
                            namesList.Add(playerListArray[i].text);
                            killCount.Add(currentKillCount[i]);
                        }
                }

                for (int i = 0; i < m_sortPlayerList.Length; i++)
                {
                    m_highestKills.Add(new Kills(namesList[i], (int)killCount[i]));
                }

                AddUpTeamDamage(m_highestKills.Count);

                m_killAmounts[0].text = $"{m_TeamOneKills} ";
                m_killAmounts[1].text = $"{m_TeamTwoKills} ";
                break;
            }
            case false:
            {
                for (int i = 0; i < m_killCountChildren.Length; i++)
                {
                    m_killCountChildren[i].SetActive(true);
                }

                break;
            }
        }
    }

    private void AddUpTeamDamage(int _count)
    {
        switch (_count)
        {
            case 6:
            {
                m_TeamOneKills = m_highestKills[0].m_kills + m_highestKills[1].m_kills + m_highestKills[2].m_kills;
                m_TeamTwoKills = m_highestKills[3].m_kills + m_highestKills[4].m_kills + m_highestKills[5].m_kills;
                break;
            }
            case 4:
            {
                m_TeamOneKills = m_highestKills[0].m_kills + m_highestKills[1].m_kills;
                m_TeamTwoKills = m_highestKills[2].m_kills + m_highestKills[3].m_kills;
                break;
            }
            case 2:
            {
                //No Team for test cases... .
                m_TeamOneKills = m_highestKills[0].m_kills;
                m_TeamTwoKills = m_highestKills[1].m_kills;
                break;
            }
            default:
                break;
        }
    }

    private void MatchRunsNow(int _eMatchMode)
    {
        switch (_eMatchMode)
        {
            case (int)EMatchMode.TeamBattle:
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
            case (int)EMatchMode.TeamBattle:
            {
                bool winnerTeam = m_TeamOneKills > m_TeamTwoKills;
                switch (winnerTeam)
                {
                    case false:
                    {
                        m_winnerText.text = "Congratulations! TeamTwo won!";
                        break;
                    }
                    case true:
                    {
                        m_winnerText.text = "Congratulations! TeamOne won!";
                        break;
                    }
                }

                m_winnerBackground.gameObject.SetActive(true);
                m_escapeMessage.gameObject.SetActive(true);
                m_matchRuns = false;
                ToggleKillListVisibility(true);
                break;
            }
            default:
                break;
        }
    }
}