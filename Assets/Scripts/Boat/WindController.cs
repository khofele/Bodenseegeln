using UnityEngine;

public class WindController : MonoBehaviour
{
    private Vector3 m_trueWind = Vector3.zero;
    private float m_baseWindStrength = 2.0f; // TODO balance
    private Vector3 m_currentWindDirection = Vector3.forward;
    private float m_startWindStrength = 1.0f;
    private float m_targetWindStrength = 1.0f;
    private Vector3 m_startWindDirection = Vector3.forward;
    private Vector3 m_targetWindDirection = Vector3.forward;
    private float m_timerValue = 0.0f;
    private float m_stabilityDuration = 5.0f;
    private float m_transitionTimer = 0.0f;
    private float m_transitionDuration = 2.0f;
    private bool m_isTransitioning = false;

    public Vector3 TrueWind
    {
        get { return m_trueWind; }
    }

    private void ChangeWindDirection()
    {
        m_startWindDirection = m_currentWindDirection;
        m_startWindStrength = m_trueWind.magnitude;

        // Timemodifier --> the smaller the longer the wind comes from the same-ish direction --> higher value = wind rotates a lot more
        float windAngleNoise = Mathf.PerlinNoise(Time.time * 0.05f, 0.0f);

        // map noise value (0-1) to -180 - 180
        float randomWindAngle = Mathf.Lerp(-180.0f, 180.0f, windAngleNoise * 1.5f);

        m_targetWindDirection = Quaternion.Euler(0.0f, randomWindAngle, 0.0f) * Vector3.forward;

        m_targetWindDirection.Normalize();


        // 100.0f as offset --> different value than wind angle noise needed! otherwise perlin would return almost the same value as the wind angle
        float windStrengthNoise = Mathf.PerlinNoise(Time.time * 0.1f + 100.0f, 0.0f);

        m_targetWindStrength = m_baseWindStrength + Mathf.Lerp(0.5f, 11.0f, windStrengthNoise);

        m_stabilityDuration = Random.Range(3.0f, 10.0f);

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
        float transitionProgress = m_transitionTimer / m_transitionDuration;
        if (transitionProgress >= 1.0f)
        {
            transitionProgress = 1.0f;
            m_isTransitioning = false;
        }

        float smoothTransitionStep = Mathf.SmoothStep(0.0f, 1.0f, transitionProgress);

        m_currentWindDirection = Vector3.Slerp(m_startWindDirection, m_targetWindDirection, smoothTransitionStep).normalized;

        float currentStrength = Mathf.Lerp(m_startWindStrength, m_targetWindStrength, smoothTransitionStep);

        m_trueWind = m_currentWindDirection * currentStrength;
    }

    public void Start()
    {
        m_trueWind = m_currentWindDirection * m_baseWindStrength;
        ChangeWindDirection();
    }

    public void Update()
    {
        m_timerValue += Time.deltaTime;

        if (m_timerValue >= m_stabilityDuration)
        {
            ChangeWindDirection();
            m_timerValue = 0.0f;
        }

        ApplyWindTransition();

        Debug.Log("Winddd " + m_trueWind);
    }
}
