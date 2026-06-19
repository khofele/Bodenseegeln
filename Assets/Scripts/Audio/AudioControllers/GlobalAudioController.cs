using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class GlobalAudioController : MonoBehaviour
{
    [SerializeField] private WwiseEvent m_stopAllAudioEvent = null;

    public void StopAllAudio()
    {
        if (m_stopAllAudioEvent != null)
        {
            m_stopAllAudioEvent.Post(gameObject);
        }
    }
}
