using UnityEngine;
using UnityEngine.InputSystem;
using WwiseEvent = AK.Wwise.Event;
using WwiseRTPC = AK.Wwise.RTPC;

public class BoatController : MonoBehaviour
{
    private float m_msPerKnot = 0.514444f; // 0.51444m/s = 1 Knot
    private Rigidbody m_rigidbody = null;
    private bool m_isBoatDrivingForward = true; // shader input
    private bool m_isInSailMode = true;

    // FORCE CALCULATION FIELDS
    private float m_airDensity = 1.2f; // kg/m^3
    private float m_waterDensity = 1000.0f; // kg/m^3
    private Vector3 m_apparentWind = Vector3.zero;
    private float m_boatSideSize = 17.0f; // m^2
    private float m_boatFrontSize = 4.0f; // m^2
    private float m_keelSize = 3.0f; // m^2
    private float m_rudderSize = 2.0f; // m^2
    private float m_maxRudderAngle = 30.0f; // max 30°

    // SAILMODE FIELDS
    private float m_mainSailSize = 52.5f; // m^2
    private float m_frontSailSize = 41.0f; // m^2
    private float[] m_liftTable = new float[181];
    private float[] m_dragTable = new float[181];
    private float m_maxSailAngle = 90.0f;
    private float m_currentMainSailAngle = 0.0f;
    private float m_currentFrontSailAngle = 0.0f;
    private float m_trimInput = 0.0f;
    private float m_trimSpeed = 30.0f;
    private Sails m_currentSail = Sails.BOTHSAILS;
    private float m_actualMainSailRotation = 0.0f; // actual rendered rotation in game
    private float m_actualFrontSailRotation = 0.0f;
    private float m_mainSailVelocity = 0.0f; // rotation speed of changing sail angle --> needed for smooth damp; shows how much the sail rotation/sail angle is currently changing
    private float m_frontSailVelocity = 0.0f;
    private float m_smoothedWindAngle = 0.0f;
    private bool m_isInButterfly = false;
    private float m_lastMainWindSideSign = 1.0f; // save last side sign of main sail --> avoid jitters, stabilize butterfly mode

    // MOTORMODE FIELDS
    private float m_thrustStep = 0.0f;
    private float m_maxThrustForwardSteps = 3.0f;
    private float m_maxThrustBackwardSteps = -3.0f;
    private float m_forwardForcePerStep = 700.0f;
    private float m_backwardForcePerStep = 300.0f;
    private float m_steeringInput = 0.0f;
    private float m_smoothedSteeringInput = 0.0f;
    private float m_motorBrakeModifier = 1.5f;
    private float m_currentFuel = 0.0f; 
    [SerializeField] private float m_maxFuel = 250.0f; // TODO SerializedField raus
    private float m_currentHealth = 0.0f;
    private float m_maxHealth = 100.0f;

    // TODO Windvektor und ggf. Strömungsvektor einlesen

    // REFERENCES
    [SerializeField] private GameManager m_gameManager = null;
    [SerializeField] private WindController windController = null;
    [SerializeField] private Transform m_parentReference = null;
    [SerializeField] private GameObject m_mainSail = null;
    [SerializeField] private GameObject m_frontSail = null;
    [SerializeField] private GameObject m_rudder = null;

    // INPUT ACTION REFERENCES
    [SerializeField] private InputActionReference m_steeringAction = null;
    [SerializeField] private InputActionReference m_rudderNeutralAction = null;
    [SerializeField] private InputActionReference m_changeMotorSailModeAction = null;
    [SerializeField] private InputActionReference m_chooseMainSailAction = null;
    [SerializeField] private InputActionReference m_chooseFrontSailAction = null;
    [SerializeField] private InputActionReference m_chooseBothSailsAction = null;
    [SerializeField] private InputActionReference m_trimAction = null;
    [SerializeField] private InputActionReference m_motorThrustForwardAction = null;
    [SerializeField] private InputActionReference m_motorThrustBackwardAction = null;
    [SerializeField] private InputActionReference m_motorThrustNeutralAction = null;

    // AUDIO REFERENCES
    [SerializeField] private GameObject m_boatAudioEmitter;
    [SerializeField] private WwiseEvent m_engineStartEvent = null;
    [SerializeField] private WwiseEvent m_engineStopEvent = null;
    [SerializeField] private WwiseEvent m_throttleMoveEvent = null;
    [SerializeField] private WwiseEvent m_throttleMoveIdleEvent = null;
    [SerializeField] private WwiseRTPC m_boatThrottleSignedRTPC = null;

    // PROPERTIES
    public float BoatSpeedInKnots
    {
        get { return m_rigidbody.linearVelocity.magnitude / m_msPerKnot; }
    }

    public float CurrentHealth
    {
        get { return m_currentHealth; }
        set { m_currentHealth = value; } // TODO setter ggf. raus?
    }

    public float MaxHealth
    {
        get { return m_maxHealth; }
    }

    public float CurrentFuel
    {
        get { return m_currentFuel; }
    }

    public float MaxFuel
    {
        get { return m_maxFuel; }
    }

    public bool IsBoatDrivingForward
    {
        get { return m_isBoatDrivingForward; }
    }

    public bool IsInButterfly
    {
        get { return m_isInButterfly; }
    }

    public bool IsInSailMode
    {
        get { return m_isInSailMode; }
    }

