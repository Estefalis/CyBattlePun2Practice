using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text m_connecting;

    private readonly TypedLobby m_killCount = new TypedLobby("KillCount", LobbyType.Default);
    private readonly TypedLobby m_teamBattle = new TypedLobby("TeamBattle", LobbyType.Default);
    private readonly TypedLobby m_noRespawn = new TypedLobby("NoRespawn", LobbyType.Default);

    private string m_levelName = "";
    private int m_levelIndex;

    [SerializeField] private TMP_Text[] m_ModeTexts;
    [SerializeField] private CarryExplanationSO m_explanation;

    private void Awake()
    {
        if (m_connecting.gameObject.activeInHierarchy)
            m_connecting.gameObject.SetActive(false);

        if (!Cursor.visible)
            Cursor.visible = true;
    }

    public void BackToMainMenu()
    {
        PhotonNetwork.Disconnect();
        SceneManager.LoadScene(EScenes.MainMenu.ToString());
    }

    public void JoinGameKillCount()
    {
        m_levelName = EScenes.KillCountArena.ToString();
        //m_levelIndex = (int)EScenes.KillCountArena;
        m_explanation.ModeExplanation = m_ModeTexts[0].text;
        PhotonNetwork.JoinLobby(m_killCount);
    }

    public void JoinGameTeamBattle()
    {
        m_levelName = EScenes.TeamBattleArena.ToString();
        //m_levelIndex = (int)EScenes.KillCountArena;
        m_explanation.ModeExplanation = m_ModeTexts[1].text;
        PhotonNetwork.JoinLobby(m_teamBattle);
    }

    public void JoinGameNoRespawn()
    {
        m_levelName = EScenes.NoRespawnArena.ToString();
        //m_levelIndex = (int)EScenes.NoRespawnArena;
        m_explanation.ModeExplanation = m_ModeTexts[2].text;
        PhotonNetwork.JoinLobby(m_noRespawn);
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 6;
        PhotonNetwork.CreateRoom("Arena" + Random.Range(1, 1000), roomOptions);
    }

    public override void OnJoinedRoom()
    {
        m_connecting.gameObject.SetActive(true);
        PhotonNetwork.LoadLevel(m_levelName);
    }
}