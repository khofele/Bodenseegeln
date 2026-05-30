using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // GENERAL FIELDS
    [SerializeField] private GameManager m_gameManager = null;
    [SerializeField] private CinemachineOrbitalFollow m_orbitalFollow = null;

    // INPUT ACTIONS
    [SerializeField] private InputActionReference m_resetCameraAction = null;
    [SerializeField] private InputActionReference m_mouseLookAction = null;
    [SerializeField] private InputActionReference m_lookLeftAction = null;
    [SerializeField] private InputActionReference m_lookRightAction = null;

    private bool m_isResetting = false;

    private void OnEnable()
    {
        if (m_resetCameraAction != null)
        {
            m_resetCameraAction.action.Enable();
        }

        if (m_mouseLookAction != null)
        {
            m_mouseLookAction.action.Enable();
        }

        if (m_lookLeftAction != null)
        {
            m_lookLeftAction.action.Enable();
        }

        if (m_lookRightAction != null)
        {
            m_lookRightAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (m_resetCameraAction != null)
        {
            m_resetCameraAction.action.Disable();
        }

        if (m_mouseLookAction != null)
        {
            m_mouseLookAction.action.Disable();
        }

        if (m_lookLeftAction != null)
        {
            m_lookLeftAction.action.Disable();
        }

        if (m_lookRightAction != null)
        {
            m_lookRightAction.action.Disable();
        }
    }

    private void CheckStartReset()
    {
        if (m_resetCameraAction != null && m_resetCameraAction.action.triggered == true)
        {
            StartReset();
        }
    }

    private void TriggerStopReset()
    {
        if (m_isResetting && m_mouseLookAction != null)
        {
            Vector2 lookInput = m_mouseLookAction.action.ReadValue<Vector2>();

            if (lookInput.sqrMagnitude > 0.0001f)
            {
                StopReset();
            }
        }
    }

    private void StartReset()
    {
        m_isResetting = true;

        InputAxis horizontal = m_orbitalFollow.HorizontalAxis;
        horizontal.TriggerRecentering();
        m_orbitalFollow.HorizontalAxis = horizontal;

        InputAxis vertical = m_orbitalFollow.VerticalAxis;
        vertical.TriggerRecentering();
        m_orbitalFollow.VerticalAxis = vertical;

        InputAxis radial = m_orbitalFollow.RadialAxis;
        radial.TriggerRecentering();
        m_orbitalFollow.RadialAxis = radial;
    }

    private void StopReset()
    {
        m_isResetting = false;
    }

    private void TriggerLookLeftRight()
    {
        if (m_lookLeftAction.action.triggered == true)
        {
            // TODO implement: lerp to left
        }

        if (m_lookRightAction.action.triggered == true)
        {
            // TODO implement: lerp to right
        }
    }

    private void Update()
    {
        if(m_gameManager.CurrentState == GameStates.SAILMODE || m_gameManager.CurrentState == GameStates.MOTORMODE)
        {
            if (m_orbitalFollow == null)
            {
                return;
            }

            TriggerLookLeftRight();

            CheckStartReset();
            TriggerStopReset();
        }
    }
}