using Cinemachine;
using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

/// <summary>
/// Enum of Layers just for the YBot. Needs to be identical to the Layers of the YBot at all times!
/// </summary>
public enum EYBotLayers { Default, TransparentFX, Ignore_Raycast, Player, Water, UI, Walls, Ground, Weapon }
public enum EWeapon { WeaponOne, WeaponTwo, WeaponThree }

public class WeaponChange : MonoBehaviour
{
    private enum EInputDevice { Keyboard, Mouse, GamePad }

    private enum EMouseButtons { Left, Right, Middle, }

    #region Rigging
    [SerializeField] private TwoBoneIKConstraint m_leftHand;
    [SerializeField] private TwoBoneIKConstraint m_leftFinger;
    [SerializeField] private TwoBoneIKConstraint m_rightHand;

    [SerializeField] private RigBuilder m_rigBuilder;
    [SerializeField] private Transform[] m_leftTargetWeapons;
    [SerializeField] private Transform[] m_leftFingerTargets;
    [SerializeField] private Transform[] m_rightTargetWeapons;
    #endregion

    #region Weapon and Ammo
    [SerializeField] private GameObject[] m_weapons;
    [SerializeField] private Sprite[] m_weaponIcons;
    public int[] WeaponAmmoAmount { get => m_weaponAmmoAmount; set => m_weaponAmmoAmount = value; }
    [SerializeField] private int[] m_weaponAmmoAmount;

    private GameObject m_chooseColorPanel;
    private GameObject m_weaponSpawner;
    private Image m_weaponIcon;
    private TMP_Text m_weaponAmmo;
    private int m_deviceButtonIndex;
    private int m_weaponNumber = 0;     //Changed in runtime by mouse and keyboard.
    #endregion

    [SerializeField] private GameObject[] m_muzzleFlashs;
    [SerializeField] private float m_muzzleOffDelay = 0.03f;

    #region Set Cinemachine PlayerCamera
    private CinemachineVirtualCamera m_playerCamera;
    private GameObject m_cameraGameObject;
    //[SerializeField] private MultiAimConstraint[] m_aimObjects;
    //private Transform m_aimTargets;
    #endregion

    private string m_shooterName;
    private string m_targetName;
    [SerializeField] private float[] m_damageAmounts;
    [SerializeField] private float m_raycastLength = 500.0f;

    private bool m_isDead = false;
    private bool m_matchRuns = false;
    private bool m_respawnIsBlocked = false;

    private PhotonView m_photonView;

    private void Awake()
    {
        m_photonView = GetComponent<PhotonView>();
        CustomTimer.KillCountMatchRuns += MatchRunsNow;
        CustomTimer.KillCountMatchEnds += MatchEndsNow;
        CustomTimer.TeamBattleMatchRuns += MatchRunsNow;
        CustomTimer.TeamBattleMatchEnds += MatchEndsNow;
        CustomTimer.NoRespawnMatchRuns += MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds += MatchEndsNow;
        AvatarColor.IsDead += WhenCharacterDies;
        AvatarColor.IsAliveAgain += WhenCharacterRespawns;
        m_chooseColorPanel = GameObject.Find("ChooseColorPanel");
        m_weaponIcon = GameObject.Find("WeaponIcon").GetComponent<Image>();
        m_weaponAmmo = GameObject.Find("WeaponAmmo").GetComponent<TMP_Text>();
    }

    private void OnDisable()
    {
        CustomTimer.KillCountMatchRuns -= MatchRunsNow;
        CustomTimer.KillCountMatchEnds -= MatchEndsNow;
        CustomTimer.TeamBattleMatchRuns -= MatchRunsNow;
        CustomTimer.TeamBattleMatchEnds -= MatchEndsNow;
        CustomTimer.NoRespawnMatchRuns -= MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds -= MatchEndsNow;
        AvatarColor.IsDead -= WhenCharacterDies;
        AvatarColor.IsAliveAgain -= WhenCharacterRespawns;
    }