    // PRIVATE METHODS ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void CalculateLiftAndDragTables()
    {
        m_liftTable = new float[] { -0.05f, 0.020883078f, 0.092470145f, 0.164057211f, 0.235644278f, 0.307231345f, 0.378818412f, 0.450405478f, 0.504490273f, 0.565930411f, 0.627370548f, 0.688810686f, 0.750250824f, 0.805642556f, 0.853819935f, 0.901997314f, 0.950174693f, 0.998352072f, 1.046529451f, 1.088761889f, 1.130677276f, 1.165245295f, 1.197876052f, 1.228547608f, 1.256131425f, 1.283715242f, 1.294746187f, 1.302365856f, 1.302408885f, 1.300696517f, 1.295771298f, 1.290846079f, 1.2818945f, 1.272631861f, 1.258578528f, 1.242851952f, 1.223503499f, 1.209775299f, 1.196047099f, 1.188616161f, 1.182037515f, 1.175458869f, 1.168480788f, 1.160250041f, 1.15218895f, 1.146909313f, 1.141629676f, 1.13635004f, 1.126385357f, 1.114030145f, 1.101674933f, 1.08931972f, 1.076964508f, 1.064609296f, 1.052254083f, 1.038540735f, 1.02255314f, 1.006565545f, 0.99057795f, 0.974241725f, 0.956802971f, 0.939364217f, 0.921925463f, 0.904322536f, 0.885322141f, 0.866321745f, 0.84732135f, 0.824191214f, 0.800805787f, 0.777420361f, 0.754034935f, 0.730243921f, 0.705564708f, 0.680885495f, 0.656206282f, 0.631527069f, 0.606847856f, 0.582168643f, 0.555080611f, 0.525492436f, 0.495904261f, 0.466316086f, 0.436100994f, 0.405758636f, 0.375416278f, 0.344186659f, 0.312498236f, 0.280809813f, 0.249333033f, 0.218519452f, 0.187705871f, 0.15526957f, 0.119279676f, 0.083289781f, 0.046613879f, 0.005617003f, -0.024460955f, -0.049140168f, -0.069869439f, -0.09044886f, -0.110993543f, -0.130752022f, -0.150510501f, -0.166515903f, -0.180106845f, -0.193697787f, -0.207288729f, -0.220879672f, -0.234162472f, -0.247341586f, -0.260520701f, -0.273534762f, -0.285117266f, -0.29669977f, -0.308282274f, -0.319864779f, -0.331447283f, -0.343029787f, -0.354612291f, -0.365995976f, -0.376977284f, -0.387958593f, -0.398939901f, -0.40992121f, -0.420902518f, -0.431883826f, -0.44278009f, -0.453651446f, -0.464522803f, -0.475394159f, -0.486265516f, -0.497136872f, -0.506878711f, -0.516141349f, -0.525403987f, -0.534666625f, -0.543929263f, -0.553191902f, -0.56245454f, -0.571717178f, -0.580979816f, -0.590242454f, -0.598481708f, -0.606712456f, -0.614943203f, -0.623344553f, -0.63206096f, -0.640777368f, -0.649493775f, -0.658210182f, -0.66692659f, -0.675642997f, -0.684359404f, -0.693075812f, -0.701792219f, -0.710508626f, -0.719225034f, -0.727941441f, -0.745650528f, -0.763616722f, -0.784783239f, -0.813554069f, -0.873442789f, -0.946688773f, -0.994284252f, -1.004872848f, -1.004280002f, -0.988633743f, -0.959963118f, -0.917563513f, -0.867171527f, -0.806180985f, -0.737089336f, -0.661059537f, -0.581294014f, -0.492326981f, -0.396603617f, -0.308954542f, -0.225154278f, -0.123170151f, -0.021186024f };
        
        m_dragTable = new float[] { 0.38686385f, 0.38686385f, 0.38686385f, 0.38686385f, 0.389313944f, 0.392970084f, 0.39627427f, 0.400844893f, 0.406525589f, 0.413448441f, 0.423087326f, 0.43436206f, 0.446969015f, 0.461843124f, 0.477379126f, 0.494470073f, 0.511844278f, 0.530824867f, 0.551880783f, 0.573473466f, 0.595613179f, 0.618209053f, 0.640621824f, 0.662929695f, 0.689879447f, 0.717907852f, 0.742850651f, 0.768920302f, 0.795362681f, 0.823316744f, 0.850209803f, 0.875819503f, 0.902869248f, 0.929952637f, 0.956062275f, 0.982152684f, 1.00783812f, 1.032219846f, 1.056128497f, 1.079807038f, 1.103792918f, 1.126769184f, 1.148463259f, 1.170392417f, 1.191483401f, 1.211927291f, 1.232460607f, 1.251496506f, 1.26986412f, 1.287926532f, 1.305267082f, 1.321581271f, 1.336975061f, 1.352433199f, 1.367474706f, 1.381879313f, 1.395370904f, 1.408437175f, 1.42077202f, 1.432323005f, 1.442932234f, 1.453247113f, 1.463107136f, 1.472244833f, 1.481002418f, 1.48898399f, 1.496326908f, 1.503874636f, 1.510469672f, 1.516160814f, 1.520762198f, 1.524862379f, 1.528414588f, 1.531505316f, 1.534396653f, 1.536929303f, 1.539348753f, 1.541768202f, 1.543557613f, 1.54546413f, 1.547581813f, 1.549468654f, 1.548920021f, 1.54801459f, 1.546543847f, 1.544999096f, 1.543531685f, 1.54202705f, 1.540196571f, 1.538897543f, 1.537466191f, 1.535683999f, 1.53355819f, 1.531394771f, 1.529080687f, 1.52636414f, 1.523296284f, 1.519923682f, 1.516217375f, 1.511846219f, 1.506802417f, 1.501377281f, 1.495823772f, 1.489852422f, 1.483303796f, 1.476166382f, 1.468601931f, 1.46103748f, 1.453121688f, 1.444432961f, 1.435363018f, 1.426135009f, 1.416626582f, 1.4060435f, 1.394535499f, 1.382541884f, 1.370241135f, 1.35748521f, 1.344036318f, 1.329988939f, 1.315536648f, 1.299903026f, 1.283327307f, 1.266037303f, 1.248179244f, 1.229432006f, 1.20930074f, 1.189087279f, 1.166532089f, 1.143668855f, 1.120641873f, 1.095927105f, 1.068977914f, 1.041870702f, 1.012157234f, 0.981880102f, 0.952218843f, 0.92072355f, 0.886264821f, 0.851341686f, 0.81609477f, 0.780375363f, 0.743510309f, 0.705298108f, 0.667971289f, 0.632315819f, 0.598288727f, 0.56281104f, 0.530969521f, 0.499784719f, 0.472547329f, 0.448385954f, 0.425068473f, 0.405095466f, 0.389551534f, 0.377744637f, 0.368761857f, 0.362676155f, 0.358217857f, 0.353394115f, 0.351457994f, 0.353654607f, 0.355851219f, 0.358047832f, 0.360244444f, 0.362441057f, 0.364637669f, 0.366834282f, 0.369030894f, 0.371227506f, 0.373424119f, 0.375620731f, 0.377817344f, 0.380013956f, 0.382210569f, 0.384407181f, 0.386603794f, 0.388800406f, 0.390997019f, 0.393193631f, 0.395390244f };
    }

