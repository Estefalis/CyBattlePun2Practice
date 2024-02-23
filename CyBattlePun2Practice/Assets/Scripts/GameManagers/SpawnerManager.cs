using Photon.Pun;
using System.Collections;
using UnityEngine;

public class SpawnerManager : MonoBehaviourPun
{
    [Header("Characters")]
    [SerializeField] private GameObject m_character;
    [SerializeField] private Transform[] m_spawnPoints;

    [Header("Weapons")]
    [SerializeField] private GameObject[] m_weapons;
    [SerializeField] private Transform[] m_weaponSpawnPoints;

    private IEnumerator m_delayedSpawn;
    private IEnumerator m_delayedLobbyLoading;

    private void Awake()
    {
        m_delayedSpawn = SpawnPlayer();
        m_delayedLobbyLoading = DelayedLobbyLoading();
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            StartCoroutine(m_delayedSpawn);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnPlayer()
    {
        yield return new WaitUntil(PhotonNetworkIsConnectedAndReady);

        PhotonNetwork.Instantiate(m_character.name, m_spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].position, m_spawnPoints[PhotonNetwork.CurrentRoom.PlayerCount - 1].rotation, 0);

        StopCoroutine(m_delayedSpawn);
    }

    private bool PhotonNetworkIsConnectedAndReady()
    {
        return PhotonNetwork.IsConnectedAndReady;
    }

    public void SpawnWeaponsAtStart()
    {
        for (int i = 0; i < m_weapons.Length; i++)
        {
            PhotonNetwork.Instantiate(m_weapons[i].name, m_weaponSpawnPoints[i].position, m_weaponSpawnPoints[i].rotation, 0);
        }
    }

    public void LeaveToLobby()
    {
        StartCoroutine(m_delayedLobbyLoading);
    }

    private IEnumerator DelayedLobbyLoading()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LoadLevel((int)EScenes.Lobby);
        }

        yield break;
    }
}