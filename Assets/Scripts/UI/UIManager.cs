using Unity.VisualScripting;
using UnityEngine;

public class UIManager : Manager<UIManager>
{
    [SerializeField] private BoatController m_boatController = null;
    [SerializeField] private WindController m_windController = null;

    // BOAT DATA /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public float GetBoatSpeed()
    {
        return m_boatController.BoatSpeedInKnots;
    }

    public Vector3 GetBoatPosition()
    {
        return m_boatController.transform.position;
    }

    public float GetThrustLeverStep()
    {
        return m_boatController.ThrustStep;
    }

    public float GetHeadingAngle()
    {
        float angle = Vector3.SignedAngle(Vector3.forward, m_boatController.transform.forward, Vector3.up);

        if (angle < 0.0f)
        {
            angle += 360.0f;
        }

        return angle;
    }

    public float GetFuelLevel()
    {
        return m_boatController.CurrentFuel;
    }

    public float GetBoatHealth()
    {
        return m_boatController.CurrentHealth;
    }

    public bool CheckFuelLevel()
    {
        return m_boatController.CurrentFuel < (m_boatController.MaxFuel * 0.2f);
    }

    public bool CheckBoathHealth()
    {
        return m_boatController.CurrentHealth < (m_boatController.MaxHealth * 0.2f);
    }

    public float GetMainSailTrim()
    {
        return m_boatController.MainSailTrim;
    }

    public float GetFrontSailTrim()
    {
        return m_boatController.FrontSailTrim;
    }

    public bool CheckButterflyModeEnabled()
    {
        return m_boatController.IsInButterfly;
    }

    public bool CheckFenderEnabled()
    {
        return m_boatController.IsFenderEnabled;
    }

    public bool CheckFenderAlarm()
    {
        if(m_boatController.IsFenderEnabled == true && (m_boatController.transform.GetComponent<Rigidbody>().linearVelocity.magnitude/ 0.514444f) >= 8.0f) // TODO speed threshold might need to be balanced
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CheckSailModeEnabled()
    {
        return m_boatController.IsInSailMode; // true = sail mode, false = motor mode
    }

    public float GetSteeringWheelAngle()
    {
        return m_boatController.CurrentRudderAngle * 3.0f; // TODO might need to balance modifier
    }

    public float GetApparentWindSpeed()
    {
        return (m_windController.TrueWind - m_boatController.transform.GetComponent<Rigidbody>().linearVelocity).magnitude;
    }

    public float GetApparentWindAngle()
    {
        Vector3 apparentWind = m_windController.TrueWind - m_boatController.transform.GetComponent<Rigidbody>().linearVelocity;

        float angle = Vector3.SignedAngle(m_boatController.gameObject.transform.forward, apparentWind, Vector3.up);

        if (angle < 0.0f)
        {
            angle += 360.0f;
        }

        return angle;
    }

    // WIND DATA /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public float GetTrueWindSpeed()
    {
        return m_windController.TrueWind.magnitude;
    }

    public float GetTrueWindAngle()
    {
        float angle = Vector3.SignedAngle(m_boatController.transform.forward, m_windController.TrueWind, Vector3.up);

        if(angle < 0.0f)
        {
            angle += 360.0f;
        }

        return angle;
    }
}
