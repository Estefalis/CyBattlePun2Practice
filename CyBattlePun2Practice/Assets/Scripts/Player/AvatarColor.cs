using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script attached inside the YBot Prefab.
/// </summary>
public class AvatarColor : MonoBehaviour
{
    [SerializeField] private int[] m_buttonIDs;
    public int[] ViewID { get => m_viewID; }
    [SerializeField] private int[] m_viewID;
    [SerializeField] private Color32[] m_colors;
    [SerializeField] private Color32[] m_teamColors;
    [SerializeField] private float m_exitDelay = 3.0f;

    [SerializeField] private AudioClip[] m_weaponSounds;
    private GameObject m_namesBackground;
    private GameObject m_waitForPlayers;
    private GameObject m_spawnerManager;
    private GameObject m_chooseColorPanel;
    private bool m_setupPreparationIsFinished = false;
    private bool m_matchRuns = false;

    private EMatchMode m_eMatchMode;
    public static event Action<bool, int> IsDead;
    public static event Action<bool, int> IsAliveAgain;
    //public static event Action<int> NoRespawnMatchRuns;
    public static event Action LastPlayerLeftOnNoRespawn;
    //public static event Action<int> HPZeroInNoRespawnMatch;   HPZeroInNoRespawnMatch.Invoke version from AvatarColor.cs script.

    private PhotonView m_photonView;

    //private string m_lastPlayerLeft;
    private string m_compareDeadNameInternal;
    //private string m_compareDeadNameExternal;  HPZeroInNoRespawnMatch.Invoke version from AvatarColor.cs script.

    private void Awake()
    {
        m_photonView = GetComponent<PhotonView>();
        m_compareDeadNameInternal = PhotonNetwork.LocalPlayer.NickName;
        CustomTimer.NoRespawnMatchRuns += MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds += MatchEndsNow;
    }

    private void Start()
    {
        m_namesBackground = GameObject.Find("NamesBackground");
        m_waitForPlayers = GameObject.Find("WaitingBackground");
        m_spawnerManager = GameObject.Find("SpawnerManager");
        m_chooseColorPanel = GameObject.Find("ChooseColorPanel");
        m_eMatchMode = m_namesBackground.GetComponent<HUDNameHealth>().MatchMode;
    }

    private void OnDisable()
    {
        CustomTimer.NoRespawnMatchRuns -= MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds -= MatchEndsNow;
    }

    private void Update()
    {
        if (m_photonView.IsMine && !m_waitForPlayers.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //foreach (var viewID in m_viewID)
                //    if (m_photonView.ViewID == viewID)
                //    {
                //    }
                RemoveOwnData(/*viewID*/);
                ExitTheArena(/*viewID*/);
            }
        }