    private IEnumerator Start()
    {
        SetAmmoAmounts();

        if (m_photonView.IsMine)
        {
            yield return new WaitUntil(PhotonNetworkIsConnectedAndReady);
            m_cameraGameObject = GameObject.Find("PlayerCamera");
            //m_aimTargets = GameObject.Find("Targeting").transform;

            m_playerCamera = m_cameraGameObject.GetComponent<CinemachineVirtualCamera>();
            m_playerCamera.Follow = this.gameObject.transform;
            m_playerCamera.LookAt = this.gameObject.transform;
            //Invoke(nameof(SetCinemachineLookAt), 0.1f); //if Start is void.
            //yield return new WaitForSeconds(0.1f);
            //SetCinemachineLookAt();
        }
        else
        {
            transform.gameObject.GetComponent<PlayerMovement>().enabled = false;
        }

        SpawnWeaponInNetwork();
    }

    private void Update()
    {
        if (!m_isDead && m_matchRuns && !m_chooseColorPanel.activeInHierarchy)
        {
            if (m_photonView.IsMine)
            {
                FireWeapons();
                SwitchWeapons();
            }
        }
    }

    private void SetAmmoAmounts()
    {
        m_weaponAmmoAmount[0] = 60;
        m_weaponAmmoAmount[1] = 0;
        m_weaponAmmoAmount[2] = 0;
        m_weaponAmmo.text = $"{m_weaponAmmoAmount[0]}";
    }

    private void MatchRunsNow(int _eMatchMode)
    {
        switch (_eMatchMode)
        {
            case (int)EMatchMode.NoRespawn:
            {
                m_respawnIsBlocked = true;
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
                gameObject.layer = (int)EYBotLayers.Ignore_Raycast;
                break;
            }
        }
    }

    private void WhenCharacterDies(bool _charIsDead, int _viewID)
    {
        if (m_photonView.ViewID == _viewID && !m_respawnIsBlocked)
            m_isDead = _charIsDead;
    }

    private void WhenCharacterRespawns(bool _charIsDead, int _viewID)
    {
        if (m_photonView.ViewID == _viewID)
        {
            m_isDead = _charIsDead;
        }
    }

    #region Guide code replaced by Tutor himself.
    //private void SetCinemachineLookAt()
    //{
    //    if (m_aimTargets != null)
    //    {
    //        for (int i = 0; i < m_aimObjects.Length; i++)
    //        {
    //            //Accessing Source Objects inside the PlayerPrefab, changing and setting it.
    //            var target = m_aimObjects[i].data.sourceObjects;
    //            target.SetTransform(0, m_aimTargets.transform);
    //            m_aimObjects[i].data.sourceObjects = target;
    //        }

    //        m_rigBuilder.Build();
    //    }
    //}
    #endregion

