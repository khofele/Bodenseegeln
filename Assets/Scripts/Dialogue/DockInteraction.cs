using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Dialogue;

public class DockInteraction : MonoBehaviour
{
    [SerializeField] private bool m_showGizmos = true;

    [Header("Detection")]
    [SerializeField] private float m_maxBoatSpeed = 0.5f;

    [Header("Parking")]
    [SerializeField] private bool m_requireCorrectParking = false;
    [SerializeField] private float m_allowedAngleTolerance = 25f;

    [Header("Parking Zone")]
    [SerializeField] private ParkingZoneVisualizer m_parkingZone = null;
    [SerializeField] private float m_zoneVisibilityDistance = 40f;

    [Header("References")]
    [SerializeField] private Transform m_boatTransform = null;

    [Header("UI")]
    [SerializeField] private GameObject m_interactionIcon = null;
    [SerializeField] private GameObject m_dockUIPanel = null;
    [SerializeField] private DockUIController m_dockUIController = null;
    [SerializeField] private Sprite m_DockBackground = null;

    [Header("Input")]
    [SerializeField] private InputActionReference m_interactAction = null;

    private Rigidbody m_boatRigidbody = null;
    private bool m_isPlayerInRange = false;
    private bool m_isDocked = false;
    private bool m_lastInteractableState = false;

    internal Transform CurrentBoat => m_boatTransform;
    internal Sprite DockBackgroundImage => m_DockBackground;

    private void OnEnable()
    {
        if (m_interactAction != null)
        {
            m_interactAction.action.Enable();
        }

        if (m_boatRigidbody == null)
        {
            m_boatRigidbody = m_boatTransform.GetComponent<Rigidbody>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody == m_boatRigidbody)
        {
            m_isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody == m_boatRigidbody)
        {
            m_isPlayerInRange = false;
        }
    }

    private void Update()
    {
        CheckDockConditions();
        UpdateUI();
        UpdateParkingZone();

        if (m_isDocked && m_interactAction.action.WasPressedThisFrame())
        {
            OpenDockUI();
        }
    }

    //check if boat is close enough and slow enough and correctly parked (if it requires correct parking)
    private void CheckDockConditions()
    {
        if (m_boatRigidbody == null)
        {
            m_boatRigidbody = m_boatTransform.GetComponent<Rigidbody>();
        }

        float _speed = m_boatRigidbody.linearVelocity.magnitude;
        bool _isSlowEnough = _speed <= m_maxBoatSpeed;
        bool _isCorrectlyParked = IsBoatCorrectlyParked();

        m_isDocked = m_isPlayerInRange && _isSlowEnough && _isCorrectlyParked;
    }

    //park in direction of the parking slot or 180° rotated
    private bool IsBoatCorrectlyParked()
    {
        if (!m_requireCorrectParking)
        {
            return true;
        }

        if (m_boatTransform == null)
        {
            return false;
        }

        float _angle = Vector3.SignedAngle(transform.forward, m_boatTransform.forward, Vector3.up);
        _angle = Mathf.Abs(_angle);

        bool _isForwardCorrect = _angle <= m_allowedAngleTolerance;
        bool _isBackwardCorrect = Mathf.Abs(_angle - 180f) <= m_allowedAngleTolerance;

        return _isForwardCorrect || _isBackwardCorrect;
    }

    private void UpdateParkingZone()
    {
        if (m_parkingZone == null)
        {
            return;
        }

        float _dist = Vector3.Distance(m_boatTransform.position, transform.position);
        bool _visible = _dist <= m_zoneVisibilityDistance;

        m_parkingZone.SetVisible(_visible);

        if (!_visible)
        {
            return;
        }

        m_parkingZone.SetCorrectParking(m_isDocked);
    }

    private void UpdateUI()
    {
        if (m_interactionIcon == null)
        {
            return;
        }

        //check that toggle is only set when state really changed
        if (m_lastInteractableState != m_isDocked)
        {
            m_lastInteractableState = m_isDocked;
            m_interactionIcon.SetActive(m_isDocked);
        }
        
        if (!m_isDocked)
        {
            return;
        }

        //set icon above dock
        Camera _cam = Camera.main;
        if (_cam == null)
        {
            return;
        }

        Vector3 _worldPos = transform.position + Vector3.up * 2f;
        Vector3 _screenPos = _cam.WorldToScreenPoint(_worldPos);

        //only set if in front of camera
        if (_screenPos.z > 0)
        {
            m_interactionIcon.transform.position = _screenPos;
        }
    }

    private void OpenDockUI()
    {
        GameManager.Instance.SetState(GameStates.DIALOGMODE);
        m_dockUIPanel.SetActive(true);
        m_dockUIController.Init(this);
    }

    public void CloseDockUI()
    {
        m_dockUIPanel.SetActive(false);
        GameManager.Instance.SetState(GameStates.MOTORMODE);
    }


    //---------------
    //--- GIZMOS ---
    //---------------
    private void OnDrawGizmos()
    {
        if (!m_showGizmos)
        {
            return;
        }

        //trigger box
        Gizmos.color = m_isDocked ? Color.darkGreen : Color.darkOrange;

        BoxCollider _col = GetComponent<BoxCollider>();
        if (_col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_col.center, _col.size);
        }

        //parking direction visualization
        if (!m_requireCorrectParking)
        {
            return;
        }

        Gizmos.matrix = Matrix4x4.identity;

        Vector3 _origin = transform.position + Vector3.up * 0.25f;
        float _lineLength = 2f;

        //forward direction
        Vector3 _forward = transform.forward * _lineLength;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_origin, _origin + _forward);

        //forward arrow head
        Vector3 _arrowRight = Quaternion.Euler(0f, 150f, 0f) * transform.forward * 0.4f;
        Vector3 _arrowLeft = Quaternion.Euler(0f, -150f, 0f) * transform.forward * 0.4f;

        Gizmos.DrawLine(_origin + _forward, _origin + _forward + _arrowRight);
        Gizmos.DrawLine(_origin + _forward, _origin + _forward + _arrowLeft);

        //backward direction (valid too)
        Vector3 _backward = -transform.forward * _lineLength;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_origin, _origin + _backward);

        //tolerance visualization
        Gizmos.color = Color.yellow;

        Vector3 _forwardLeft = Quaternion.Euler(0f, -m_allowedAngleTolerance, 0f) * transform.forward * _lineLength;
        Vector3 _forwardRight = Quaternion.Euler(0f, m_allowedAngleTolerance, 0f) * transform.forward * _lineLength;
        Vector3 _backwardLeft = Quaternion.Euler(0f, 180f - m_allowedAngleTolerance, 0f) * transform.forward * _lineLength;
        Vector3 _backwardRight = Quaternion.Euler(0f, 180f + m_allowedAngleTolerance, 0f) * transform.forward * _lineLength;

        Gizmos.DrawLine(_origin, _origin + _forwardLeft);
        Gizmos.DrawLine(_origin, _origin + _forwardRight);

        Gizmos.DrawLine(_origin, _origin + _backwardLeft);
        Gizmos.DrawLine(_origin, _origin + _backwardRight);
    }
}
