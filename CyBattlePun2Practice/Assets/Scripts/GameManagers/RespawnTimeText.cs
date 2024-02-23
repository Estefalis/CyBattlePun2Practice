using TMPro;
using UnityEngine;

public class RespawnTimeText : MonoBehaviour
{
    [SerializeField] private TMP_Text m_respawnTimeText;
    [SerializeField] private float m_respawnTime = 3.0f;

    private float m_countStartTime;

    private void OnEnable()
    {
        m_countStartTime = Time.time;
    }

    private void Update()
    {
        UpdateTimerDisplay(m_respawnTime - (Time.time - m_countStartTime));
    }

    private void UpdateTimerDisplay(float _timeToDisplay)
    {
        if (_timeToDisplay >= 0)
        {
            //Calculating Seconds.
            float seconds = Mathf.FloorToInt(_timeToDisplay % 60);
            m_respawnTimeText.text = $"Respawning in\n{seconds}";
        }
        else
            gameObject.SetActive(false);
    }
}