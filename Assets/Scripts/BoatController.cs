using UnityEngine;
using UnityEngine.InputSystem;

public class BoatController : MonoBehaviour
{
    private float m_maxKnotSpeed = 7.9f;
    private float m_msPerKnot = 0.514444f; // 0.51444m/s = 1 Knot
    private float m_maximumSpeed = 0.0f;
    private Rigidbody m_rigidbody = null;

    // Force calculation
    private float m_airDensity = 1.2f; // kg/m^3
    private float m_waterDensity = 1000.0f; // kg/m^3
    private Vector3 m_apparentWind = Vector3.zero;
    private float m_boatSideSize = 0.0f; // TODO Bootcontroller 
    private float m_keelSize = 0.0f; // TODO Bootcontroller 
    private float m_rudderSize = 0.0f; // TODO Bootcontroller 
    private float m_boatWeight = 0.0f; // TODO Bootcontroller 

    // Sail
    private float m_totalSailSize = 93.0f; // m^2
    //private float m_mainSailSize = 52.5f; // m^2
    //private float m_sideSailSize = 41.0f; // m^2
    private float[] m_liftTable = new float[181];
    private float[] m_dragTable = new float[181];
    private float m_maxSailAngle = 0.0f; // TODO Segel trimmen

    // Motormode
    private int m_thrustStep = 0;
    private int m_maxThrustForwardSteps = 3; // TODO Bootcontroller Motormode-Werte balancen, wenn mehr Kräfte enthalten
    private int m_maxThrustBackwardSteps = -3;
    private float m_forwardForcePerStep = 25000f;
    private float m_backwardForcePerStep = 10000f;
    private float m_motorSteeringInput = 0.0f;
    private float m_motorBrakeModifier = 1.5f;
    private float m_fuel = 0.0f; // TODO Bootcontroller 
    private float m_health = 100.0f;

    public float Health { 
        get { return m_health; }
        set { m_health = value; }
    }

    // TODO Windvektor und Strömungsvektor einlesen

    // INPUT-ACTIONS
    [SerializeField] private GameManager m_gameManager = null;
    [SerializeField] private GameObject m_sail = null;
    [SerializeField] private InputActionReference m_motorThrustForwardAction = null;
    [SerializeField] private InputActionReference m_motorThrustBackwardAction = null;
    [SerializeField] private InputActionReference m_motorThrustNeutralAction = null;
    [SerializeField] private InputActionReference m_motorSteeringAction = null;

    private void CalculateLiftAndDragTables()
    {
        for (int angle = 0; angle <= 180; angle++)
        {
            float rad = angle * Mathf.Deg2Rad;

            // max at 30°-45°
            // > 45° less
            float lift = Mathf.Sin(2f * rad);
            lift = Mathf.Clamp01(lift);

            // max at 90°-180°
            // increases with every angle
            float drag = Mathf.Sin(rad);
            drag = drag * drag;

            m_liftTable[angle] = lift;
            m_dragTable[angle] = drag;
        }
    }

    private float GetLiftCoefficient(float angle)
    {
        int lookUpAngle = Mathf.Clamp((int)angle, 0, 180);
        return m_liftTable[lookUpAngle];
    }

    private float GetDragCoefficient(float angle)
    {
        int lookUpAngle = Mathf.Clamp((int)angle, 0, 180);
        return m_dragTable[lookUpAngle];
    }

    private Vector3 CalculateApprentWind()
    {
        Vector3 airStream = -transform.forward;
        // TODO get wind vector from wind script --> (1, 0, 0) as placeholder
        m_apparentWind = new Vector3(1.0f, 0.0f, 0.0f) + airStream;
        return m_apparentWind;
    }

    private Vector3 CalculateSailForce()
    {
        Vector3 apparentWind = CalculateApprentWind();

        // 0.5 * airdensity * (magnitude apparent wind)^2 * sail size * coefficient (drag or lift)
        float sailForceValue = 0.5f * m_airDensity * apparentWind.magnitude * apparentWind.magnitude * m_totalSailSize;

        // -apparentWind = where wind comes from
        // apparentWind = where wind goes
        float angleSailWind = Vector3.Angle(-apparentWind, m_sail.transform.forward);

        // wind from left or right side
        // sign = -1 if projection of rightvector and apparent wind is negative
        // sign = 1 if projection of right vector and apparent wind is positive
        float sign = Mathf.Sign(Vector3.Dot(m_sail.transform.right, apparentWind));

        // cross product --> vertical vector on sail (90°) --> right hand rule!!
        // sign moves lift to left or right side depending on wind direction
        Vector3 liftDirection = Vector3.Cross(apparentWind.normalized, Vector3.up).normalized * sign;

        // resulting sail force = lift + drag
        Vector3 lift = liftDirection * sailForceValue * GetLiftCoefficient(angleSailWind);
        Vector3 drag = apparentWind.normalized * sailForceValue * GetDragCoefficient(angleSailWind); // TODO drag ggf. skalieren

        return lift + drag;
    }

    private Vector3 CalculateKeelForce()
    {
        // TODO Bootcontroller implement
        return Vector3.zero;
    }

    private Vector3 CalculateRudderForce()
    {
        // TODO Bootcontroller implement
        return Vector3.zero;
    }

    private Vector3 CalculateWindOnHull()
    {
        // TODO Bootcontroller implement
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

    // TODO Bootcontroller Waterforces
    // TODO Bootcontroller Massenträgheit

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

        CalculateLiftAndDragTables();
    }

    public void Update()
    {
        // TODO Bootcontroller motor mode and sailmode
        // TODO Bootcontroller set/enable controls based on game state

        if(m_gameManager.CurrentGameState == GameStates.MOTORMODE)
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
        //// DEBUG //////////////////////////////////////////////////////////////////////////////
        float speedKnot = m_rigidbody.linearVelocity.magnitude / m_msPerKnot;
        Debug.Log(speedKnot + " knots");
        //// DEBUG //////////////////////////////////////////////////////////////////////////////

        if(m_gameManager.CurrentGameState == GameStates.MOTORMODE)
        {
            // TODO Bootcontroller: Motormodus: Kielkraft, Ruderkraft, Wind auf Hülle, Wasserkräfte, Massenträgheit zu Kraft hinzufügen
            m_rigidbody.AddForce(CalculateMotorForce());
            CalculateMotorSteering();
        }

        if(m_gameManager.CurrentGameState == GameStates.SAILMODE)
        {
            Vector3 totalForce = Vector3.zero;
            totalForce += CalculateSailForce();
            totalForce += CalculateKeelForce();
            totalForce += CalculateWindOnHull();
            totalForce += CalculateRudderForce(); // TODO Bootcontroller: ggf. nur auf Ruder anwenden --> AddForceAtPosition() oder AddTorque

            m_rigidbody.AddForce(totalForce);
            // TODO Damage durch zu hohe Wellen
        }
    }
}
