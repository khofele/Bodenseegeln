using UnityEngine;

public class BoatGameOverController : MonoBehaviour
{
    private float m_rotationAngle = 95.0f;
    private float m_moveDownAmount = 0.6f;
    private float m_rotationDuration = 4.0f;
    private Quaternion m_startRotation = Quaternion.identity;
    private Quaternion m_targetRotation = Quaternion.identity;
    private Vector3 m_startPosition = Vector3.zero;
    private Vector3 m_targetPosition = Vector3.zero;
    private float m_timer = 0.0f;
    private bool m_isRotating = false;

    public bool IsRotating
    {
        get { return m_isRotating; }
        set { m_isRotating = value; }
    }

    private void Start()
    {
        m_startRotation = transform.rotation;
        m_targetRotation = m_startRotation * Quaternion.Euler(0.0f, 0.0f, m_rotationAngle);

        m_startPosition = transform.position;
        m_targetPosition = transform.position + Vector3.down * m_moveDownAmount;
    }

    private void RotateBoat()
    {
        if (m_timer >= m_rotationDuration)
        {
            return;
        }

        m_timer += Time.deltaTime;

        float timerProgression = m_timer / m_rotationDuration;

        timerProgression = Mathf.SmoothStep(0.0f, 1.0f, timerProgression);

        transform.rotation = Quaternion.Lerp(m_startRotation, m_targetRotation, timerProgression);
        transform.position = Vector3.Lerp(m_startPosition, m_targetPosition, timerProgression);
    }

    private void Update()
    {
        if(m_isRotating == true)
        {
            RotateBoat();
        }
    }
}
