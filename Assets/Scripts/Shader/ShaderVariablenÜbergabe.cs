using UnityEngine;

public class ShaderVariablenÜbergabe : MonoBehaviour
{
    [SerializeField] private GameObject m_BoatRefrence = null;

    public void Update()
    {
        if (m_BoatRefrence.GetComponent<BoatController>().IsBoatDrivingForward) {
            Shader.SetGlobalVector("_INvelocity", new Vector4(
                m_BoatRefrence.GetComponent<Rigidbody>().linearVelocity.x,
                m_BoatRefrence.GetComponent<Rigidbody>().linearVelocity.y,
                m_BoatRefrence.GetComponent<Rigidbody>().linearVelocity.z,
                0.0f));
            Debug.Log("Velocity: " + m_BoatRefrence.GetComponent<Rigidbody>().linearVelocity);
            Debug.Log("Velocity: " + m_BoatRefrence.GetComponent<Rigidbody>().linearVelocity.magnitude);
        }
    }
}