    private float GetCoefficient(float[] coefficientTable, float signedAngle)
    {
        // Angle stays between 0 and 360
        float angleNormalized = Mathf.Repeat(signedAngle, 360f);

        // Fold angle to 0-180 --> index for table
        float angleFoldedToHalf = 0.0f;

        if (angleNormalized > 180.0f)
        {
            // angle > 180 --> example 200° --> 360-200 = 160 --> index for table
            angleFoldedToHalf = 360.0f - angleNormalized;
        }
        else
        {
            angleFoldedToHalf = angleNormalized;
        }

        // Round float angle to int --> index for table
        int roundedAngle = Mathf.FloorToInt(angleFoldedToHalf);

        // Next index after angle = index --> max 180
        int nextIndexInTable = Mathf.Clamp(roundedAngle + 1, 0, 180);

        // decimals after comma --> distance of decimals between actual angle and rounded angle
        float decimals = angleFoldedToHalf - roundedAngle;

        // Interpolate table result from rounded angle and next index --> avoid jumps
        // decimals decides the portion of the table results of both indexes
        float lerpedIndex = Mathf.Lerp(coefficientTable[roundedAngle], coefficientTable[nextIndexInTable], decimals);

        return lerpedIndex;
    }

    private Vector3 CalculateApprentWind()
    {
        // TODO get wind vector from wind script --> placeholder vector
        m_apparentWind = new Vector3(0.0f, 0.0f, 10.0f) - m_rigidbody.linearVelocity;
        return m_apparentWind;
    }

    private Vector3 CalculateSailForce(GameObject sail, float sailSize)
    {
        Vector3 apparentWind = CalculateApprentWind();
        float apparentWindSpeed = apparentWind.magnitude * apparentWind.magnitude;

        // No sail force if there is barely any wind
        if (apparentWindSpeed < 0.01f)
        {
            return Vector3.zero;
        }

        // apparentWind.normalized = where wind goes
        // -apparentWind.normalized = where wind comes from

        float angleWindSail = Vector3.Angle(sail.transform.forward, -apparentWind.normalized);
        Debug.Log("Angle Wind Sail" + Mathf.RoundToInt(angleWindSail));

        // Dead zone depends on angle of wind and boat
        float angleWindBoat = Vector3.Angle(transform.forward, -apparentWind.normalized);
        Debug.Log("Angle Wind Boat" + Mathf.RoundToInt(angleWindBoat));

        // Angle between wind origin and sail too low --> sail can't catch wind properly, sail flutters --> no lift, drag stays
        if (angleWindSail < 5.0f || angleWindSail > 175.0f)
        {
            return Vector3.zero;
        }

        bool isFluttering = false;

        // dead zone 
        if (angleWindBoat < 15.0f)
        {
            isFluttering = true;
        }
        else
        {
            isFluttering = false;
        }

        // 0.5 * airdensity * (magnitude apparent wind)^2 * sail size * coefficient (drag or lift)
        float sailForceValue = 0.5f * m_airDensity * apparentWindSpeed * sailSize;

        // lift acts in 90° to wind direction
        Vector3 liftDirection = Vector3.Cross(Vector3.up, apparentWind.normalized).normalized;

        // Check which side the wind is hitting the sail
        float sideSign = Mathf.Sign(Vector3.Dot(-apparentWind.normalized, sail.transform.right));

        float liftCoefficient = 0.0f;
        float dragCoefficient = 0.0f;

        if (isFluttering == true)
        {
            Debug.Log("Fluttering");
            return Vector3.zero;
        }
        else
        {
            liftCoefficient = GetCoefficient(m_liftTable, angleWindSail);
            dragCoefficient = GetCoefficient(m_dragTable, angleWindSail);
        }

        // liftforce direction orthogonal to wind
        Vector3 liftForce = liftDirection * sideSign * sailForceValue * liftCoefficient;

        // dragforce in wind direction
        Vector3 dragForce = apparentWind.normalized * sailForceValue * dragCoefficient;

        // Heeling = Krängung
        float rollAngle = transform.localEulerAngles.z;

        // clamp roll angle to -180° - 180°
        if (rollAngle > 180.0f)
        {
            rollAngle -= 360.0f;
        }

        float absoluteRollAngle = Mathf.Abs(rollAngle);
        float heelFactor = 1.0f; // modifier if needed

        // if boot rolls more than 15° sail start to become inefficient --> linear downgrade
        if (absoluteRollAngle > 15.0f)
        {
            heelFactor = Mathf.Clamp01(1.0f - ((absoluteRollAngle - 15.0f) / 6.0f));
        }

        return (liftForce + dragForce) * 1.8f * heelFactor;
    }

