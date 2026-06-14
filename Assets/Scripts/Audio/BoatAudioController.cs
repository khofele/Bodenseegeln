using UnityEngine;
using WwiseEvent = AK.Wwise.Event;
using WwiseRTPC = AK.Wwise.RTPC;

public class BoatAudioController : MonoBehaviour
{
    // AUDIO REFERENCES
    [Header("Audio Emitters")]
    [SerializeField] private GameObject m_boatEngineEmitter = null;
    [SerializeField] private GameObject m_boatSternWaterEmitter = null;
    [SerializeField] private GameObject m_boatCockpitEmitter = null;
    [SerializeField] private GameObject m_boatHullEmitter = null;
    [SerializeField] private GameObject m_boatSailEmitter = null;

    [Header("Audio Events")]
    [SerializeField] private WwiseEvent m_engineStartEvent = null;
    [SerializeField] private WwiseEvent m_engineStopEvent = null;
    [SerializeField] private WwiseEvent m_engineStartKeyEvent = null;
    [SerializeField] private WwiseEvent m_throttleMoveEvent = null;
    [SerializeField] private WwiseEvent m_throttleMoveIdleEvent = null;
    [SerializeField] private WwiseEvent m_engineWaterStartEvent = null;
    [SerializeField] private WwiseEvent m_engineWaterStopEvent = null;
    [SerializeField] private WwiseEvent m_woodVibrationStartEvent = null;
    [SerializeField] private WwiseEvent m_woodVibrationStopEvent = null;
    [SerializeField] private WwiseEvent m_sailDeployEvent = null;
    [SerializeField] private WwiseEvent m_sailRetractEvent = null;
    [SerializeField] private WwiseEvent m_sailLoopStartEvent = null;
    [SerializeField] private WwiseEvent m_sailLoopStopEvent = null;

    [Header("Audio Game Parameters")]
    [SerializeField] private WwiseRTPC m_boatThrottleSignedRTPC = null;
    [SerializeField] private WwiseRTPC m_sailTensionRTPC = null;
    
    // PRIVATE METHODS ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void RetractSailAudio()
    {
        m_sailRetractEvent.Post(m_boatSailEmitter);  // Audio Event: Close Sail
    }

    private void DeploySailAudio()
    {
        m_sailDeployEvent.Post(m_boatSailEmitter);  // Audio Event: Open Sail
    }

    private void StartEngineAudio()
    {
        m_boatThrottleSignedRTPC.SetValue(m_boatEngineEmitter, 0.0f);  // Audio RTPC: Set to 0 for Wwise 
        m_boatThrottleSignedRTPC.SetValue(m_boatHullEmitter, 0.0f);  // Audio RTPC: Set to 0 for Wwise
        m_boatThrottleSignedRTPC.SetValue(m_boatSternWaterEmitter, 0.0f);  // Audio RTPC: Set to 0 for Wwise
        m_engineStartKeyEvent.Post(m_boatCockpitEmitter);  // Audio Event: Start Key
        m_engineStartEvent.Post(m_boatEngineEmitter);  // Audio Event: Start Engine
        m_engineWaterStartEvent.Post(m_boatSternWaterEmitter);  // Audio Event: Water Engine Sound
        m_woodVibrationStartEvent.Post(m_boatHullEmitter);  // Audio Event: Wood Vibration
    }

    private void StopEngineAudio()
    {
        m_engineStopEvent.Post(m_boatEngineEmitter);  // Audio Event: Stop Engine
        m_engineWaterStopEvent.Post(m_boatSternWaterEmitter);  // Audio Event: Stop Water Engine Sounds
        m_woodVibrationStopEvent.Post(m_boatHullEmitter);  // Audio Event: Stop Wood Vibration
        m_boatThrottleSignedRTPC.SetValue(m_boatEngineEmitter, 0.0f);  // Audio RTPC: Set to 0 for Wwise 
        m_boatThrottleSignedRTPC.SetValue(m_boatHullEmitter, 0.0f);  // Audio RTPC: Set to 0 for Wwise
        m_boatThrottleSignedRTPC.SetValue(m_boatSternWaterEmitter, 0.0f);  // Audio RTPC: Set to 0 for Wwise
    }

    // PUBLIC METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void PlayMotormodeAudio()
    {
        RetractSailAudio();
        StartEngineAudio();
    }

    public void PlaySailmodeAudio()
    {
        DeploySailAudio();
        StopEngineAudio();
    }

    public void PlayMotorAudio()
    {
        StartEngineAudio();
    }

    public void StopMotorAudio()
    {
        StopEngineAudio();
    }

    public void PlayThrottleIdleAudio()
    {
        m_throttleMoveIdleEvent.Post(m_boatCockpitEmitter);
    }

    public void PlayThrottleAudio()
    {
        m_throttleMoveEvent.Post(m_boatCockpitEmitter);
    }

    public void SetThrottleValues(float thrustStep)
    {
        // Send current throttle lever value to Wwise RTPC.
        //AkUnitySoundEngine.SetRTPCValue("Boat_ThrottleSigned", m_thrustStep, gameObject);
        m_boatThrottleSignedRTPC.SetValue(m_boatEngineEmitter, thrustStep);
        m_boatThrottleSignedRTPC.SetValue(m_boatHullEmitter, thrustStep);
        m_boatThrottleSignedRTPC.SetValue(m_boatSternWaterEmitter, thrustStep);
    }

    public void StartSailFluttering()
    {
        m_sailLoopStartEvent.Post(m_boatSailEmitter);
    }

    public void StopSailFluttering()
    {
        m_sailLoopStopEvent.Post(m_boatSailEmitter);
    }

    public void SetSailAngle(float sailAngle)
    {
        m_sailTensionRTPC.SetValue(m_boatSailEmitter, sailAngle);
    }
}
