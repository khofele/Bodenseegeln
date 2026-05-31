using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    // GENERAL FIELDS
    private CameraStates m_currentState = CameraStates.MIDDLE;
    private bool m_isResettingCamera = false;
    private bool m_isResettingRadius = false;
    private bool m_isChangingCameraToLeftRight = false;
    private Transform m_boatTransform = null;
    private float m_radiusLeftRight = 4.0f;
    private float m_radiusMiddle = 10.0f;
    
    [Header("General Fields")]
    [SerializeField] private GameManager m_gameManager = null;
    [SerializeField] private CinemachineOrbitalFollow m_orbitalFollow = null;
    [SerializeField] private Transform m_cameraTargetLeft = null;
    [SerializeField] private Transform m_cameraTargetRight = null;

    // INPUT ACTIONS
    [Header("Input Action References")]
    [SerializeField] private InputActionReference m_resetCameraAction = null;
    [SerializeField] private InputActionReference m_mouseLookAction = null;
    [SerializeField] private InputActionReference m_lookLeftAction = null;
    [SerializeField] private InputActionReference m_lookRightAction = null;

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

    private void TriggerStartReset()
    {
        if (m_isResettingCamera == true || m_isResettingRadius == true || m_isChangingCameraToLeftRight == true)
        {
            return;
        }

        if (m_resetCameraAction.action.WasPressedThisFrame() == true)
        {
            // differentiate between resets
            if (m_currentState == CameraStates.MIDDLE)
            {
                // reset camera back to default position
                m_isResettingCamera = true;
            }
            else if (m_currentState == CameraStates.LEFT || m_currentState == CameraStates.RIGHT)
            {   
                // reset camera back to middle mode + default position
                m_orbitalFollow.GetComponent<CinemachineCamera>().Follow = m_boatTransform;
                m_currentState = CameraStates.MIDDLE;
                m_isResettingRadius = true;
                m_isResettingCamera = true;
            }
        }
    }

    private void TriggerStopCameraReset()
    {
        // stop camera reset when mouse moves
        if (m_isResettingCamera == true)
        {
            Vector2 lookInput = m_mouseLookAction.action.ReadValue<Vector2>();

            if (lookInput.sqrMagnitude > 3.0f)
            {
                m_isResettingCamera = false;
            }
        }
    }

    private void ApplyCameraReset()
    {
        if(m_isResettingCamera == true)
        {
            if (Mathf.Abs(m_orbitalFollow.HorizontalAxis.Value) < 0.1f)
            {
                m_isResettingCamera = false;
            }
            
            m_orbitalFollow.HorizontalAxis.TriggerRecentering();
            m_orbitalFollow.VerticalAxis.TriggerRecentering();
            m_orbitalFollow.RadialAxis.TriggerRecentering();
        }
    }

    private void ApplyRadiusReset()
    {
        // reset radius from left/right to middle mode radius
        if(m_isResettingRadius == true)
        {
            m_orbitalFollow.Radius = Mathf.Lerp(m_orbitalFollow.Radius, m_radiusMiddle, Time.deltaTime * 4.0f);

            // tiny threshold needed to make lerp a little shorter --> otherwise lerp steps can get supersmall and lerp takes forever 
            if (m_orbitalFollow.Radius >= m_radiusMiddle - 0.05f)
            {
                m_orbitalFollow.Radius = m_radiusMiddle;
                m_isResettingRadius = false;
            }
        }
    }

    private void ApplyCameraChangeToLeftRight() 
    {
        // move camera from middle to left/right + change radius
        if (m_isChangingCameraToLeftRight == true)
        {
            m_orbitalFollow.Radius = Mathf.Lerp(m_orbitalFollow.Radius, m_radiusLeftRight, Time.deltaTime * 4.0f);

            // tiny threshold needed to make lerp a little shorter --> otherwise lerp steps can get supersmall and lerp takes forever 
            if (m_orbitalFollow.Radius <= m_radiusLeftRight + 0.05f)
            {
                m_orbitalFollow.Radius = m_radiusLeftRight;
                m_isChangingCameraToLeftRight = false;
            }
        }
    }

    private void TriggerLookLeftRight()
    {
        if(m_isResettingCamera == true || m_isResettingRadius == true || m_isChangingCameraToLeftRight == true)
        {
            return;
        }

        if (m_lookLeftAction.action.triggered == true)
        {
            m_currentState = CameraStates.LEFT;
            m_orbitalFollow.GetComponent<CinemachineCamera>().Follow = m_cameraTargetLeft;
            m_isChangingCameraToLeftRight = true;
        }

        if (m_lookRightAction.action.triggered == true)
        {
            m_currentState = CameraStates.RIGHT;
            m_orbitalFollow.GetComponent<CinemachineCamera>().Follow = m_cameraTargetRight;
            m_isChangingCameraToLeftRight = true;
        }
    }

    private void EnableRotationalCameraInput(bool isEnabled)
    {
        m_orbitalFollow.GetComponent<CinemachineInputAxisController>().enabled = isEnabled;
    }

    private void Start()
    {
        m_currentState = CameraStates.MIDDLE;
        m_boatTransform = m_orbitalFollow.FollowTarget;
    }

    private void Update()
    {
        if (m_gameManager.CurrentState == GameStates.SAILMODE || m_gameManager.CurrentState == GameStates.MOTORMODE)
        {
            TriggerLookLeftRight();

            TriggerStartReset();
            TriggerStopCameraReset();

            ApplyCameraReset();
            ApplyRadiusReset();

            ApplyCameraChangeToLeftRight();

            EnableRotationalCameraInput(true);
        }
        else
        {
            EnableRotationalCameraInput(false);
        }
    }
}