    private void CalculateTotalSailForce()
    {
        Vector3 mainSailForce = CalculateSailForce(m_mainSail, m_mainSailSize);
        Vector3 frontSailForce = CalculateSailForce(m_frontSail, m_frontSailSize);
        Vector3 totalSailForce = mainSailForce + frontSailForce;
        totalSailForce.y = 0.0f;

        // apply lift force
        m_rigidbody.AddForce(transform.forward * Vector3.Dot(totalSailForce, transform.forward), ForceMode.Force);

        float angleToWind = Vector3.Angle(transform.forward, -CalculateApprentWind().normalized);

        // Heel Factor = Krängung
        float heelIntensity = 0.0f;
        if (angleToWind >= 80f && angleToWind <= 100f)
        {
            // Halbwind
            heelIntensity = 1.4f;
        }
        else if (angleToWind < 80f)
        {
            // Am Wind
            heelIntensity = 1.0f;
        }
        else if (angleToWind > 100f && angleToWind < 150f)
        {
            // Raumschot
            heelIntensity = 0.6f;
        }
        else
        {
            // Vor dem Wind
            heelIntensity = 0.2f;
        }

        Vector3 lateralSailForce = transform.right * Vector3.Dot(totalSailForce, transform.right) * heelIntensity; // side force
        Vector3 sailLeverPoint = transform.position + (transform.up * 2.0f); // apply force 2m above boat
        m_rigidbody.AddForceAtPosition(lateralSailForce, sailLeverPoint, ForceMode.Force); // apply heeling
    }

    private void SailTrimming()
    {
        if (m_gameManager.CurrentState == GameStates.SAILMODE)
        {
            float ropeInput = m_trimInput * m_trimSpeed * Time.fixedDeltaTime;

            // trim chosen sail
            if (m_currentSail == Sails.MAINSAIL || m_currentSail == Sails.BOTHSAILS)
            {
                m_currentMainSailAngle = Mathf.Clamp(m_currentMainSailAngle + ropeInput, 0.0f, m_maxSailAngle);
            }

            if (m_currentSail == Sails.FRONTSAIL || m_currentSail == Sails.BOTHSAILS)
            {
                m_currentFrontSailAngle = Mathf.Clamp(m_currentFrontSailAngle + ropeInput, 0.0f, m_maxSailAngle);
            }

            Debug.Log("Current Main Sail Angle " + m_currentMainSailAngle + " Current Front Sail Angle " + m_currentFrontSailAngle);


            Vector3 apparentWind = CalculateApprentWind();

            // do nothing if there's almost no apparent wind speed
            if (apparentWind.sqrMagnitude < 0.01f)
            {
                return;
            }

            // transfer wind dir in boat's local coordinate system
            Vector3 localWindDir = transform.InverseTransformDirection(apparentWind.normalized);

            // determine angle between boat's right vector and local wind dir
            float rawWindAngle = Vector3.SignedAngle(Vector3.right, localWindDir, Vector3.up);

            // smooth wind angle --> avoid angle jumps --> avoid sail jumps
            m_smoothedWindAngle = Mathf.Lerp(m_smoothedWindAngle, rawWindAngle, 0.08f);

            float absoluteWindAngle = Mathf.Abs(m_smoothedWindAngle);

            // angle for vorwind-kurs --> angle between forward vector and apparentwind
            float forwardWindAngle = Vector3.Angle(transform.forward, -apparentWind.normalized);

            // butterfly mode within 160-165° + avoid jitters
            if(forwardWindAngle > 165.0f && m_isInButterfly == false)
            {
                m_isInButterfly = true;
            }
            else if (m_isInButterfly == true && forwardWindAngle < 160.0f)
            {
                m_isInButterfly = false;
            }

            float stabilityFactor = 1.0f;

            // determine if sail is somewhere + -90° --> 90° = dangerous-- > sails jump around
            if (Mathf.Abs(absoluteWindAngle - 90.0f) < 6.0f)
            {
                // if sail is close to 90° (84° - 96°)
                float distanceToNinetyDegrees = Mathf.Abs(absoluteWindAngle - 90.0f) / 6.0f;

                // determine a value between 0.7 and 1.0 --> if 90° then stabilityFactor = 0.7 --> avoid harsh movement of sails --> avoid sail jumps from left to right
                stabilityFactor = Mathf.Lerp(0.7f, 1.0f, distanceToNinetyDegrees);
            }

            // reduce wind angle if it's too close to 90°
            absoluteWindAngle *= stabilityFactor;

            // determine if wind comes from left or right side
            float windSide = Vector3.Dot(apparentWind.normalized, transform.right);

            float mainWindSideSign = 0.0f;

            // avoid jitters, stabilize butterfly mode --> avoid swapping sides too fast
            if (m_isInButterfly == true)
            {
                // 0.3f and -0.3f as threshold for sign swap --> boat needs to turn more than 0.3 to swap side signs from plus to minus
                if (windSide > 0.3f)
                {
                    // windside > 0.3f --> wind pushing from right side
                    // save last main wind side sign
                    m_lastMainWindSideSign = 1.0f;
                }
                else if (windSide < -0.3f)
                {
                    // windside < -0.3f --> wind pushing from left side
                    m_lastMainWindSideSign = -1.0f;
                }

                mainWindSideSign = m_lastMainWindSideSign;
            }
            else
            {
                mainWindSideSign = GetWindSideSign(windSide, m_currentMainSailAngle);
                m_lastMainWindSideSign = mainWindSideSign;
            }

            float frontWindSideSign = 0.0f;
            
            if(m_isInButterfly == true)
            {
                // use opposite side sign of main sail if boat is in vorwind-kurs --> butterfly mode allowed
                frontWindSideSign = -mainWindSideSign;
            }
            else
            {
                frontWindSideSign = GetWindSideSign(windSide, m_currentFrontSailAngle);
            }

            // clamp sail angle to max current trimmed angle
            float mainSailTargetAngle = Mathf.Clamp(absoluteWindAngle, 0.0f, m_currentMainSailAngle);
            float frontSailTargetAngle = Mathf.Clamp(absoluteWindAngle, 0.0f, m_currentFrontSailAngle);

            // mirror angle to correct side
            mainSailTargetAngle *= -mainWindSideSign;
            frontSailTargetAngle *= -frontWindSideSign;

            // smooth sail movement
            // rendered angle, target angle, rotation speed, smoothing value/"reaction time" of movement
            // ref = unity changes and saves the velocity in the field
            m_actualMainSailRotation = Mathf.SmoothDamp(m_actualMainSailRotation, mainSailTargetAngle, ref m_mainSailVelocity, 0.18f);
            m_actualFrontSailRotation = Mathf.SmoothDamp(m_actualFrontSailRotation, frontSailTargetAngle, ref m_frontSailVelocity, 0.18f);

            // apply sail rotation to y-axis
            m_mainSail.transform.localRotation = Quaternion.Euler(0.0f, m_actualMainSailRotation, 0.0f);
            m_frontSail.transform.localRotation = Quaternion.Euler(0.0f, m_actualFrontSailRotation, 0.0f);
        }
    }

