using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    private float m_maxKnotSpeed = 7.9f;
    private float m_msPerKnot = 0.514444f; // 0.51444m/s = 1 Knot
    private float m_maximumSpeed = 0.0f;
    private float m_acceleration = 100.0f; // TODO based on Schub --> motormode!
    private float m_turnSpeed = 15.0f; // TODO fix turn speed value with proper boat controls
    private Rigidbody m_rigidbody = null;

    private float m_airDensity = 1.2f; // kg/m^3
    private float m_waterDensity = 1000.0f; // kg/m^3
    private Vector3 m_apparentWind = Vector3.zero;
    private float m_mainSailSize = 52.5f; // m^2
    private float m_sideSailSize = 41.0f; // m^2
    private float m_boatSideSize = 0.0f; // TODO
    private float m_keelSize = 0.0f; // TODO
    private float m_rudderSize = 0.0f; // TODO
    private float m_boatWeight = 0.0f; // TODO
    private float m_maxSailAngle = 0.0f; // TODO
    private float m_motorBoost = 0.0f; // TODO
    private float m_fuel = 0.0f; // TODO

    // TODO Windvektor und Strömungsvektor einlesen
    // TODO Bootgesundheit + Damage nehmen

    private Vector2 m_wasdInput = Vector2.zero;

    [SerializeField] private InputActionReference m_simpleWASDAction = null;

    private Vector3 CalculateApprentWind()
    {
        Vector3 airStream = -transform.forward;
        // TODO get wind vector from wind script
        //m_apparentWind = wahrerWind + airStream;
        return m_apparentWind;
    }

    private Vector3 CalculateSailForce()
    {
        // TODO implement
        return Vector3.zero;
    }

    private Vector3 CalculateKeelForce()
    {
        // TODO implement
        return Vector3.zero;
    }

    private Vector3 CalculateRudderForce()
    {
        // TODO implement
        return Vector3.zero;
    }

    private Vector3 CalculateWindOnHull()
    {
        // TODO implement
        return Vector3.zero;
    }

    private Vector3 CalculateMotorForce()
    {
        // TODO implement
        Vector3 motorForce = transform.forward * m_motorBoost;
        return motorForce;
    }

    // TODO Waterforces
    // TODO Massenträgheit

    public void OnEnable()
    {
        // TODO Controls for sailing and motor mode
        if (m_simpleWASDAction != null)
        {
            m_simpleWASDAction.action.Enable();
        }
    }

    public void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        // rigidbody setup
        if (m_rigidbody == null)
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            m_rigidbody.angularDamping = 2.0f; // TODO balance damping values
            m_rigidbody.linearDamping = 2.0f;
        }

        // 7.9 knots in m/s --> 4.06m/s
        m_maximumSpeed = m_maxKnotSpeed * m_msPerKnot;
    }

    public void Update()
    {
        // TODO motor mode and sailmode
        // TODO set/enable controls based on game state
        if (m_simpleWASDAction != null)
        {
            m_wasdInput = m_simpleWASDAction.action.ReadValue<Vector2>();
        }
    }

    public void FixedUpdate()
    {
        // TODO motormode and sailmode

        // Calculate forward force
        // m_wasdInput.y = 1 --> forward
        // m_wasdInput.y = -1 --> backwards
        m_rigidbody.AddForce(transform.forward * m_wasdInput.y * m_acceleration); // TODO use wind force instead of input and acceleration (in sailing mode!)

        // Limit speed to max speed
        // linearVelocity.magnitude = length of velocity vector = current speed of rigidbody
        if (m_rigidbody.linearVelocity.magnitude > m_maximumSpeed)
        {
            m_rigidbody.linearVelocity = m_rigidbody.linearVelocity.normalized * m_maximumSpeed;
        }

        // Calculate rotation
        // m_wasdInput.x = -1 --> left
        // m_wasdInput.x = 1 --> right
        float rotationValue = m_wasdInput.x * m_turnSpeed * Time.fixedDeltaTime;
        m_rigidbody.MoveRotation(m_rigidbody.rotation * Quaternion.Euler(0.0f, rotationValue, 0.0f));

        // DEBUG //////////////////////////////////////////////////////////////////////////////
        float speedKnot = m_rigidbody.linearVelocity.magnitude / m_msPerKnot;
        Debug.Log(speedKnot + " knots");
        // DEBUG //////////////////////////////////////////////////////////////////////////////
    }
}
