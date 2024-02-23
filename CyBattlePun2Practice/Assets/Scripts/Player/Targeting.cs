using UnityEngine;

public class Targeting : MonoBehaviour
{
    [SerializeField] private GameObject m_crosshair;

    private Vector3 m_worldPosition;
    private Vector3 m_screenPosition;

    private void FixedUpdate()
    {
        m_screenPosition = Input.mousePosition;
        m_screenPosition.z = 6.0f;

        m_worldPosition = Camera.main.ScreenToWorldPoint(m_screenPosition);
        transform.position = m_worldPosition;

        m_crosshair.transform.position = Input.mousePosition;
    }
}