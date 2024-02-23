using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum EScenes
{
    MainMenu,
    Lobby,
    KillCountArena,
    TeamBattleArena,
    NoRespawnArena
}

public class StartMenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_InputField m_nameInputField;
    [SerializeField] private TMP_Text m_connectingText;
    private string m_defaultText = string.Empty; //or "";

    private void Awake()
    {
        m_connectingText.gameObject.SetActive(false);
    }

    #region Custom Methods
    public void UpdateIFName()
    {
        m_defaultText = m_nameInputField.text;
        PhotonNetwork.LocalPlayer.NickName = m_defaultText;
    }

    public void EnterButton()
    {
        if (string.IsNullOrWhiteSpace(m_nameInputField.text))
        {
#if UNITY_EDITOR
            Debug.Log("StartMenu: Please enter a Playername before starting a game.");
#endif
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AutomaticallySyncScene = true;    //All clients get loaded into the same Scene as the MasterClient!
            PhotonNetwork.ConnectUsingSettings();   //Connect to Photon Servers.
            m_connectingText.gameObject.SetActive(true);
        }
    }

    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
    #endregion

    #region Photon Callback Methods
    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene(EScenes.Lobby.ToString());
    }
    #endregion
}