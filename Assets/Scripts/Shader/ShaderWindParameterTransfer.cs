using UnityEngine;

public class ShaderWindParameterTransfer : MonoBehaviour
{

    [SerializeField] WindController m_WindController = null;

    public void Update()
    {
        // Übergabe Funktioniert! wurde getestet
        Shader.SetGlobalVector("_Wind", new Vector4(
            m_WindController.ShaderWind.x,
            m_WindController.ShaderWind.y,
            m_WindController.ShaderWind.z,
            0.0f
            ));
    }
}
