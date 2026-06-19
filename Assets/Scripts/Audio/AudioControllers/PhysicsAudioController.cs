using UnityEngine;
using WwiseEvent = AK.Wwise.Event;
using WwiseRTPC = AK.Wwise.RTPC;

public class PhysicsAudioController : MonoBehaviour
{
    // AUDIO REFERENCES
    [Header("Audio Emitters")]
    [SerializeField] private GameObject m_hullWaterEmitter = null;
    [SerializeField] private GameObject m_bowWaterEmitter = null;
    [SerializeField] private GameObject m_wakeWaterEmitter = null;
    [SerializeField] private GameObject m_boatWindEmitter = null;
    [SerializeField] private GameObject m_sailWindEmitter = null;
    [SerializeField] private GameObject m_waveMotionEmitter = null;

    [Header("Water Events")]
    [SerializeField] private WwiseEvent m_waveHullStartEvent = null;
    [SerializeField] private WwiseEvent m_waveHullStopEvent = null;

    [SerializeField] private WwiseEvent m_bowWaterStartEvent = null;
    [SerializeField] private WwiseEvent m_bowWaterStopEvent = null;

    [SerializeField] private WwiseEvent m_wakeWaterStartEvent = null;
    [SerializeField] private WwiseEvent m_wakeWaterStopEvent = null;

    [SerializeField] private WwiseEvent m_waterDisplacementStartEvent = null;
    [SerializeField] private WwiseEvent m_waterDisplacementStopEvent = null;

    [SerializeField] private WwiseEvent m_hullWaterMassStartEvent = null;
    [SerializeField] private WwiseEvent m_hullWaterMassStopEvent = null;

    [Header("Wind Events")]
    [SerializeField] private WwiseEvent m_windForceBoatStartEvent = null;
    [SerializeField] private WwiseEvent m_windForceBoatStopEvent = null;

    [SerializeField] private WwiseEvent m_windForceSailStartEvent = null;
    [SerializeField] private WwiseEvent m_windForceSailStopEvent = null;

    [Header("Wave Motion Events")]
    [SerializeField] private WwiseEvent m_waveMotionStartEvent = null;
    [SerializeField] private WwiseEvent m_waveMotionStopEvent = null;

    [Header("Audio Game Parameters")]
    [SerializeField] private WwiseRTPC m_phyWaterMovementRTPC = null;
    [SerializeField] private WwiseRTPC m_windBoatIntensityRTPC = null;
    [SerializeField] private WwiseRTPC m_windSailIntensityRTPC = null;
    //[SerializeField] private WwiseRTPC m_sailTensionRTPC = null;

    [Header("RTPC Mappings")]
    [SerializeField]
    private RTPCMapping m_waterMovementMapping = new RTPCMapping
    {
        unityMin = 0.0f,
        unityMax = 15.0f,
        wwiseMin = 0.0f,
        wwiseMax = 100.0f
    };

    [SerializeField]
    private RTPCMapping m_boatWindIntensityMapping = new RTPCMapping
    {
        unityMin = 0.0f,
        unityMax = 100.0f,
        wwiseMin = 0.0f,
        wwiseMax = 100.0f
    };

    [SerializeField]
    private RTPCMapping m_sailWindIntensityMapping = new RTPCMapping
    {
        unityMin = 0.0f,
        unityMax = 100.0f,
        wwiseMin = 0.0f,
        wwiseMax = 100.0f
    };

    private bool m_isPhysicsAudioPlaying = false;
    private bool m_isSailPhysicsAudioPlaying = false;

    // PRIVATE METHODS ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        if (m_hullWaterEmitter == null)
        {
            m_hullWaterEmitter = gameObject;
        }

        if (m_bowWaterEmitter == null)
        {
            m_bowWaterEmitter = gameObject;
        }

        if (m_wakeWaterEmitter == null)
        {
            m_wakeWaterEmitter = gameObject;
        }

        if (m_boatWindEmitter == null)
        {
            m_boatWindEmitter = gameObject;
        }

        if (m_sailWindEmitter == null)
        {
            m_sailWindEmitter = gameObject;
        }

