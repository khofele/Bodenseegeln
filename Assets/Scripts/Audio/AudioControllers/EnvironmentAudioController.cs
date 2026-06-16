using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class EnvironmentAudioController : MonoBehaviour
{
    // AUDIO REFERENCES
    [Header("Audio Emitters")]
    [SerializeField] private GameObject m_boatHullEmitter = null;
    [SerializeField] private GameObject m_boatUnderwaterEmitter = null;
    [SerializeField] private GameObject m_dockEmitter = null;
    [SerializeField] private GameObject m_interactionEmitter = null;
    [SerializeField] private GameObject m_uiEmitter = null;

    [Header("Boat Collision Events")]
    [SerializeField] private WwiseEvent m_boatCollisionLightEvent = null;
    [SerializeField] private WwiseEvent m_boatCollisionHeavyEvent = null;
    [SerializeField] private WwiseEvent m_boatCollisionUnderWaterEvent = null;

    [Header("Dock Events")]
    [SerializeField] private WwiseEvent m_boatDockImpactEvent = null;
    [SerializeField] private WwiseEvent m_startBoatRubDockEvent = null;
    [SerializeField] private WwiseEvent m_stopBoatRubDockEvent = null;

    [Header("Interaction Events")]
    [SerializeField] private WwiseEvent m_boatRefuelEvent = null;
    [SerializeField] private WwiseEvent m_boatRepairEvent = null;

    [Header("Dock / Water Loop Events")]
    [SerializeField] private WwiseEvent m_startWaterDockEvent = null;
    [SerializeField] private WwiseEvent m_stopWaterDockEvent = null;

    [Header("Map Events")]
    [SerializeField] private WwiseEvent m_openMapEvent = null;
    [SerializeField] private WwiseEvent m_closeMapEvent = null;

    // PRIVATE METHODS ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        if (m_boatHullEmitter == null)
        {
            m_boatHullEmitter = gameObject;
        }

        if (m_boatUnderwaterEmitter == null)
        {
            m_boatUnderwaterEmitter = m_boatHullEmitter;
        }

        if (m_dockEmitter == null)
        {
            m_dockEmitter = gameObject;
        }

        if (m_interactionEmitter == null)
        {
            m_interactionEmitter = gameObject;
        }

        if (m_uiEmitter == null)
        {
            m_uiEmitter = gameObject;
        }
    }

    private void PlayEvent(WwiseEvent wwiseEvent, GameObject emitter)
    {
        if (wwiseEvent == null || emitter == null)
        {
            return;
        }

        wwiseEvent.Post(emitter);
    }

    // PUBLIC METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void PlayBoatCollisionLight()
    {
        PlayEvent(m_boatCollisionLightEvent, m_boatHullEmitter);
    }

    public void PlayBoatCollisionHeavy()
    {
        PlayEvent(m_boatCollisionHeavyEvent, m_boatHullEmitter);
    }

    public void PlayBoatCollisionUnderWater()
    {
        PlayEvent(m_boatCollisionUnderWaterEvent, m_boatUnderwaterEmitter);
    }

    public void PlayBoatDockImpact()
    {
        PlayEvent(m_boatDockImpactEvent, m_dockEmitter);
    }

    public void StartBoatRubDock()
    {
        PlayEvent(m_startBoatRubDockEvent, m_dockEmitter);
    }

    public void StopBoatRubDock()
    {
        PlayEvent(m_stopBoatRubDockEvent, m_dockEmitter);
    }

    public void PlayBoatRefuel()
    {
        PlayEvent(m_boatRefuelEvent, m_interactionEmitter);
    }

    public void PlayBoatRepair()
    {
        PlayEvent(m_boatRepairEvent, m_interactionEmitter);
    }

    public void StartWaterDock()
    {
        PlayEvent(m_startWaterDockEvent, m_dockEmitter);
    }

    public void StopWaterDock()
    {
        PlayEvent(m_stopWaterDockEvent, m_dockEmitter);
    }

    public void PlayOpenMap()
    {
        PlayEvent(m_openMapEvent, m_uiEmitter);
    }

    public void PlayCloseMap()
    {
        PlayEvent(m_closeMapEvent, m_uiEmitter);
    }
}
