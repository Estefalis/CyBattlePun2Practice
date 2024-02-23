using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Rigidbody m_rigidbody;
    [SerializeField] private Animator m_animator;

    [SerializeField] private float m_moveSpeed = 5.0f;
    [SerializeField] private float m_rotationSpeed = 300.0f;
    [SerializeField] private float m_jumpForce = 600.0f;

    [SerializeField] private Transform m_groundCheck;
    [SerializeField] float m_groundDistance = 0.2f;
    [SerializeField] private LayerMask m_groundLayer;
    private bool m_isGrounded;
    private bool m_isDead = false;
    private bool m_matchRuns = false;
    private bool m_respawnIsBlocked = false;

    #region RespawnMembers
    [SerializeField] private float m_respawnTime = 3.0f;
    private Vector3 m_startPosition;
    //private bool m_isAlreadyRespawning = false;
    //private float m_respawnTimeStart, m_respawnTimeEnd;
    private GameObject m_respawnPanel;
    #endregion

    private AvatarColor m_avatarColor;
    private PhotonView m_photonView;

    private void Awake()
    {
        CustomTimer.KillCountMatchRuns += MatchRunsNow;
        CustomTimer.KillCountMatchEnds += MatchEndsNow;
        CustomTimer.TeamBattleMatchRuns += MatchRunsNow;
        CustomTimer.TeamBattleMatchEnds += MatchEndsNow;
        CustomTimer.NoRespawnMatchRuns += MatchRunsNow;
        CustomTimer.NoRespawnMatchEnds += MatchEndsNow;
        AvatarColor.IsDead += WhenCharacterDies;
        AvatarColor.IsAliveAgain += WhenCharacterRespawns;
        //AvatarColor.HPZeroInNoRespawnMatch += MatchEndsNow;   HPZeroInNoRespawnMatch.Invoke version from AvatarColor.cs script.

        m_photonView = GetComponent<PhotonView>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_animator = GetComponent<Animator>();
        m_avatarColor = GetComponent<AvatarColor>();

        m_startPosition = transform.position;
        m_respawnPanel = GameObject.Find("RespawnPanel");
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
        //AvatarColor.HPZeroInNoRespawnMatch -= MatchEndsNow;   HPZeroInNoRespawnMatch.Invoke version from AvatarColor.cs script.
    }

    private void Update()
    {
        if (!m_isDead && m_matchRuns)
        {
            if (m_photonView.IsMine)
            {
                SetAnimatorAxisValues("BlendHori", Input.GetAxis("Horizontal"));
                SetAnimatorAxisValues("BlendVert", Input.GetAxis("Vertical"));

                Jump();
            }
        }
    }

    private void SetAnimatorAxisValues(string _axis, float _axisValue)
    {
        m_animator.SetFloat(_axis, _axisValue);
    }

    private void FixedUpdate()
    {
        if (!m_isDead && m_matchRuns)
        {
            m_respawnPanel.SetActive(false);

            if (m_photonView.IsMine)
            {
                RotatePlayer();
                MovePlayer();
            }
        }
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

        //'Update()' sets the values, while the game is running.
    }

    private void MatchEndsNow(int _eMatchMode)
    {
        switch (_eMatchMode)
        {
            case (int)EMatchMode.NoRespawn:
            {
                #region HPZeroInNoRespawnMatch.Invoke version from AvatarColor.cs script.
                //HAS TO COME FROM HERE, NOT FROM 'AvatarColor.cs'. OR ALL JUST EXIT TOGETHER!
                //GetComponent<AvatarColor>().NoRespawnExit();
                #endregion
                break;
            }
            default:
            {
                m_matchRuns = false;
                SetAnimatorAxisValues("BlendHori", 0.0f);
                SetAnimatorAxisValues("BlendVert", 0.0f);
                break;
            }
        }
    }

    private void WhenCharacterDies(bool _charIsDead, int _viewID)
    {
        if (m_photonView.ViewID == _viewID && m_matchRuns && !m_respawnIsBlocked)  //Codeline ot stop the respawn process, once the match ends.
        {
            m_isDead = _charIsDead;
            m_respawnPanel.SetActive(true);
            m_respawnPanel.GetComponent<RespawnTimeText>().enabled = true;
            StartCoroutine(RespawnProcess(_viewID));
        }
    }

    private void WhenCharacterRespawns(bool _charIsDead, int _viewID)
    {
        if (m_photonView.ViewID == _viewID)
        {
            m_isDead = _charIsDead;

            //m_isAlreadyRespawning = false;
            //m_respawnTimeEnd = Time.time;
            StopCoroutine(RespawnProcess(_viewID));
        }
    }

    private void RotatePlayer()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

        if (movement != Vector3.zero)
        {
            Vector3 rotateY = new Vector3(0, Input.GetAxis("Mouse X") * m_rotationSpeed * Time.fixedDeltaTime, 0);
            m_rigidbody.MoveRotation(m_rigidbody.rotation * Quaternion.Euler(rotateY));
        }
    }

    private void MovePlayer()
    {
        m_rigidbody.MovePosition(m_rigidbody.position + transform.forward * Input.GetAxis("Vertical") * m_moveSpeed * Time.fixedDeltaTime +
            transform.right * Input.GetAxis("Horizontal") * m_moveSpeed * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        m_isGrounded = Physics.CheckSphere(m_groundCheck.position, m_groundDistance, m_groundLayer);
        if (Input.GetKeyDown(KeyCode.Space) && m_isGrounded)
        {
            m_rigidbody.AddForce(m_jumpForce * Time.deltaTime * Vector3.up, ForceMode.VelocityChange);
        }
    }

    private IEnumerator RespawnProcess(int _viewID)
    {
        //m_respawnTimeStart = Time.time;
        yield return new WaitForSeconds(m_respawnTime);
        transform.position = m_startPosition;
        GetComponent<AvatarColor>().RespawnDeadPlayer(_viewID);
    }
}