using Photon.Pun;
using System.Collections;
using UnityEngine;

public class WeaponPickUps : MonoBehaviour
{
    [SerializeField] private EWeapon m_weaponType;
    [SerializeField] private float m_weaponRespawnTime;
    [SerializeField] private int m_ammoRefillAmount;

    private AudioSource m_audioSource;
    private PhotonView m_photonView;

    private MeshRenderer m_meshRenderer;
    private Collider m_weaponCollider;

    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_photonView = GetComponent<PhotonView>();

        m_meshRenderer = GetComponent<MeshRenderer>();
        m_weaponCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider _other)
    {
        if (_other.CompareTag("Player"))
        {
            if (m_photonView.IsMine)
            {
                m_photonView.RPC("RPC_PlayPickUpAudio", RpcTarget.All);
                m_photonView.RPC("RPC_TurnObjectOff", RpcTarget.All);
            }

            _other.GetComponent<WeaponChange>().WeaponAmmoAmount[(int)m_weaponType] += m_ammoRefillAmount;
            _other.GetComponent<WeaponChange>().UpdateAmmoAmount();
        }
    }

    [PunRPC]
    private void RPC_PlayPickUpAudio()
    {
        m_audioSource.Play();
    }

    [PunRPC]
    private void RPC_TurnObjectOff()
    {
        ShowWeapon(false);

        StartCoroutine(StartRespawnTimer());
    }


    [PunRPC]
    private void RPC_TurnObjectOn()
    {
        ShowWeapon(true);
    }

    private IEnumerator StartRespawnTimer()
    {
        yield return new WaitForSeconds(m_weaponRespawnTime);
        m_photonView.RPC("RPC_TurnObjectOn", RpcTarget.All);
    }

    private void ShowWeapon(bool _visibleStatus)
    {
        switch (m_weaponType)
        {
            case EWeapon.WeaponOne:
            {
                m_meshRenderer.enabled = _visibleStatus;
                m_weaponCollider.enabled = _visibleStatus;
                break;
            }
            case EWeapon.WeaponTwo:
            case EWeapon.WeaponThree:
            {
                transform.GetChild(0).gameObject.SetActive(_visibleStatus);  //first ChildObject.
                m_weaponCollider.enabled = _visibleStatus;
                break;
            }
        }
    }
}