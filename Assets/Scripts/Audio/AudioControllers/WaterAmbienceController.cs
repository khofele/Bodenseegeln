using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class WaterAmbienceController : MonoBehaviour
{
    [Header("Water Ambience Events")]
    [SerializeField] private WwiseEvent m_waterAmbienceStartEvent = null;
    [SerializeField] private WwiseEvent m_waterAmbienceStopEvent = null;

    [Header("Settings")]
    [SerializeField] private bool m_startOnGameStart = true;

    private bool m_isWaterAmbiencePlaying = false;

    private void Start()
    {
        if (m_startOnGameStart)
        {
            StartWaterAmbience();
        }
    }

    public void StartWaterAmbience()
    {
        if (m_isWaterAmbiencePlaying)
        {
            return;
        }

        if (m_waterAmbienceStartEvent != null)
        {
            m_waterAmbienceStartEvent.Post(gameObject);
        }

        m_isWaterAmbiencePlaying = true;
    }

    public void StopWaterAmbience()
    {
        if (!m_isWaterAmbiencePlaying)
        {
            return;
        }

        if (m_waterAmbienceStopEvent != null)
        {
            m_waterAmbienceStopEvent.Post(gameObject);
        }

        m_isWaterAmbiencePlaying = false;
    }
}
