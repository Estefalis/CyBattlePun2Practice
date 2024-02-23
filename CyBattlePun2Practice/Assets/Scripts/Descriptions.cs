using UnityEngine;
using UnityEngine.EventSystems;

public class Descriptions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject m_dropdown;

    public void OnPointerEnter(PointerEventData eventData)
    {
        m_dropdown.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_dropdown.SetActive(false);
    }

    private void Start()
    {
        m_dropdown.SetActive(false);
    }
}
