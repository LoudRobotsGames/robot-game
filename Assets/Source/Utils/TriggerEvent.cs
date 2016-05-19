using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class TriggerEvent : MonoBehaviour
{
    public string m_TagToFilter = "Player";
    public UnityEvent m_TriggerEvent;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(m_TagToFilter))
        {
            m_TriggerEvent.Invoke();
        }
    }
}