    float GetWindSideSign(float windSide, float sailAngle)
    {
        float deadZone = 0.12f; // deadzone around 0

        if (windSide > deadZone)
        {
            // wind is clearly on the right side
            return 1.0f;
        }
        else if (windSide < -deadZone)
        {
            // wind is clearly on the left side
            return -1.0f;
        }
        else
        {
            // wind is almost in the middle --> between -0.12 and +0.12
            // move wind side a little bit --> avoid unstable movements if wind angle is 0°
            return Mathf.Sign(sailAngle + 0.001f);
        }
    }

    private void ResetSails()
    {
        // resets sails for switching to motor mode
        m_mainSail.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        m_frontSail.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        Debug.Log("Segel werden eingeholt!");
    }

    private void CalculateKeelForce()
    {
        // keel force doesn't need y-speed of boat
        Vector3 boatVelocity = m_rigidbody.linearVelocity;  // TODO Strömung --> anpassen, wenn Strömungssimulation vorhanden
        boatVelocity.y = 0f;

        // drift on x-axis of boat
        float driftToSide = Vector3.Dot(boatVelocity, transform.right);

        // project boal velocity on forward axis --> forward speed of boat
        float forwardSpeed = Vector3.Dot(boatVelocity, transform.forward);

        // keel lift needs to be increased for proper impact
        //float keelLiftValue = 0.5f * m_waterDensity * (driftToSide * driftToSide) * m_keelSize * 100.0f;
        float keelLiftValue = 0.5f * m_waterDensity * Mathf.Abs(driftToSide) * m_keelSize * 20.0f;

        // impact in opposite direction of drift
        Vector3 keelLift = -transform.right * keelLiftValue * Mathf.Sign(driftToSide);

        // drag needs to be separated from lift!

        // decrease drag by 0.01f --> drag is very small and impacts the forward speed a little --> small drag to the front
        // keel cuts the water while driving forward
        float keelDragValue = 0.5f * m_waterDensity * (forwardSpeed * forwardSpeed) * m_keelSize * 0.005f;
        Vector3 keelDrag = -transform.forward * keelDragValue * Mathf.Sign(forwardSpeed);

        m_rigidbody.AddForce(keelLift + keelDrag, ForceMode.Force);
    }

    private void CalculateRudderForce()
    {
        // similar to apparent wind: water flows against boat --> negative velocity
        // relative waterflow on rudder
        Vector3 waterVelocity = m_rigidbody.linearVelocity; // TODO Strömung --> anpassen, wenn Strömungssimulation vorhanden
        waterVelocity.y = 0.0f;

        float rudderAngle = m_steeringInput * m_maxRudderAngle;

        //float rudderForceValue = 0.5f * m_waterDensity * waterVelocity.magnitude * waterVelocity.magnitude * m_rudderSize * 0.05f;
        float rudderForceValue = 0.5f * m_waterDensity * waterVelocity.magnitude * m_rudderSize * 0.05f;

        // check force to the side
        float rudderEfficiency = Mathf.Sin(rudderAngle * Mathf.Deg2Rad);

        // vector to left/right --> cross product results in vector vertical on given vectors
        Vector3 rudderDirection = Vector3.Cross(waterVelocity, Vector3.up).normalized;

        Vector3 rudderForce = rudderDirection * rudderForceValue * rudderEfficiency;
        rudderForce.y = 0.0f;

        m_rigidbody.AddForceAtPosition(rudderForce, m_rudder.transform.position, ForceMode.Force);
    }