        if (m_waveMotionEmitter == null)
        {
            m_waveMotionEmitter = gameObject;
        }
    }

    private void OnDisable()
    {
        StopPhysicsAudio();
        StopSailPhysicsAudio();
    }

    private void OnDestroy()
    {
        StopPhysicsAudio();
        StopSailPhysicsAudio();
    }

    private void PlayEvent(WwiseEvent wwiseEvent, GameObject emitter)
    {
        if (wwiseEvent == null || emitter == null)
        {
            return;
        }

        wwiseEvent.Post(emitter);
    }

    private void SetRTPC(WwiseRTPC rtpc, GameObject emitter, float value)
    {
        if (rtpc == null || emitter == null)
        {
            return;
        }

        rtpc.SetValue(emitter, value);
    }

    // PUBLIC METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void StartPhysicsAudio()
    {
        if (m_isPhysicsAudioPlaying)
        {
            return;
        }

        SetPhysicsRTPCsToDefault();

        PlayEvent(m_waveHullStartEvent, m_hullWaterEmitter);
        PlayEvent(m_bowWaterStartEvent, m_bowWaterEmitter);
        PlayEvent(m_wakeWaterStartEvent, m_wakeWaterEmitter);
        PlayEvent(m_waterDisplacementStartEvent, m_hullWaterEmitter);

        PlayEvent(m_windForceBoatStartEvent, m_boatWindEmitter);

        PlayEvent(m_waveMotionStartEvent, m_waveMotionEmitter);

        PlayEvent(m_hullWaterMassStartEvent, m_hullWaterEmitter);

        m_isPhysicsAudioPlaying = true;
    }

    public void StopPhysicsAudio()
    {
        if (!m_isPhysicsAudioPlaying)
        {
            return;
        }

        PlayEvent(m_waveHullStopEvent, m_hullWaterEmitter);
        PlayEvent(m_bowWaterStopEvent, m_bowWaterEmitter);
        PlayEvent(m_wakeWaterStopEvent, m_wakeWaterEmitter);
        PlayEvent(m_waterDisplacementStopEvent, m_hullWaterEmitter);

        PlayEvent(m_windForceBoatStopEvent, m_boatWindEmitter);

        PlayEvent(m_waveMotionStopEvent, m_waveMotionEmitter);

        PlayEvent(m_hullWaterMassStopEvent, m_hullWaterEmitter);

        m_isPhysicsAudioPlaying = false;
    }

    public void SetPhysicsRTPCsToDefault()
    {
        SetWaterMovement(0.0f);
        SetBoatWindIntensity(0.0f);
        SetSailWindIntensity(0.0f);
        //SetSailTension(100.0f);
    }

    public void StartSailPhysicsAudio()
    {
        if (m_isSailPhysicsAudioPlaying)
        {
            return;
        }

        PlayEvent(m_windForceSailStartEvent, m_sailWindEmitter);

        m_isSailPhysicsAudioPlaying = true;
    }

    public void StopSailPhysicsAudio()
    {
        if (!m_isSailPhysicsAudioPlaying)
        {
            return;
        }

        PlayEvent(m_windForceSailStopEvent, m_sailWindEmitter);

        m_isSailPhysicsAudioPlaying = false;
    }

    public void SetWaterMovement(float waterMovement)
    {
        //float clampedValue = Mathf.Clamp(waterMovement, 0.0f, 100.0f);

        //SetRTPC(m_phyWaterMovementRTPC, m_hullWaterEmitter, clampedValue);
        //SetRTPC(m_phyWaterMovementRTPC, m_bowWaterEmitter, clampedValue);
        //SetRTPC(m_phyWaterMovementRTPC, m_wakeWaterEmitter, clampedValue);
        //SetRTPC(m_phyWaterMovementRTPC, m_waveMotionEmitter, clampedValue);

        float mappedValue = m_waterMovementMapping.Map(waterMovement);

        SetRTPC(m_phyWaterMovementRTPC, m_hullWaterEmitter, mappedValue);
        SetRTPC(m_phyWaterMovementRTPC, m_bowWaterEmitter, mappedValue);
        SetRTPC(m_phyWaterMovementRTPC, m_wakeWaterEmitter, mappedValue);
        SetRTPC(m_phyWaterMovementRTPC, m_waveMotionEmitter, mappedValue);
    }

    public void SetBoatWindIntensity(float windIntensity)
    {
        //float clampedValue = Mathf.Clamp(windIntensity, 0.0f, 100.0f);

        //SetRTPC(m_windBoatIntensityRTPC, m_boatWindEmitter, clampedValue);

        float mappedValue = m_boatWindIntensityMapping.Map(windIntensity);

        SetRTPC(m_windBoatIntensityRTPC, m_boatWindEmitter, mappedValue);
    }

    public void SetSailWindIntensity(float windIntensity)
    {
        //float clampedValue = Mathf.Clamp(windIntensity, 0.0f, 100.0f);

        //SetRTPC(m_windSailIntensityRTPC, m_sailWindEmitter, clampedValue);

        float mappedValue = m_sailWindIntensityMapping.Map(windIntensity);

        SetRTPC(m_windSailIntensityRTPC, m_sailWindEmitter, mappedValue);
    }

    //public void SetSailTension(float sailTension)
    //{
    //    float clampedValue = Mathf.Clamp(sailTension, 0.0f, 100.0f);

    //    SetRTPC(m_sailTensionRTPC, m_sailWindEmitter, clampedValue);
    //}

    public void UpdatePhysicsAudioValues(
        float waterMovement,
        float boatWindIntensity,
        float sailWindIntensity,
        float sailTension)
    {
        SetWaterMovement(waterMovement);
        SetBoatWindIntensity(boatWindIntensity);
        SetSailWindIntensity(sailWindIntensity);
        //SetSailTension(sailTension);
    }
}
