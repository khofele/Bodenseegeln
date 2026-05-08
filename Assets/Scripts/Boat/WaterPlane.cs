using UnityEngine;

public class WaterPlane : MonoBehaviour
{
    public void Update()
    {
        Vector3 currentEulerAngles = transform.eulerAngles;
        transform.eulerAngles = new Vector3(currentEulerAngles.x, currentEulerAngles.y, 0.0f);
    }
}