    private void CalculateWindOnHull()
    {
        Vector3 apparentWind = CalculateApprentWind();
        apparentWind.y = 0.0f;

        if(apparentWind.sqrMagnitude < 0.01f)
        {
            return;
        }

        // wind direction relative to boat vectors (forward and right)
        // 1 = wind from right side
        // -1 = wind from left side
        float windDotRight = Vector3.Dot(apparentWind.normalized, transform.right);
        // 1 = wind from behind
        // -1 = wind from front 
        float windDotForward = Vector3.Dot(apparentWind.normalized, transform.forward);

        // calculate wind effect
        float effectiveSideArea = m_boatSideSize * Mathf.Abs(windDotRight);
        float effectiveFrontArea = m_boatFrontSize * Mathf.Abs(windDotForward);

        // modifier
        float hullDragCoefficient = 0.4f;

        // calculate wind force value
        float windForceValue = 0.5f * m_airDensity * apparentWind.magnitude * apparentWind.magnitude * hullDragCoefficient;

        // side force
        Vector3 lateralForce = transform.right * Mathf.Sign(windDotRight) * effectiveSideArea * windForceValue;

        // front/back force
        Vector3 longitudinalForce = transform.forward * Mathf.Sign(windDotForward) * effectiveFrontArea * windForceValue;

        Vector3 totalHullForce = lateralForce + longitudinalForce;

        totalHullForce.y = 0.0f;

        // apply force 1m above boat --> lever effect --> heeling
        m_rigidbody.AddForceAtPosition(totalHullForce, transform.position + (transform.up * 1.0f), ForceMode.Force);
    }

    private void CalculateWaterResistance() // force agains boat forward direction
    {
        float boatSpeed = m_rigidbody.linearVelocity.magnitude;

        // avoid jittering
        if (boatSpeed < 0.01f)
        {
            return;
        }

        // linear drag = drag proportional to boat speed --> smooth movements, smooth braking
        // quadratic drag = drag proportional to quadartic boat speed --> dominant at high speed
        float linearDrag = 150.0f;
        float quadraticDrag = 70.0f;

        // simplified physics formula --> no real water and realistic boat physics
        // result: boat brakes smooth with lower speed, brakes drasticly with higher speed especially in motor mode --> just for the feeling, no proper physics formula
        Vector3 dragForce = Vector3.zero;
        if (m_gameManager.CurrentState == GameStates.SAILMODE)
        {
            dragForce = -m_rigidbody.linearVelocity.normalized * (boatSpeed * linearDrag + boatSpeed * boatSpeed * quadraticDrag);

        }
        else if(m_gameManager.CurrentState == GameStates.MOTORMODE)
        {
            dragForce = -m_rigidbody.linearVelocity.normalized * (boatSpeed * linearDrag + boatSpeed * boatSpeed * quadraticDrag + 500);
        }

        m_rigidbody.AddForce(dragForce, ForceMode.Force);
    }

    private void CalculateMotorForce() // max speed 7.9 knots in m/s --> 4.06m/s
    {
        if(m_currentFuel <= 0.0f)
        {
            return;
        }

        Vector3 motorForce = Vector3.zero;

        float speed = Vector3.Dot(m_rigidbody.linearVelocity, transform.forward); // project velocity on forward vector

        if (m_thrustStep > 0.0f) // boat driving forward
        {
            motorForce = transform.forward * m_thrustStep * m_forwardForcePerStep;
        }
        else if (m_thrustStep < 0.0f) // boat driving backward
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
        Debug.Log("thrust step " + m_thrustStep); // TODO Debug Logs raus

        // braking over time without thrust
        motorForce.y = 0.0f;
        if (m_thrustStep == 0.0f && m_rigidbody.linearVelocity.magnitude > 0.5f)
        {
            motorForce += -m_rigidbody.linearVelocity.normalized * 1500.0f; // 1500.0 = brake force modifier
        }

        m_rigidbody.AddForce(motorForce, ForceMode.Force);
    }

    private void ReduceFuel()
    {
        if(m_currentFuel < 0.0f)
        {
            m_currentFuel = 0.0f;
            m_thrustStep = 0.0f;
            Debug.LogError("TANK LEER");
            // TODO Game Over einbauen
            return;
        }

        // calculate fuel consumption based on thrust level
        float baseFuelConsumption = 0.05f;
        float fuelConsumptionPerThrust = 0.25f;
        float totalFuelConsumption = baseFuelConsumption + Mathf.Abs(m_thrustStep) * fuelConsumptionPerThrust;

        Debug.Log("Fuel Consumption " + totalFuelConsumption);

        m_currentFuel -= totalFuelConsumption * Time.fixedDeltaTime;
        Debug.Log("Fuel " + m_currentFuel);
    }

    // method for water-trail-shader to check whether the boat is moving backwards or not
    private void CheckBoatDrivingForward()
    {
        if(m_gameManager.CurrentState == GameStates.SAILMODE || m_gameManager.CurrentState == GameStates.MOTORMODE)
        {
            float dotProduct = Vector3.Dot(transform.forward, m_rigidbody.linearVelocity);

            if(dotProduct > 0.0f)
            {
                m_isBoatDrivingForward = true;
            }
            else if (dotProduct < 0.0f)
            {
                m_isBoatDrivingForward = false;
            }

            Debug.Log("Driving forward " + m_isBoatDrivingForward);
        }
    }

