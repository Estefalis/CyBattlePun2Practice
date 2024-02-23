using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public enum EMatchMode
{
    KillCount,
    TeamBattle,
    NoRespawn
}

public class HUDNameHealth : MonoBehaviour
{
    public Transform[] PlayerDetailParents { get => m_playerDetailParents; }
    [SerializeField] private Transform[] m_playerDetailParents;
    public TMP_Text[] Names { get => m_names; }
    [SerializeField] private TMP_Text[] m_names;
    public Image[] HealthBars { get => m_healthBars; }
    [SerializeField] private Image[] m_healthBars;
    public Transform KillMessageBackground { get => m_killMessageBackground; }
    [SerializeField] private Transform m_killMessageBackground;
    public Transform NoRespawnBackground { get => m_noRespawnBackground; }
    [SerializeField] private Transform m_noRespawnBackground;
    [SerializeField ]private TMP_Text m_norespawnText;
    public TMP_Text KillMessageTest { get => m_killMessageText; }
    [SerializeField] private TMP_Text m_killMessageText;

    public uint[] CurrentKillCount { get => m_currentKillCount; }
    [SerializeField] private uint[] m_currentKillCount;
    //public bool TeamMode { get => m_teamMode; }
    //[SerializeField] private bool m_teamMode = false;

    public EMatchMode MatchMode { get => m_eMatchMode; }
    [SerializeField] private EMatchMode m_eMatchMode;

    private float m_switchOffCountDown = 3.0f;

    private PhotonView m_photonView;

    private void Awake()
    {
        m_photonView = GetComponent<PhotonView>();

        if (m_noRespawnBackground != null || m_eMatchMode == EMatchMode.NoRespawn)
        {
            m_noRespawnBackground.gameObject.SetActive(false);
        }

        AvatarColor.LastPlayerLeftOnNoRespawn += GratulateTheWinner;
        m_killMessageBackground.gameObject.SetActive(false);

        for (int i = 0; i < m_names.Length; i++)
        {
            m_playerDetailParents[i].gameObject.SetActive(false);
            #region Guide code replace by just setting the visibility of their parent above.
            //m_names[i].gameObject.SetActive(false);
            //m_healthBar[i].gameObject.SetActive(false);
            #endregion
        }
    }

    private void OnDisable()
    {        
        AvatarColor.LastPlayerLeftOnNoRespawn -= GratulateTheWinner;
    }

    private void Start()
    {
        for (uint i = 0; i < m_currentKillCount.Length; i++)
        {
            m_currentKillCount[i] = 0;
        }
    }

    private void GratulateTheWinner()
    {
        Player[] lastOneInArena = PhotonNetwork.PlayerList;
        m_noRespawnBackground.gameObject.gameObject.SetActive(true);
        m_norespawnText.text = $"Congratulations to {lastOneInArena[0].NickName} for the win!";
    }

    public void DisplayKillMessage(string _shooter, string _target)
    {
        m_photonView.RPC("RPC_DisplayKillMessage", RpcTarget.All, _shooter, _target);
        UpdateKillCount(_shooter);
    }

    private void UpdateKillCount(string _shooter)
    {
        for (int i = 0; i < m_names.Length; i++)
        {
            if (_shooter == m_names[i].text)
            {
                m_currentKillCount[i]++;
            }
        }
    }

    private IEnumerator SwitchOffKillMessage()
    {
        yield return new WaitForSeconds(m_switchOffCountDown);
        m_photonView.RPC("RPC_SwitchOffKillMessage", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_DisplayKillMessage(string _shooter, string _target)
    {
        m_killMessageBackground.gameObject.SetActive(true);
        m_killMessageText.text = $"{_shooter} killed {_target}";
        StartCoroutine(SwitchOffKillMessage());
    }

    [PunRPC]
    private void RPC_SwitchOffKillMessage()
    {
        m_killMessageBackground.gameObject.SetActive(false);
    }
}