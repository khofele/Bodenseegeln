using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class MissionsFeedbackAudioController : MonoBehaviour
{
    [Header("Mission Feedback Events")]
    [SerializeField] private WwiseEvent m_missionAcceptEvent = null;
    [SerializeField] private WwiseEvent m_missionCompleteEvent = null;
    [SerializeField] private WwiseEvent m_missionFailEvent = null;
    [SerializeField] private WwiseEvent m_missionAbortEvent = null;
    [SerializeField] private WwiseEvent m_missionVoiceLineNpcEvent = null;

    [Header("Money Events")]
    [SerializeField] private WwiseEvent m_moneyZeroEvent = null;

    [Header("Goal Events")]
    [SerializeField] private WwiseEvent m_goalReachedEvent = null;
    [SerializeField] private WwiseEvent m_goalFailEvent = null;

    [Header("Warning Events")]
    [SerializeField] private WwiseEvent m_boatDamageEvent = null;
    [SerializeField] private WwiseEvent m_fastMovementEvent = null;

    // PRIVATE METHODS ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void PlayEvent(WwiseEvent wwiseEvent)
    {
        if (wwiseEvent == null)
        {
            return;
        }

        wwiseEvent.Post(gameObject);
    }

    // PUBLIC METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void PlayMissionAccept()
    {
        PlayEvent(m_missionAcceptEvent);
    }

    public void PlayMissionComplete()
    {
        PlayEvent(m_missionCompleteEvent);
    }

    public void PlayMissionFail()
    {
        PlayEvent(m_missionFailEvent);
    }

    public void PlayMissionAbort()
    {
        PlayEvent(m_missionAbortEvent);
    }

    public void PlayMissionVoiceLineNpc()
    {
        PlayEvent(m_missionVoiceLineNpcEvent);
    }

    public void PlayMoneyZero()
    {
        PlayEvent(m_moneyZeroEvent);
    }

    public void PlayGoalReached()
    {
        PlayEvent(m_goalReachedEvent);
    }

    public void PlayGoalFail()
    {
        PlayEvent(m_goalFailEvent);
    }

    public void PlayBoatDamage()
    {
        PlayEvent(m_boatDamageEvent);
    }

    public void PlayFastMovement()
    {
        PlayEvent(m_fastMovementEvent);
    }
}
