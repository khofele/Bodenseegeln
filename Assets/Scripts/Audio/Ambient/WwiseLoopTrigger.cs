using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

[RequireComponent(typeof(Collider))]
public class WwiseLoopTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string m_playerTag = "Player";

    [Header("Wwise")]
    [SerializeField] private GameObject m_emitter = null;

    [SerializeField] private WwiseEvent[] m_startEvents = null;
    [SerializeField] private WwiseEvent[] m_stopEvents = null;

    private bool m_isPlaying = false;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        if (m_emitter == null)
        {
            m_emitter = gameObject;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(m_playerTag))
        {
            return;
        }

        if (m_isPlaying)
        {
            return;
        }

        PlayStartEvents();
        m_isPlaying = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(m_playerTag))
        {
            return;
        }

        if (!m_isPlaying)
        {
            return;
        }

        PlayStopEvents();
        m_isPlaying = false;
    }

    private void PlayStartEvents()
    {
        foreach (WwiseEvent startEvent in m_startEvents)
        {
            startEvent?.Post(m_emitter);
        }
    }

    private void PlayStopEvents()
    {
        foreach (WwiseEvent stopEvent in m_stopEvents)
        {
            stopEvent?.Post(m_emitter);
        }
    }

}
