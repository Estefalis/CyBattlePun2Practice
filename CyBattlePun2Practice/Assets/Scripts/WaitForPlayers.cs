using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class WaitForPlayers : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text m_modeExplanationText;
    [SerializeField] private TMP_Text m_currentPlayersJoinedText;
    [SerializeField] private TMP_Text m_waitingForText;
    [SerializeField] private GameObject m_enterButton;
    [SerializeField] private GameObject m_returnButton;

    [SerializeField, Range(2, 6)] private int m_maxPlayers;
    [SerializeField] private CarryExplanationSO m_explanationSO;

    private IEnumerator m_delayedLobbyLoading;

    private void Awake()
    {
        m_delayedLobbyLoading = DelayedLobbyLoading();
        m_enterButton.SetActive(false);
        m_modeExplanationText.text = $"Battle Target:\n {m_explanationSO.ModeExplanation}";
    }

    public override void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PhotonNetwork.InRoom)
            {
                if (m_maxPlayers < PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    if (PhotonNetwork.CurrentRoom.PlayerCount <= m_maxPlayers)
                        SwitchMaxPlayerSource(PhotonNetwork.CurrentRoom.PlayerCount, m_maxPlayers);
                }
                else
                {
                    if (PhotonNetwork.CurrentRoom.PlayerCount <= PhotonNetwork.CurrentRoom.MaxPlayers)
                        SwitchMaxPlayerSource(PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.CurrentRoom.MaxPlayers);
                }

                if (PhotonNetwork.CurrentRoom.PlayerCount == m_maxPlayers || PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    m_waitingForText.text = "Ready to";
                    m_enterButton.SetActive(true);
                }
            }
        }
    }

    private void SwitchMaxPlayerSource(int _currentPlayerAmount, int _maxPlayerAmount)
    {
        m_currentPlayersJoinedText.text = $"Players joined: {_currentPlayerAmount}/{_maxPlayerAmount}";
    }

    public void GoToColorSelection()
    {
        gameObject.SetActive(false);
    }

    public void LeaveToLobby()
    {
        StartCoroutine(m_delayedLobbyLoading);
    }

    private IEnumerator DelayedLobbyLoading()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        yield break;
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel((int)EScenes.Lobby);
    }
}