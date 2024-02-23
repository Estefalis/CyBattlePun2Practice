using UnityEngine;

public class WeaponRotation : MonoBehaviour
{
    public float m_rotationSpeed = 20.0f;

    private void Update()
    {
        transform.Rotate(0.0f, m_rotationSpeed * Time.deltaTime, 0.0f);
    }
}
