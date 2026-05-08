using UnityEngine;

public class WindController : MonoBehaviour
{
    private Vector3 m_trueWind = Vector3.zero;

    public Vector3 TrueWind
    {
        get { return m_trueWind; }
    }

    private void ChangeTrueWind()
    {
        // TODO implement
        // TODO Intervalle
    }

    public void Update()
    {
        ChangeTrueWind();
    }
}