    private void CalculateSteering()
    {
        // smooth player input --> avoid jumps from -1 to 1 etc.
        // MoveTowards smooths value towards another value with fixedDeltaTime * 2 steps
        m_smoothedSteeringInput = Mathf.MoveTowards(m_smoothedSteeringInput, m_steeringInput, Time.fixedDeltaTime * 5.0f);

        // total rotational force
        float yawTorque = 0f;

        if (m_gameManager.CurrentState == GameStates.MOTORMODE)
        {
            // slow speed = assist boat rotation --> no speed = 1 --> full assist; speed = 0 --> no assist
            // "normalize" boat speed to 0.8
            // clamp between 0 and 1
            // rotational force while "standing"/slow speed --> multiplier 900
            float lowSpeedTorque = 900.0f * Mathf.Clamp01(1.0f - (m_rigidbody.linearVelocity.magnitude / 0.8f));

            // speed based rudder "force" --> acts as weight for rotational force --> wieghs the effectivness of motor and rudder
            // higher with speed --> the faster the boat, the higher the rudder impact
            float rudderSteeringImpact = Mathf.Clamp01(m_rigidbody.linearVelocity.magnitude / 0.8f) * 9000.0f;

            // motor power --> multiplies current thrust step --> the higher the thrust, the more the power
            // motor flow --> less rotational impact with higher speed 
            // multiplier 1000
            // 1.0f + ... avoids dividing by zero
            float thrustFlow = Mathf.Abs(m_thrustStep) * 1000.0f * (1.0f / (1.0f + m_rigidbody.linearVelocity.magnitude * 0.5f));

            // quadratic speed damping factor --> avoid wild movements at high speed
            // 1.0f + ... avoids dividing by zero
            float speedDamping = 1.0f / (1.0f + m_rigidbody.linearVelocity.magnitude * m_rigidbody.linearVelocity.magnitude * 0.3f);

            // total rotational force
            // steering input * motor/water/etc influences * stability factor
            yawTorque = m_smoothedSteeringInput * (lowSpeedTorque + rudderSteeringImpact + thrustFlow) * speedDamping;

            // Krängung depending on input and speed
            float rollTorque = -m_smoothedSteeringInput * m_rigidbody.linearVelocity.magnitude * 2000.0f;
            m_rigidbody.AddRelativeTorque(new Vector3(0, 0, rollTorque), ForceMode.Force);
        }
        else
        {
            // rotational force depends on input and boat speed
            // "normalize" speed with 5.0 and clamp between 0 and 1
            // needs to be "normalized" and clamped bc otherwise boat would rotate too much at high speed
            float speedFactor = Mathf.Clamp01(m_rigidbody.linearVelocity.magnitude / 5.0f);

            // multiplier 10000
            yawTorque = m_smoothedSteeringInput * 10000.0f * speedFactor;
        }

        m_rigidbody.AddTorque(m_parentReference.up * yawTorque, ForceMode.Force);
    }

    private void ApplyStability() // applies stability --> boat won't heel or rotate too much
    {
        float rollAngle = transform.localEulerAngles.z;

        // clamp roll angle to -180° - 180°
        if (rollAngle > 180.0f)
        {
            rollAngle -= 360.0f;
        }

        // modifiers
        float rollStabilityStrength = 80000.0f;
        float rollDamping = 30000.0f;

        // absolute heeling
        float absoluteRollAngle = Mathf.Abs(rollAngle);
        float progressiveFactor = 1.0f;

        // stability rises above 15° --> boat won't heel too much, exponential rise of stability
        if (absoluteRollAngle > 15.0f)
        {
            progressiveFactor += Mathf.Pow((absoluteRollAngle - 15.0f), 2.0f) * 5.0f;
        }

        // "lifting" boat upwards
        float rightingTorque = -Mathf.Sin(rollAngle * Mathf.Deg2Rad) * rollStabilityStrength * progressiveFactor;

        // rotational angle velocity in local coordinates
        Vector3 localAngularVelocity = transform.InverseTransformDirection(m_rigidbody.angularVelocity);
        // brake movement --> avoid jitters and oscillations
        float rollDampingTorque = -localAngularVelocity.z * rollDamping;

        m_rigidbody.AddRelativeTorque(new Vector3(0, 0, rightingTorque + rollDampingTorque));
    }

    private void BlockXRotation() // block x rotation --> boat won't dive
    {
        Vector3 currentEulerAngles = transform.localEulerAngles;
        currentEulerAngles.x = 0;
        transform.localEulerAngles = currentEulerAngles;
    }

    // INPUT METHODS /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void ChangeBoatMode()
    {
        if (m_changeMotorSailModeAction.action.triggered == true)
        {
            if (m_gameManager.CurrentState == GameStates.SAILMODE)
            {
                m_gameManager.SetState(GameStates.MOTORMODE);
                m_isInSailMode = false;
                ResetSails(); // TODO Jasi: Segel einholen Animation
                m_engineStartEvent.Post(gameObject); // Audio Event
                Debug.Log("Motormode enabled!");
            }
            else if (m_gameManager.CurrentState == GameStates.MOTORMODE)
            {
                m_gameManager.SetState(GameStates.SAILMODE);
                m_isInSailMode = true;
                m_engineStopEvent.Post(gameObject);  // Audio Event
                Debug.Log("Sailmode enabled!");
                Debug.Log("Segel werden aufgespannt!"); // TODO Jasi: Segel aufspannen Animation
            }
        }
    }
    
    private void GetSteeringInput()
    {
        if (m_gameManager.CurrentState == GameStates.MOTORMODE || m_gameManager.CurrentState == GameStates.SAILMODE)
        {
            // steering via 1d axis, possible in both modes
            if (m_steeringAction != null)
            {
                float rawSteeringInput = m_steeringAction.action.ReadValue<float>();

                if (Mathf.Abs(rawSteeringInput) > 0.01f)
                {
                    // accumulate steering input
                    m_steeringInput += rawSteeringInput * Time.deltaTime;
                    Debug.Log("steering input " + m_steeringInput);
                }

                m_steeringInput = Mathf.Clamp(m_steeringInput, -1.0f, 1.0f);
            }

            if (m_rudderNeutralAction != null && m_rudderNeutralAction.action.triggered == true)
            {
                m_steeringInput = 0.0f;
            }

            float rudderAngle = m_steeringInput * m_maxRudderAngle;

            Debug.Log("rudder angle " + rudderAngle);
        }
    }

    private void GetSelectedSail()
    {
        if(m_gameManager.CurrentState == GameStates.SAILMODE)
        {
            if(m_chooseMainSailAction.action.triggered == true)
            {
                m_currentSail = Sails.MAINSAIL;
            }

            if(m_chooseFrontSailAction.action.triggered == true)
            {
                m_currentSail = Sails.FRONTSAIL;
            }

            if(m_chooseBothSailsAction.action.triggered == true)
            {
                m_currentSail = Sails.BOTHSAILS;
            }
        }
    }

