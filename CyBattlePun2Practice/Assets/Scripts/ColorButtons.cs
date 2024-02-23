using Photon.Pun;
using UnityEngine;

public class ColorButtons : MonoBehaviour
{
    private GameObject[] m_players;
    private int m_myID;
    private GameObject m_chooseColorPanel;
    private GameObject m_namesBGParent;

    private void Start()
    {
        Cursor.visible = true;
        m_namesBGParent = GameObject.Find("NamesBackground");
        m_chooseColorPanel = GameObject.Find("ChooseColorPanel");
    }

    public void SelectButton(int _buttonID)
    {
        m_players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < m_players.Length; i++)
        {
            if (m_players[i].GetComponent<PhotonView>().IsMine)
            {
                m_myID = m_players[i].GetComponent<PhotonView>().ViewID;
                break; //Stops after finding me.
            }
        }

        GetComponent<PhotonView>().RPC("SelectedColor", RpcTarget.AllBuffered, _buttonID, m_myID);
        Cursor.visible = false;
        m_chooseColorPanel.SetActive(false);
    }

    [PunRPC]
    private void SelectedColor(int _buttonID, int _myID)
    {
        m_players = GameObject.FindGameObjectsWithTag("Player");

        for (int i = 0; i < m_players.Length; i++)
        {
            m_players[i].GetComponent<AvatarColor>().ViewID[_buttonID] = _myID;
            m_players[i].GetComponent<AvatarColor>().SetAvatarColor();
        }

        m_namesBGParent.GetComponent<CustomTimer>().PrepareTimer(_myID);
        this.transform.gameObject.SetActive(false);
    }
}