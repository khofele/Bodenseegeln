using UnityEngine;

public class ShaderWindParameterTransfer : MonoBehaviour
{

    [SerializeField] WindController m_WindController = null;

    public void Update()
    {
        // Übergabe Funktioniert! wurde getestet
        Shader.SetGlobalVector("_Wind", new Vector4(
            m_WindController.TrueWind.x,
            m_WindController.TrueWind.y,
            m_WindController.TrueWind.z,
            0.0f
            ));
    }
}
