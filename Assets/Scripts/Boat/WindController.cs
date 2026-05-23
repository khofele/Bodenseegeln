using UnityEngine;

public class WindController : MonoBehaviour
{
    // RESULT WIND VECTOR
    private Vector3 m_trueWind = Vector3.zero;

    // WIND STRENGTH VALUES
    private float m_baseWindStrength = 2.0f;
    private float m_startWindStrength = 1.0f; // start and target wind strength for smooth wind strength transitions
    private float m_targetWindStrength = 1.0f;

    // WIND DIRECTION VALUES
    private Vector3 m_currentWindDirection = Vector3.forward;
    private Vector3 m_startWindDirection = Vector3.forward; // start and target wind direction for smooth wind direction transitions
    private Vector3 m_targetWindDirection = Vector3.forward;

    // TIMER AND TRANSITION VALUES
    private float m_timerValue = 0.0f;
    private float m_stabilityDuration = 1.0f; // defines who long wind is stable --> defines when wind transition takes place
    private float m_transitionTimer = 0.0f; // timer for wind transition
    private float m_transitionDuration = 2.0f; // duration of wind transition
    private bool m_isTransitioning = false;

    public Vector3 TrueWind
    {
        get { return m_trueWind; }
    }

    private void ChangeWindVector()
    {
        // save current wind values
        m_startWindDirection = m_currentWindDirection;
        m_startWindStrength = m_trueWind.magnitude;

        // Timemodifier --> the smaller the longer the wind comes from the same-ish direction --> higher value = wind rotates a lot more, hectic changes
        float windAngleNoise = Mathf.PerlinNoise(Time.time * 0.05f, 0.0f);

        // map noise value (0-1) to -180 - 180
        // interpolate between max angles
        float randomWindAngle = Mathf.Lerp(-180.0f, 180.0f, windAngleNoise * 1.5f);

        // set target wind direction
        m_targetWindDirection = Quaternion.Euler(0.0f, randomWindAngle, 0.0f) * Vector3.forward;

        m_targetWindDirection.Normalize();

        // 100.0f as offset --> different value than wind angle noise needed! otherwise perlin would return almost the same value as the wind angle
        float windStrengthNoise = Mathf.PerlinNoise(Time.time * 2.0f + 100.0f, 0.0f);

        // base wind strength + random wind strength --> interpolate between -1 and 15
        m_targetWindStrength = m_baseWindStrength + Mathf.Lerp(-1.0f, 15.0f, windStrengthNoise);

        // randomize duration before wind direction change takes place
        m_stabilityDuration = Random.Range(10.0f, 25.0f);

        // start wind transition
        m_transitionTimer = 0.0f;
        m_isTransitioning = true;
    }

    private void ApplyWindTransition()
    {
        if(m_isTransitioning == false)
        {
            return;
        }

        m_transitionTimer += Time.deltaTime;

        // calculate transition progress
        // example: transition timer = 1, duration = 2 --> 1/2 = 50% of the transition is done
        float transitionProgress = m_transitionTimer / m_transitionDuration;

        // check if wind transition is complete
        if (transitionProgress >= 1.0f)
        {
            transitionProgress = 1.0f;
            m_isTransitioning = false;
        }

        // smooth step interpolates between 0 and 1 with smoothing at the beginning and end
        float smoothTransitionStep = Mathf.SmoothStep(0.0f, 1.0f, transitionProgress);

        // interpolate between start and target wind direction with smooth steps
        // slerp = spherical interpolation --> better for directions than lerp
        m_currentWindDirection = Vector3.Slerp(m_startWindDirection, m_targetWindDirection, smoothTransitionStep).normalized;

        // interpolate between start and target wind strength with smooth steps
        float currentStrength = Mathf.Lerp(m_startWindStrength, m_targetWindStrength, smoothTransitionStep);

        Debug.Log("Strength winddd " + currentStrength); // TODO Debug raus

        // calculate result wind vector with interpolated wind dir and wind strength
        m_trueWind = m_currentWindDirection * currentStrength;
    }

    public void Start()
    {
        m_trueWind = m_currentWindDirection * m_baseWindStrength;
        ChangeWindVector();
    }

    public void Update()
    {
        m_timerValue += Time.deltaTime;

        // check if wind change is due
        if (m_timerValue >= m_stabilityDuration)
        {
            ChangeWindVector();
            m_timerValue = 0.0f;
        }

        ApplyWindTransition();

        Debug.Log("Winddd " + m_trueWind); // TODO Debug raus
    }
}
