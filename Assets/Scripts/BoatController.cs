using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    private float m_maxKnotSpeed = 7.9f;
    private float m_msPerKnot = 0.514444f; // 0.51444m/s = 1 Knot
    private float m_maximumSpeed = 0.0f;
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

    // Motormode
    private int m_thrustStep = 0;
    private int m_maxThrustForwardSteps = 3; // TODO Motormode-Werte balancen, wenn mehr Kräfte enthalten
    private int m_maxThrustBackwardSteps = -3;
    private float m_forwardForcePerStep = 25000f;
    private float m_backwardForcePerStep = 10000f;
    private float m_motorSteeringInput = 0.0f;
    private float m_motorBrakeModifier = 1.5f;
    private float m_fuel = 0.0f; // TODO

    // TODO Windvektor und Strömungsvektor einlesen
    // TODO Bootgesundheit + Damage nehmen

    // INPUT-ACTIONS
    [SerializeField] private GameManager m_gameManager = null;
    [SerializeField] private InputActionReference m_motorThrustForwardAction = null;
    [SerializeField] private InputActionReference m_motorThrustBackwardAction = null;
    [SerializeField] private InputActionReference m_motorThrustNeutralAction = null;
    [SerializeField] private InputActionReference m_motorSteeringAction = null;

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
        Vector3 motorForce = Vector3.zero;

        float speed = Vector3.Dot(m_rigidbody.linearVelocity, transform.forward); // project velocity on forward vector

        if(m_thrustStep > 0) // boat driving forward
        {
            motorForce = transform.forward * m_thrustStep * m_forwardForcePerStep; 
        }
        else if(m_thrustStep < 0) // boat driving backward
        {
            if (speed > 0.1f) // braking needed
            {
                // braking needs force in the opposite direction to the travel direction
                motorForce = transform.forward * m_thrustStep * m_forwardForcePerStep * m_motorBrakeModifier; 
            }
            else
            {
                // driving backwards
                motorForce = transform.forward * m_thrustStep * m_backwardForcePerStep;
            }
        }
        Debug.Log(m_thrustStep); // TODO Debug Logs raus
        return motorForce;
    }

    private void CalculateMotorSteering()
    {
        // ignore very small inputs
        if(Mathf.Abs(m_motorSteeringInput) < 0.2f)
        {
            return;
        }

        // current speed influences steering: 0 = no speed, 1 = maximum speed
        float speedInfluence = Mathf.Clamp01(m_rigidbody.linearVelocity.magnitude / m_maximumSpeed);
        
        // minimum turn factor 0.3 --> boat can rotate when standing still
        float turn = Mathf.Max(speedInfluence, 0.3f);

        // calculate finale rotation
        // turn speed modifier 4.0
        float rotation = m_motorSteeringInput * 4.0f * turn * Time.fixedDeltaTime;

        // rotate boat
        m_rigidbody.MoveRotation(m_rigidbody.rotation * Quaternion.Euler(0.0f, rotation, 0.0f));
    }

    // TODO Waterforces
    // TODO Massenträgheit

    public void OnEnable()
    {
        if (m_motorThrustForwardAction != null)
        {
            m_motorThrustForwardAction.action.Enable();
        }

        if(m_motorThrustBackwardAction != null)
        {
            m_motorThrustBackwardAction.action.Enable();
        }

        if(m_motorThrustNeutralAction != null) 
        { 
            m_motorThrustNeutralAction.action.Enable();
        }

        if(m_motorSteeringAction != null)
        {
            m_motorSteeringAction.action.Enable();
        }
    }

    public void OnDisable()
    {
        if (m_motorThrustForwardAction != null)
        {
            m_motorThrustForwardAction.action.Disable();
        }

        if (m_motorThrustBackwardAction != null)
        {
            m_motorThrustBackwardAction.action.Disable();
        }

        if (m_motorThrustNeutralAction != null)
        {
            m_motorThrustNeutralAction.action.Disable();
        }

        if (m_motorSteeringAction != null)
        {
            m_motorSteeringAction.action.Disable();
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
            m_rigidbody.mass = 9000.0f;
        }

        // 7.9 knots in m/s --> 4.06m/s
        m_maximumSpeed = m_maxKnotSpeed * m_msPerKnot;
    }

    public void Update()
    {
        // TODO motor mode and sailmode
        // TODO set/enable controls based on game state

        if(m_gameManager.CurrentGameState == GameStates.MOTORMODE) // TODO ggf. Modi aufteilen in Segel und 
        {
            // read input and increase or decrease thrust or set thrust to neutral position
            if (m_motorThrustForwardAction != null && m_motorThrustForwardAction.action.triggered == true)
            {
                m_thrustStep = Mathf.Min(m_thrustStep + 1, m_maxThrustForwardSteps);
            }

            if (m_motorThrustBackwardAction != null && m_motorThrustBackwardAction.action.triggered == true)
            {
                m_thrustStep = Mathf.Max(m_thrustStep - 1, m_maxThrustBackwardSteps);
            }

            if (m_motorThrustNeutralAction != null && m_motorThrustNeutralAction.action.triggered == true)
            {
                m_thrustStep = 0;
            }

            // steering via 1d axis
            if (m_motorSteeringAction != null)
            {
                m_motorSteeringInput = m_motorSteeringAction.action.ReadValue<float>();
            }
        }
    }

    public void FixedUpdate()
    {
        // TODO motormode and sailmode
        //// DEBUG //////////////////////////////////////////////////////////////////////////////
        float speedKnot = m_rigidbody.linearVelocity.magnitude / m_msPerKnot;
        Debug.Log(speedKnot + " knots");
        //// DEBUG //////////////////////////////////////////////////////////////////////////////

        if(m_gameManager.CurrentGameState == GameStates.MOTORMODE)
        {
            m_rigidbody.AddForce(CalculateMotorForce());
            CalculateMotorSteering();
        }

        if(m_gameManager.CurrentGameState == GameStates.SAILMODE)
        {
            // TODO Segelmodus
        }
    }
}
