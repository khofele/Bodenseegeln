using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

[RequireComponent(typeof(Collider))]
public class WwiseOneShotTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private string m_playerTag = "Player";

    [Header("Wwise")]
    [SerializeField] private GameObject m_emitter = null;

    [SerializeField] private WwiseEvent[] m_playEvents = null;

    private bool m_hasPlayed = false;

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

        if (m_hasPlayed)
        {
            return;
        }

        PlayEvents();
        m_hasPlayed = true;
    }

    private void PlayEvents()
    {
        foreach (WwiseEvent playEvent in m_playEvents)
        {
            playEvent?.Post(m_emitter);
        }
    }
}
