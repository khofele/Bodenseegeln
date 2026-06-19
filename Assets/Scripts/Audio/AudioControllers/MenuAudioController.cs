using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class MenuAudioController : MonoBehaviour
{
    [Header("Menu Navigation Events")]
    [SerializeField] private WwiseEvent m_navigateForwardEvent = null;
    [SerializeField] private WwiseEvent m_navigateBackwardEvent = null;

    [Header("Menu Open Events")]
    [SerializeField] private WwiseEvent m_openMainMenuEvent = null;
    [SerializeField] private WwiseEvent m_openPauseMenuEvent = null;

    [Header("Settings Events")]
    [SerializeField] private WwiseEvent m_modifiedSettingsEvent = null;

    [Header("Game Start Event")]
    [SerializeField] private WwiseEvent m_startGameEvent = null;

    [Header("Main Menu Ambient Events")]
    [SerializeField] private WwiseEvent m_menuAmbientStartEvent = null;
    [SerializeField] private WwiseEvent m_menuAmbientStopEvent = null;

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
    public void PlayVolumeSliderChanged()
    {
        PlayEvent(m_navigateForwardEvent);
    }

    public void PlayNavigateBackward()
    {
        PlayEvent(m_navigateBackwardEvent);
    }

    public void PlayEndGame()
    {
        PlayEvent(m_openMainMenuEvent);
    }

    public void PlayOpenPauseMenu()
    {
        PlayEvent(m_openPauseMenuEvent);
    }

    public void PlayModifiedSettings()
    {
        PlayEvent(m_modifiedSettingsEvent);
    }

    public void PlayStartGame()
    {
        PlayEvent(m_startGameEvent);
    }

    public void StartMenuAmbient()
    {
        PlayEvent(m_menuAmbientStartEvent);
    }

    public void StopMenuAmbient()
    {
        PlayEvent(m_menuAmbientStopEvent);
    }
}