    private void SwitchWeapons()
    {
        if (Input.GetMouseButtonDown(1))
        {
            m_deviceButtonIndex = (int)EMouseButtons.Right;
            m_photonView.RPC("RPC_UpdatePlayerWeapon", RpcTarget.AllBuffered, EInputDevice.Mouse, m_deviceButtonIndex);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                m_deviceButtonIndex = (int)EWeapon.WeaponOne;
                m_photonView.RPC("RPC_UpdatePlayerWeapon", RpcTarget.AllBuffered, EInputDevice.Keyboard, m_deviceButtonIndex);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                m_deviceButtonIndex = (int)EWeapon.WeaponTwo;
                m_photonView.RPC("RPC_UpdatePlayerWeapon", RpcTarget.AllBuffered, EInputDevice.Keyboard, m_deviceButtonIndex);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                m_deviceButtonIndex = (int)EWeapon.WeaponThree;
                m_photonView.RPC("RPC_UpdatePlayerWeapon", RpcTarget.AllBuffered, EInputDevice.Keyboard, m_deviceButtonIndex);
            }
        }
    }

    private void FireWeapons()
    {
        if (Input.GetMouseButtonDown(0) && m_weaponAmmoAmount[m_weaponNumber] > 0)
        {
            m_weaponAmmoAmount[m_weaponNumber]--;
            m_weaponAmmo.text = $"{m_weaponAmmoAmount[m_weaponNumber]}";
            GetComponent<AvatarColor>().PlayWeaponShot(m_photonView.Owner.NickName, m_weaponNumber);
            m_photonView.RPC("MuzzleFlashAnimation", RpcTarget.All);

            WeaponRaycast();
        }
    }

    private void WeaponRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        this.gameObject.layer = (int)EYBotLayers.Ignore_Raycast;

        if (Physics.Raycast(ray, out RaycastHit hitTarget, m_raycastLength))
        {
            if (hitTarget.transform.gameObject.GetComponent<PhotonView>() != null /*&& !m_photonView.IsMine*/)  //Isn't mine is bad while developing. :/
            {
                m_targetName = hitTarget.transform.gameObject.GetComponent<PhotonView>().Owner.NickName;
            }

            if (hitTarget.transform.gameObject.GetComponent<AvatarColor>() != null)
            {
                hitTarget.transform.gameObject.GetComponent<AvatarColor>().DamageOpponents(m_photonView.Owner.NickName, hitTarget.transform.gameObject.GetComponent<PhotonView>().Owner.NickName, m_damageAmounts[m_weaponNumber]);
            }

            m_shooterName = GetComponent<PhotonView>().Owner.NickName;
        }
        this.gameObject.layer = (int)EYBotLayers.Default;
    }

    private bool PhotonNetworkIsConnectedAndReady()
    {
        return PhotonNetwork.IsConnectedAndReady;
    }

    private void SpawnWeaponInNetwork()
    {
        m_weaponSpawner = GameObject.Find("Sci-Fi Gun PickUp(Clone)");
        if (m_weaponSpawner == null)
        {
            if (m_photonView.Owner.IsMasterClient)
            {
                var weaponSpawner = GameObject.Find("SpawnerManager");
                weaponSpawner.GetComponent<SpawnerManager>().SpawnWeaponsAtStart();
            }
        }
    }

    /// <summary>
    /// BuildRig within '{}' prevents a rebuild of the Rig on each Update().
    /// </summary>
    /// <param name="_arrayIndex"></param>
    private void BuildRig(int _arrayIndex)
    {
        foreach (GameObject weapon in m_weapons)
        {
            if (weapon != m_weapons[_arrayIndex])
                weapon.SetActive(false);
        }

        m_weapons[_arrayIndex].SetActive(true);
        m_weaponIcon.GetComponent<Image>().sprite = m_weaponIcons[_arrayIndex];
        m_weaponAmmo.text = $"{m_weaponAmmoAmount[_arrayIndex]}";
        m_leftHand.data.target = m_leftTargetWeapons[_arrayIndex];
        m_leftFinger.data.target = m_leftFingerTargets[_arrayIndex];
        m_rightHand.data.target = m_rightTargetWeapons[_arrayIndex];
        m_rigBuilder.Build();
    }

    public void UpdateAmmoAmount()
    {
        m_weaponAmmo.text = $"{m_weaponAmmoAmount[m_weaponNumber]}";
    }

    [PunRPC]
    private void RPC_UpdatePlayerWeapon(EInputDevice _inputType, int _deviceButtonIndex)
    {
        switch (_inputType)
        {
            case EInputDevice.Mouse:
            {
                if (_deviceButtonIndex == (int)EMouseButtons.Right)
                {
                    m_weaponNumber++;

                    if (m_weaponNumber > m_weapons.Length - 1)
                    {
                        m_weaponIcon.GetComponent<Image>().sprite = m_weaponIcons[0];
                        m_weaponAmmo.text = $"{m_weaponAmmoAmount[0]}";
                        m_weaponNumber = 0;
                    }

                    BuildRig(m_weaponNumber);
                }
                break;
            }
            case EInputDevice.Keyboard:
            {
                switch (_deviceButtonIndex)
                {
                    case 0:
                    {
                        m_weaponNumber = 0;
                        BuildRig(m_weaponNumber);
                        break;
                    }
                    case 1:
                    {
                        m_weaponNumber = 1;
                        BuildRig(m_weaponNumber);
                        break;
                    }
                    case 2:
                    {
                        m_weaponNumber = 2;
                        BuildRig(m_weaponNumber);
                        break;
                    }
                    default:
                        break;
                }
                break;
            }
            case EInputDevice.GamePad:
            {
                break;
            }
        }
    }

    [PunRPC]
    private void MuzzleFlashAnimation()
    {
        m_muzzleFlashs[m_weaponNumber].SetActive(true);
        StartCoroutine(MuzzleFlashOff());
    }

    private IEnumerator MuzzleFlashOff()
    {
        yield return new WaitForSeconds(m_muzzleOffDelay);
        m_photonView.RPC("OthersMuzzleFlashOff", RpcTarget.All);
    }

    [PunRPC]
    private void OthersMuzzleFlashOff()
    {

        m_muzzleFlashs[m_weaponNumber].SetActive(false);
    }
}