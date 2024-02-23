using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CustomTimer : MonoBehaviour
{
    private enum ETimerDisplaySettings
    {
        SecondsToMilliseconds,
        Seconds,
        MinutesToMilliseconds,
        MinutesToSeconds,
        HoursToMilliseconds,
        HoursToSeconds,
        HoursToMinutes,
        DaysToMilliseconds,
        DaysToSeconds,
        DaysToMinutes,
        DaysToHours,
    }

    private enum ELeapYear
    {
        No = 365,
        Yes = 366
    }

    [SerializeField] private ETimerDisplaySettings m_timerFormatAndRange = ETimerDisplaySettings.MinutesToSeconds;
    [SerializeField] private TextMeshProUGUI m_timerDisplay;

    [SerializeField] private int m_setDays, m_setHours, m_setMinutes, m_setSeconds, m_setMilliseconds;
    [SerializeField] private bool m_timerShallCountUp = false;

    private float m_matchStartTime, m_matchEndTime;
    private float m_combinedTimeValuesF;
    private double m_combinedTimeValuesD;

    private bool m_showMilliseconds = false;
    private bool m_matchRuns = false;

    public static event Action<int> KillCountMatchRuns;
    public static event Action<int> KillCountMatchEnds;
    public static event Action<int> TeamBattleMatchRuns;
    public static event Action<int> TeamBattleMatchEnds;
    public static event Action<int> NoRespawnMatchRuns;
    public static event Action<int> NoRespawnMatchEnds;
    //private TimeSpan m_timeSpan;

    private List<int> m_ViewIDList = new();

    private int m_today, m_thisYear, /*m_silvester,*/ m_moduloLeapYear;
    private IEnumerator m_checkOnDayChange;
    private EMatchMode m_eMatchMode;

    private PhotonView m_photonView;

    private void Awake()
    {
        m_photonView = GetComponent<PhotonView>();

        m_checkOnDayChange = ICheckOnDayChange();
        KillCountMatchRuns += MatchRunsNow;
        KillCountMatchEnds += MatchEndsNow;
        TeamBattleMatchRuns += MatchRunsNow;
        TeamBattleMatchEnds += MatchEndsNow;
        NoRespawnMatchRuns += MatchRunsNow;
        NoRespawnMatchEnds += MatchEndsNow;
        AvatarColor.LastPlayerLeftOnNoRespawn += EndNoRespawnMatch;        
        m_eMatchMode = gameObject.GetComponent<HUDNameHealth>().MatchMode;

        //Converts set time values into seconds.
        m_combinedTimeValuesF = (m_setHours * 3600) + (m_setMinutes * 60) + m_setSeconds + (m_setMilliseconds / 1000);
        m_combinedTimeValuesD = (m_setDays * 86400) + (m_setHours * 3600) + (m_setMinutes * 60) + m_setSeconds + (m_setMilliseconds / 1000);
    }

    private void Start()
    {
        UpdateCheckForLeapYear();
    }

    private void OnDisable()
    {
        m_matchRuns = false;
        KillCountMatchRuns -= MatchRunsNow;
        KillCountMatchEnds -= MatchEndsNow;
        TeamBattleMatchRuns -= MatchRunsNow;
        TeamBattleMatchEnds -= MatchEndsNow;
        NoRespawnMatchRuns -= MatchRunsNow;
        NoRespawnMatchEnds -= MatchEndsNow;
        AvatarColor.LastPlayerLeftOnNoRespawn -= EndNoRespawnMatch;

        StopCoroutine(m_checkOnDayChange);
    }

    private void Update()
    {
        if (m_matchRuns)
        {
            SwitchTimer();
        }
    }

    /// <summary>
    /// Add playerIDs to a viewIdList, to delay the timer start counting, until IdAmount == PhotonNetwork.CurrentRoom.PlayerCount.
    /// </summary>
    /// <param name="_viewID"></param>
    public void PrepareTimer(int _viewID)
    {
        m_ViewIDList.Add(_viewID);
        m_photonView.RPC("Count", RpcTarget.AllBuffered);   //GetComponent<PhotonView>().RPC("Count", RpcTarget.AllBuffered); (In Tutorial.)
    }

    [PunRPC]
    private void Count()
    {
        if (m_ViewIDList.Count == PhotonNetwork.CurrentRoom.PlayerCount)
            StartCounting();
    }

    private void StartCounting()
    {
        m_matchStartTime = Time.time;
        InvokeMatchModeStarts(m_eMatchMode);
    }

    private void InvokeMatchModeStarts(EMatchMode _eMatchMode)
    {
        switch (_eMatchMode)
        {
            case EMatchMode.KillCount:
            {
                KillCountMatchRuns.Invoke((int)_eMatchMode);
                break;
            }
            case EMatchMode.TeamBattle:
            {
                TeamBattleMatchRuns.Invoke((int)_eMatchMode);
                break;
            }
            case EMatchMode.NoRespawn:
            {
                NoRespawnMatchRuns.Invoke((int)_eMatchMode);
                break;
            }
            default:
                break;
        }
    }

    private void MatchRunsNow(int _eMatchModeIndex)
    {
        m_matchRuns = true;

        if (m_checkOnDayChange != null)
        {
            StopCoroutine(m_checkOnDayChange);
        }
        StartCoroutine(m_checkOnDayChange);

    }

    private void MatchEndsNow(int _eMatchModeIndex)
    {
        m_matchEndTime = Time.time; //or m_timeSpan = TimeSpan.FromSeconds(Time.time - m_matchStartTime); ?
        m_matchRuns = false;
    }

    private void SwitchTimer()
    {
        switch (m_timerShallCountUp)
        {
            case false: //Counts down == !m_timerShallCountUp.
            {
                switch (m_timerFormatAndRange) //TODO: Change to m_timerFormatAndRange-switch!
                {
                    case ETimerDisplaySettings.SecondsToMilliseconds:
                    case ETimerDisplaySettings.Seconds:
                    case ETimerDisplaySettings.MinutesToMilliseconds:
                    case ETimerDisplaySettings.MinutesToSeconds:
                    case ETimerDisplaySettings.HoursToMilliseconds:
                    case ETimerDisplaySettings.HoursToSeconds:
                    case ETimerDisplaySettings.HoursToMinutes:
                    {
                        DisplayTimeMathf(m_combinedTimeValuesF - (Time.time - m_matchStartTime));
                        break;
                    }
                    case ETimerDisplaySettings.DaysToMilliseconds:
                    case ETimerDisplaySettings.DaysToSeconds:
                    case ETimerDisplaySettings.DaysToMinutes:
                    case ETimerDisplaySettings.DaysToHours:
                    {
                        DisplayTimeMath(m_combinedTimeValuesD - (Time.time - m_matchStartTime));
                        break;
                    }
                    default:
                        break;
                }

                break;
            }
            case true:  //Counts up == m_timerShallCountUp.
            {
                switch (m_timerFormatAndRange)
                {
                    case ETimerDisplaySettings.SecondsToMilliseconds:
                    case ETimerDisplaySettings.Seconds:
                    case ETimerDisplaySettings.MinutesToMilliseconds:
                    case ETimerDisplaySettings.MinutesToSeconds:
                    case ETimerDisplaySettings.HoursToMilliseconds:
                    case ETimerDisplaySettings.HoursToSeconds:
                    case ETimerDisplaySettings.HoursToMinutes:
                    {
                        DisplayTimeMathf(Time.time - m_matchStartTime);
                        break;
                    }
                    case ETimerDisplaySettings.DaysToMilliseconds:
                    case ETimerDisplaySettings.DaysToSeconds:
                    case ETimerDisplaySettings.DaysToMinutes:
                    case ETimerDisplaySettings.DaysToHours:
                    {
                        DisplayTimeMath(Time.time - m_matchStartTime);
                        break;
                    }
                    default:
                        break;
                }

                break;
            }
        }
    }

    /// <summary>
    /// Original source: https://www.youtube.com/watch?v=HmHPJL-OcQE to display the round time counter.
    /// </summary>
    /// <param name="_timeToDisplay"></param>
    private void DisplayTimeMathf(float _timeToDisplay)
    {
        switch (m_timerShallCountUp)
        {
            case false: //Counts down == !m_timerShallCountUp.
            {
                if (_timeToDisplay <= 0)
                {
                    _timeToDisplay = 0;
                    InvokeNoRespawnMatchEnds(m_eMatchMode);
                }
                else if (!m_showMilliseconds)
                {
                    _timeToDisplay += 1;
                }

                break;
            }
            case true:  //Counts up == m_timerShallCountUp.
            {
                if (_timeToDisplay >= m_combinedTimeValuesF)
                {
                    _timeToDisplay = m_combinedTimeValuesF;
                    InvokeNoRespawnMatchEnds(m_eMatchMode);
                }

                break;
            }
        }

        switch (m_timerFormatAndRange)
        {
            case ETimerDisplaySettings.SecondsToMilliseconds:
            case ETimerDisplaySettings.MinutesToMilliseconds:
            case ETimerDisplaySettings.HoursToMilliseconds:
            {
                if (!m_showMilliseconds)
                    m_showMilliseconds = true;
                break;
            }
            case ETimerDisplaySettings.Seconds:
            case ETimerDisplaySettings.MinutesToSeconds:
            case ETimerDisplaySettings.HoursToSeconds:
            case ETimerDisplaySettings.HoursToMinutes:
            {
                if (m_showMilliseconds)
                    m_showMilliseconds = false;
                break;
            }
        }

        //Mathf.FloorToInt(hours: '_timeToDisplay / 3600' minutes: '_timeToDisplay / 60', seconds: '_timeToDisplay % 60');.
        //Calculating Hours.
        float hours = Mathf.FloorToInt(_timeToDisplay / 3600 % 24);
        //Calculating Minutes.
        float minutes = Mathf.FloorToInt(_timeToDisplay / 60 % 60);
        //Calculating Seconds.
        float seconds = Mathf.FloorToInt(_timeToDisplay % 60);
        //Calculating Milliseconds:
        double milliseconds = _timeToDisplay % 1 * 1000;

        switch (m_timerFormatAndRange)
        {
            case ETimerDisplaySettings.SecondsToMilliseconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:000}", seconds, milliseconds);
                break;
            }
            case ETimerDisplaySettings.Seconds:
            {
                m_timerDisplay.text = string.Format("{0:00}", seconds);
                break;
            }
            case ETimerDisplaySettings.MinutesToSeconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                break;
            }
            case ETimerDisplaySettings.MinutesToMilliseconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
                break;
            }
            case ETimerDisplaySettings.HoursToMilliseconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, milliseconds);
                break;
            }
            case ETimerDisplaySettings.HoursToSeconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
                break;
            }
            case ETimerDisplaySettings.HoursToMinutes:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}", hours, minutes);
                break;
            }
            default:
                break;
        }

        #region Old if-DisplayTime
        //if (!m_timerShallCountUp)       //Counts down.
        //{
        //    if (_timeToDisplay <= 0)
        //    {
        //        _timeToDisplay = 0;
        //    }
        //    else if (!m_showMilliseconds)
        //    {
        //        _timeToDisplay += 1;
        //    }

        //    UpdateTimeDisplayFormat(m_timerFormatAndRange, _timeToDisplay);
        //}
        //else if (m_timerShallCountUp)   //Counts up.
        //{
        //    if (_timeToDisplay >= m_combinedTimeValuesF)
        //    {
        //        _timeToDisplay = m_combinedTimeValuesF;                
        //    }

        //    UpdateTimeDisplayFormat(m_timerFormatAndRange, _timeToDisplay);
        //}
        #endregion
    }

    private void InvokeNoRespawnMatchEnds(EMatchMode _eMatchMode)
    {
        switch (_eMatchMode)
        {
            case EMatchMode.KillCount:
            {
                KillCountMatchEnds.Invoke((int)m_eMatchMode);
                break;
            }
            case EMatchMode.TeamBattle:
            {
                TeamBattleMatchEnds.Invoke((int)m_eMatchMode);
                break;
            }
            case EMatchMode.NoRespawn:
            {
                NoRespawnMatchEnds.Invoke((int)m_eMatchMode);
                break;
            }
            default:
                break;
        }
    }

    private void EndNoRespawnMatch()
    {
        InvokeNoRespawnMatchEnds(EMatchMode.NoRespawn);
    }

    /// <summary>
    /// Original source: https://www.youtube.com/watch?v=HmHPJL-OcQE to display the round time counter.
    /// </summary>
    /// <param name="_timeToDisplay"></param>
    private void DisplayTimeMath(double _timeToDisplay)
    {
        switch (m_timerShallCountUp)
        {
            case false: //Counts down == !m_timerShallCountUp.
            {
                if (_timeToDisplay <= 0)
                {
                    _timeToDisplay = 0;
                    InvokeNoRespawnMatchEnds(m_eMatchMode);
                }
                else if (!m_showMilliseconds)
                {
                    _timeToDisplay += 1;
                    //if (_timeToDisplay >= m_combinedTimeValuesD)
                    //    _timeToDisplay = m_combinedTimeValuesD;
                }

                break;
            }
            case true:  //Counts up == m_timerShallCountUp.
            {
                if (_timeToDisplay >= m_combinedTimeValuesD)
                {
                    _timeToDisplay = m_combinedTimeValuesD;
                    InvokeNoRespawnMatchEnds(m_eMatchMode);
                }

                break;
            }
        }

        switch (m_timerFormatAndRange)
        {
            case ETimerDisplaySettings.SecondsToMilliseconds:
            case ETimerDisplaySettings.MinutesToMilliseconds:
            case ETimerDisplaySettings.HoursToMilliseconds:
            case ETimerDisplaySettings.DaysToMilliseconds:

            {
                if (!m_showMilliseconds)
                    m_showMilliseconds = true;
                break;
            }
            case ETimerDisplaySettings.Seconds:
            case ETimerDisplaySettings.MinutesToSeconds:
            case ETimerDisplaySettings.HoursToSeconds:
            case ETimerDisplaySettings.HoursToMinutes:
            case ETimerDisplaySettings.DaysToSeconds:
            case ETimerDisplaySettings.DaysToMinutes:
            case ETimerDisplaySettings.DaysToHours:
            {
                if (m_showMilliseconds)
                    m_showMilliseconds = false;
                break;
            }
        }

        //Math.Floor
        //(days: '_timeToDisplay / 86400', hours: '_timeToDisplay / 3600' minutes: '_timeToDisplay / 60', seconds: '_timeToDisplay % 60');.
        //Calculating Days.
        double days = Math.Floor(_timeToDisplay / 86400 % m_moduloLeapYear);
        //Calculating Hours.
        double hours = Math.Floor(_timeToDisplay / 3600 % 24);
        //Calculating Minutes.
        double minutes = Math.Floor(_timeToDisplay / 60 % 60);
        //Calculating Seconds.
        double seconds = Math.Floor(_timeToDisplay % 60);
        //Calculating Milliseconds:
        double milliseconds = _timeToDisplay % 1 * 1000;

        switch (m_timerFormatAndRange)
        {
            case ETimerDisplaySettings.SecondsToMilliseconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:000}", seconds, milliseconds);
                break;
            }
            case ETimerDisplaySettings.Seconds:
            {
                m_timerDisplay.text = string.Format("{0:00}", seconds);
                break;
            }
            case ETimerDisplaySettings.MinutesToSeconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                break;
            }
            case ETimerDisplaySettings.MinutesToMilliseconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
                break;
            }
            case ETimerDisplaySettings.HoursToMilliseconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}:{2:00}:{3:000}", hours, minutes, seconds, milliseconds);
                break;
            }
            case ETimerDisplaySettings.HoursToSeconds:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
                break;
            }
            case ETimerDisplaySettings.HoursToMinutes:
            {
                m_timerDisplay.text = string.Format("{0:00}:{1:00}", hours, minutes);
                break;
            }
            case ETimerDisplaySettings.DaysToMilliseconds:
            {
                m_timerDisplay.text = string.Format("{0:000}:{1:00}:{2:00}:{3:00}:{4:000}", days, hours, minutes, seconds, milliseconds);
                break;
            }
            case ETimerDisplaySettings.DaysToSeconds:
            {
                m_timerDisplay.text = string.Format("{0:000}:{1:00}:{2:00}:{3:00}", days, hours, minutes, seconds);
                break;
            }
            case ETimerDisplaySettings.DaysToMinutes:
            {
                m_timerDisplay.text = string.Format("{0:000}:{1:00}:{2:00}", days, hours, minutes);
                break;
            }
            case ETimerDisplaySettings.DaysToHours:
            {
                m_timerDisplay.text = string.Format("{0:000}:{1:00}", days, hours);
                break;
            }
            default:
                break;
        }
    }

    #region Leap Year Check
    private void UpdateCheckForLeapYear()
    {
        m_thisYear = DateTime.Now.Year;
        m_today = DateTime.Now.Day;

        switch (DateTime.IsLeapYear(m_thisYear))
        {
            case false:
            {
                m_moduloLeapYear = (int)ELeapYear.No;
                //m_silvester = (int)ELeapYear.No;
                break;
            }
            case true:
            {
                m_moduloLeapYear = (int)ELeapYear.Yes;
                //m_silvester = (int)ELeapYear.Yes;
                break;
            }
        }
    }

    private IEnumerator ICheckOnDayChange()
    {
        while (m_matchRuns)
        {
            //Waits until the current day has passed, after the initial check in 'Start()'.
            yield return new WaitWhile(DayPassesBy);
            UpdateCheckForLeapYear();
        }
    }

    #region TrueFalseSwitch-ICheckOnDayChange()-Alternative
    //    private IEnumerator ICheckOnDayChange()
    //    {
    //        while (m_matchRuns)
    //        {
    //            bool compareDayInYear = m_today == m_silvester;
    //            switch (compareDayInYear)
    //            {
    //                case false:
    //                {
    //                    //Coroutine runs without a break. (Or maybe does other stuff later.)
    //                    yield return null;
    //                    break;
    //                }
    //                case true:
    //                {
    //                    //Waiting for the next day to set m_moduloLeapYear again.
    //                    yield return new WaitWhile(DayPassesBy);
    //                    UpdateCheckForLeapYear();
    //                    break;
    //                }
    //            }
    //        }
    //    }
    #endregion
    /// <summary>
    /// DelegateBool to wait, while the previously set 'm_today' still equals to DateTime.Now.Day.
    /// </summary>
    /// <returns></returns>
    private bool DayPassesBy()
    {
        return m_today == DateTime.Now.Day;
        //return m_today == m_silvester;
    }
    #endregion
}