    private void GetSailTrimInput()
    {
        if(m_gameManager.CurrentState == GameStates.SAILMODE)
        {
            if(m_trimAction != null)
            {
                m_trimInput = m_trimAction.action.ReadValue<float>();
            }
        }
    }

    private void GetMotorInput()
    {
        if (m_gameManager.CurrentState == GameStates.MOTORMODE)
        {
            // read input and increase or decrease thrust or set thrust to neutral position
            if (m_motorThrustForwardAction != null && m_motorThrustForwardAction.action.IsPressed() == true)
            {
                m_thrustStep = Mathf.Min(m_thrustStep + 0.005f, m_maxThrustForwardSteps);
                //m_throttleMoveEvent.Post(gameObject);  // Audio Event
            }

            if (m_motorThrustBackwardAction != null && m_motorThrustBackwardAction.action.IsPressed() == true)
            {
                m_thrustStep = Mathf.Max(m_thrustStep - 0.005f, m_maxThrustBackwardSteps);
                //m_throttleMoveEvent.Post(gameObject);  // Audio Event
            }

            if (m_motorThrustNeutralAction != null && m_motorThrustNeutralAction.action.triggered == true)
            {
                m_thrustStep = 0.0f;
                m_throttleMoveIdleEvent.Post(gameObject);  // Audio Event
            }

            // Send current throttle lever value to Wwise RTPC.
            AkUnitySoundEngine.SetRTPCValue("Boat_ThrottleSigned", m_thrustStep, gameObject);
        }
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    // PUBLIC METHODS ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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

        if (m_steeringAction != null)
        {
            m_steeringAction.action.Enable();
        }

        if(m_rudderNeutralAction != null)
        {
            m_rudderNeutralAction.action.Enable();
        }

        if (m_changeMotorSailModeAction != null)
        {
            m_changeMotorSailModeAction.action.Enable();
        }

        if (m_chooseMainSailAction != null)
        {
            m_chooseMainSailAction.action.Enable();
        }

        if (m_chooseFrontSailAction != null)
        {
            m_chooseFrontSailAction.action.Enable();
        }

        if(m_chooseBothSailsAction != null)
        {
            m_chooseBothSailsAction.action.Enable();
        }

        if(m_trimAction != null)
        {
            m_trimAction.action.Enable();
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

        if (m_steeringAction != null)
        {
            m_steeringAction.action.Disable();
        }

        if (m_rudderNeutralAction != null)
        {
            m_rudderNeutralAction.action.Disable();
        }

        if (m_changeMotorSailModeAction != null)
        {
            m_changeMotorSailModeAction.action.Disable();
        }

        if (m_chooseMainSailAction != null)
        {
            m_chooseMainSailAction.action.Disable();
        }

        if (m_chooseFrontSailAction != null)
        {
            m_chooseFrontSailAction.action.Disable();
        }

        if (m_chooseBothSailsAction != null)
        {
            m_chooseBothSailsAction.action.Disable();
        }

        if (m_trimAction != null)
        {
            m_trimAction.action.Disable();
        }
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // UNITY METHODS /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();

        // rigidbody setup
        if (m_rigidbody == null)
        {
            m_rigidbody = gameObject.AddComponent<Rigidbody>();
            m_rigidbody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX; // | RigidbodyConstraints.FreezeRotationZ |
            m_rigidbody.angularDamping = 2.0f;
            m_rigidbody.linearDamping = 0.0f;
            m_rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            m_rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            m_rigidbody.mass = 9000.0f;
            m_rigidbody.centerOfMass = new Vector3(0.0f, -4.0f, 0.0f);
        }

        CalculateLiftAndDragTables(); // for lift and drag coefficients for sail force

        m_currentFuel = m_maxFuel;
        m_currentHealth = m_maxHealth;
    }

    public void Update()
    {
        ChangeBoatMode();

        GetSteeringInput();
        GetMotorInput();
        GetSelectedSail();
        GetSailTrimInput();

        CheckBoatDrivingForward();
    }

    public void FixedUpdate()
    {           
        // TODO Kollisionssystem Damage durch zu hohe Wellen

        //// DEBUG //////////////////////////////////////////////////////////////////////////////
        float speedKnot = m_rigidbody.linearVelocity.magnitude / m_msPerKnot;
        Debug.Log(speedKnot + " knots");
        //// DEBUG //////////////////////////////////////////////////////////////////////////////

        if(m_gameManager.CurrentState == GameStates.DIALOGMODE)
        {
            m_rigidbody.linearVelocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_rigidbody.isKinematic = true;
            BlockXRotation();

            Debug.Log("DIALOG-MODE");
        }
        else
        {
            m_rigidbody.isKinematic = false;
        }

        if(m_gameManager.CurrentState == GameStates.MOTORMODE)
        {
            ReduceFuel();
            CalculateMotorForce();
            CalculateKeelForce();
            CalculateWaterResistance();
            CalculateWindOnHull();
            CalculateRudderForce();
            CalculateSteering();
            ApplyStability();
            BlockXRotation();

            Debug.DrawRay(transform.position, m_rigidbody.linearVelocity * 10, Color.blue);
        }

        if (m_gameManager.CurrentState == GameStates.SAILMODE)
        {
            SailTrimming();

            CalculateKeelForce();
            CalculateWaterResistance();
            CalculateTotalSailForce();
            CalculateWindOnHull();
            CalculateRudderForce();
            CalculateSteering();
            ApplyStability();
            BlockXRotation();

            Debug.DrawRay(transform.position, m_rigidbody.linearVelocity * 10, Color.yellow);
        }
    }
}
