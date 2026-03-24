using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    private float m_maxKnotSpeed = 7.9f;
    private float m_currentKnotSpeed = 0.0f;
    private float m_kmhPerKnot = 1.852f; // TODO m/s ?
    private float m_acceleration = 100.0f; // TODO based on Schub + balance value
    private float m_turnSpeed = 15.0f; // TODO balance and fix turn speed value with proper boat controls
    private Rigidbody m_rigidbody = null;

    private Vector2 m_wasdInput = Vector2.zero;

    [SerializeField] private InputActionReference m_simpleWASDAction = null;

    public void OnEnable()
    {
        if (m_simpleWASDAction != null)
        {
            m_simpleWASDAction.action.Enable();
        }
    }

    public void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        if (m_rigidbody == null)
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            m_rigidbody.angularDamping = 2.0f;
            m_rigidbody.linearDamping = 2.0f;
        }
    }

    public void Update()
    {
        // TODO set/enable controls based on game state
        if (m_simpleWASDAction != null)
        {
            m_wasdInput = m_simpleWASDAction.action.ReadValue<Vector2>();
            Debug.Log(m_wasdInput);
        }

        m_currentKnotSpeed = m_maxKnotSpeed * m_kmhPerKnot; // TODO depends on Schub --> calculate current knot speed based on Schub 
    }

    public void FixedUpdate()
    {
        m_rigidbody.AddForce(transform.forward * m_wasdInput.y * m_acceleration);

        float turn = m_wasdInput.x * m_turnSpeed * Time.fixedDeltaTime;
        m_rigidbody.MoveRotation(m_rigidbody.rotation * Quaternion.Euler(0.0f, turn, 0.0f));
    }
}
