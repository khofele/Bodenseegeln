using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetCamera : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow m_orbitalFollow = null;
    [SerializeField] private InputActionReference m_resetCameraAction = null;
    [SerializeField] private InputActionReference m_lookAction = null;

    private bool m_isResetting = false;

    private void OnEnable()
    {
        if (m_resetCameraAction != null)
            m_resetCameraAction.action.Enable();

        if (m_lookAction != null)
            m_lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (m_resetCameraAction != null)
            m_resetCameraAction.action.Disable();

        if (m_lookAction != null)
            m_lookAction.action.Disable();
    }

    private void Update()
    {
        if (m_orbitalFollow == null)
            return;

        if (m_resetCameraAction != null &&
            m_resetCameraAction.action.triggered)
        {
            StartReset();
        }

        if (m_isResetting && m_lookAction != null)
        {
            Vector2 lookInput = m_lookAction.action.ReadValue<Vector2>();

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
}