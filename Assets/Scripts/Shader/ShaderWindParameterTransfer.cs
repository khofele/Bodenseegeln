using UnityEngine;

public class ShaderWindParameterTransfer : MonoBehaviour
{

    [SerializeField] WindController m_WindController = null;

    public void Update()
    {
        if (m_WindController != null) 
        {
            // Übergabe Funktioniert! wurde getestet
            Shader.SetGlobalVector("_Wind", new Vector4(
                m_WindController.ShaderWind.x,
                m_WindController.ShaderWind.y,
                m_WindController.ShaderWind.z,
                0.0f
                ));
        }
        else
        {
            Shader.SetGlobalVector("_Wind", Time.time * new Vector4(
                2.0f, 
                0.0f, 
                2.0f,
                0.0f
                ));
        }
    }
}