        if (m_matchRuns && m_eMatchMode == EMatchMode.NoRespawn)
            NoRespawnLastPlayerCheck();
    }

    private void NoRespawnLastPlayerCheck()
    {
        switch (PhotonNetwork.CurrentRoom.PlayerCount)      //Takes Action, if only one Player in Room is left, if match runs.
        {
            case 1:
            {
                LastPlayerLeftOnNoRespawn.Invoke();
                break;
            }
            default:
            {
                break;
            }
        }
    }

    private void MatchRunsNow(int _eMatchMode)
    {
        switch (_eMatchMode)
        {
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

    #region Ensure that 'No Respawn' works
    private void BlockTargetMovements(string _shooterName, string _targetName, int i)
    {
        this.gameObject.layer = (int)EYBotLayers.Ignore_Raycast;

        #region Guide code, replace by the 'IsDead' Action.
        //gameObject.GetComponent<PlayerMovement>().isDead = true;
        //gameObject.GetComponent<WeaponChange>().isDead = true;
        //gameObject.GetComponentInChildren<OurAimLookAt>().isDead = true;
        #endregion
        m_namesBackground.GetComponent<HUDNameHealth>().HealthBars[i].gameObject.GetComponent<Image>().fillAmount = 0;
        GetComponent<Animator>().SetBool("Dead", true);
        GetComponent<Animator>().SetBool("Hit", false);

        if (m_matchRuns)
        {
            switch (m_eMatchMode)
            {
                case EMatchMode.NoRespawn:
                {
                    //m_lastPlayerLeft = _shooterName;
                    NoRespawnExitInternal(_targetName);         //Kicks the currently died Player within the match (_targetName).
                    #region HPZeroInNoRespawnMatch.Invoke version from AvatarColor.cs script.
                    //m_compareDeadNameExternal = _targetName;
                    //HPZeroInNoRespawnMatch.Invoke((int)m_eMatchMode);
                    #endregion
                    break;
                }
                default:
                {
                    //Outer switch for actions out of 'NoRespawn Match' mode.
                    IsDead.Invoke(true, m_photonView.ViewID);
                    m_namesBackground.GetComponent<HUDNameHealth>().DisplayKillMessage(_shooterName, _targetName);
                    break;
                }
            }
        }
    }

    private void NoRespawnExitInternal(string _targetName)
    {
        if (m_compareDeadNameInternal == _targetName)
        {
            m_namesBackground.GetComponent<HUDNameHealth>().NoRespawnBackground.gameObject.SetActive(true);
            StartCoroutine(WaitToExit());
        }
    }

    #region HPZeroInNoRespawnMatch.Invoke version from AvatarColor.cs script.
    //public void NoRespawnExit()
    //{
    //    if (PhotonNetwork.LocalPlayer.NickName == m_compareDeadNameExternal)
    //    {
    //        m_namesBackground.GetComponent<HUDNameHealth>().NoRespawnBackground.gameObject.SetActive(true);
    //        StartCoroutine(WaitToExit());
    //    }
    //}
    #endregion
    #endregion

    private void ExitTheArena(/*int _viewID*/)
    {
        //if (m_photonView.ViewID == _viewID)
        StartCoroutine(LeaveTheArena());
    }

    #region PunRPCs
    private void RemoveOwnData(/*int _viewID*/)
    {
        //if (m_photonView.ViewID == _viewID)
        m_photonView.RPC("RPC_RemoveMe", RpcTarget.AllBuffered);
    }

    public void DamageOpponents(string _shooterName, string _targetName, float _damage)
    {
        m_photonView.RPC("RPC_DamageOpponents", RpcTarget.AllBuffered, _shooterName, _targetName, _damage);
    }

    public void PlayWeaponShot(string _playerName, int _weaponNumber)
    {
        if (m_photonView.IsMine && !m_chooseColorPanel.activeInHierarchy)
            m_photonView.RPC("PlaySpecificWeaponSound", RpcTarget.All, _playerName, _weaponNumber);
    }

    public void SetAvatarColor()
    {
        GetComponent<PhotonView>().RPC("AssignAvatarColor", RpcTarget.AllBuffered);
    }

    public void RespawnDeadPlayer(int _viewID)
    {
        m_photonView.RPC("RPC_FinishedPlayerRespawn", RpcTarget.AllBuffered, _viewID);
    }

    [PunRPC]
    private void RPC_DamageOpponents(string _shooterName, string _targetName, float _damage)
    {
        for (int i = 0; i < m_namesBackground.GetComponent<HUDNameHealth>().Names.Length; i++)
        {
            if (_targetName == m_namesBackground.GetComponent<HUDNameHealth>().Names[i].text)
            {
                if (m_namesBackground.GetComponent<HUDNameHealth>().HealthBars[i].gameObject.GetComponent<Image>().fillAmount > 0)
                {
                    GetComponent<Animator>().SetBool("Hit", true);
                    float restLife = m_namesBackground.GetComponent<HUDNameHealth>().HealthBars[i].gameObject.GetComponent<Image>().fillAmount -= _damage;

                    if (restLife <= 0)
                    {
                        BlockTargetMovements(_shooterName, _targetName, i);
                    }
                    else
                        StartCoroutine(Recover());
                }
                else
                {
                    BlockTargetMovements(_shooterName, _targetName, i);
                }
            }
        }
    }

    [PunRPC]
    private void PlaySpecificWeaponSound(string _playerName, int _weaponNumber)
    {
        for (int i = 0; i < m_namesBackground.GetComponent<HUDNameHealth>().Names.Length; i++)
        {
            if (_playerName == m_namesBackground.GetComponent<HUDNameHealth>().Names[i].text)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                audioSource.clip = m_weaponSounds[_weaponNumber];
                audioSource.Play();
            }
        }
    }

    [PunRPC]
    private void AssignAvatarColor()
    {
        for (int i = 0; i < m_viewID.Length; i++)
        {
            switch (m_eMatchMode)
            {
                case EMatchMode.KillCount:
                case EMatchMode.NoRespawn:
                {
                    if (GetComponent<PhotonView>().ViewID == m_viewID[i])
                    {
                        this.transform.GetChild(1).GetComponent<Renderer>().material.color = m_colors[i];   //Gets YBot's Surface child and sets our color.
                        m_namesBackground.GetComponent<HUDNameHealth>().PlayerDetailParents[i].gameObject.SetActive(true);
                        #region Guide code replace by just setting the visibility of their parent above.
                        //m_namesBackground.GetComponent<HUDNameHealth>().Names[i].gameObject.SetActive(true);
                        //m_namesBackground.GetComponent<HUDNameHealth>().HealthBar[i].gameObject.SetActive(true);
                        #endregion
                        m_namesBackground.GetComponent<HUDNameHealth>().Names[i].text = GetComponent<PhotonView>().Owner.NickName;
                        m_setupPreparationIsFinished = true;
                    }
                    break;
                }
                case EMatchMode.TeamBattle:
                {
                    if (GetComponent<PhotonView>().ViewID == m_viewID[i])
                    {
                        this.transform.GetChild(1).GetComponent<Renderer>().material.color = m_teamColors[i];   //Gets YBot's Surface child and sets our color.
                        m_namesBackground.GetComponent<HUDNameHealth>().PlayerDetailParents[i].gameObject.SetActive(true);
                        #region Guide code replace by just setting the visibility of their parent above.
                        //m_namesBackground.GetComponent<HUDNameHealth>().Names[i].gameObject.SetActive(true);
                        //m_namesBackground.GetComponent<HUDNameHealth>().HealthBar[i].gameObject.SetActive(true);
                        #endregion
                        m_namesBackground.GetComponent<HUDNameHealth>().Names[i].text = GetComponent<PhotonView>().Owner.NickName;
                        m_setupPreparationIsFinished = true;
                    }
                    break;
                }
                default:
                    break;
            }
        }
    }

    [PunRPC]
    private void RPC_RemoveMe()
    {
        for (int i = 0; i < m_namesBackground.GetComponent<HUDNameHealth>().Names.Length; i++)
        {
            if (m_photonView.Owner.NickName == m_namesBackground.GetComponent<HUDNameHealth>().Names[i].text)    //Will be me!
            {
                m_namesBackground.GetComponent<HUDNameHealth>().PlayerDetailParents[i].gameObject.SetActive(false);
                #region Guide code replace by just setting the visibility of their parent above.
                //m_namesBackground.GetComponent<HUDNameHealth>().Names[i].gameObject.SetActive(false);
                //m_namesBackground.GetComponent<HUDNameHealth>().HealthBar[i].gameObject.SetActive(false);
                #endregion
                m_setupPreparationIsFinished = false;
            }
        }
    }

    [PunRPC]
    private void RPC_FinishedPlayerRespawn(int _viewID)
    {
        if (_viewID == m_photonView.ViewID)
        {
            IsAliveAgain.Invoke(false, m_photonView.ViewID);
            GetComponent<Animator>().SetBool("Dead", false);
            this.gameObject.layer = (int)EYBotLayers.Default;
        }

        for (int i = 0; i < m_namesBackground.GetComponent<HUDNameHealth>().Names.Length; i++)
        {
            if (m_photonView.Owner.NickName == m_namesBackground.GetComponent<HUDNameHealth>().Names[i].text)
            {
                m_namesBackground.GetComponent<HUDNameHealth>().HealthBars[i].gameObject.GetComponent<Image>().fillAmount = 1;
            }
        }
    }
    #endregion

    #region IEnumerators
    private IEnumerator LeaveTheArena()
    {
        yield return new WaitWhile(SetupPreparationIsFinished);
        m_spawnerManager.GetComponent<SpawnerManager>().LeaveToLobby();

        PhotonNetwork.LeaveRoom();  //Character disappears NOW and no codeline after THIS will be executed!!!        
    }

    private bool SetupPreparationIsFinished()
    {
        return m_setupPreparationIsFinished;
    }

    private IEnumerator Recover()
    {
        float animationlength = GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length * 0.1f;
        //yield return new WaitForSeconds(GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length);
        yield return new WaitForSeconds(animationlength);
        GetComponent<Animator>().SetBool("Hit", false);
        //StopCoroutine(Recover());
    }

    private IEnumerator WaitToExit()
    {
        yield return new WaitForSeconds(m_exitDelay);
        #region Remove Data Options
        //foreach (var viewID in m_viewID)
        //    if (m_photonView.ViewID == viewID)
        //    {
        //    }
        RemoveOwnData(/*viewID*/);
        ExitTheArena(/*viewID*/);
        #endregion
    }
    #endregion
}