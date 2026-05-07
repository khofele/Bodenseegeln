using UnityEngine;

public class UIManager : Manager<UIManager>
{
    [SerializeField] private BoatController m_boat = null;
    [SerializeField] private WindController m_wind = null;

    // BOAT DATA /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public float GetBoatSpeed()
    {
        return m_boat.BoatSpeedInKnots;
    }

    public Vector3 GetBoatPosition()
    {
        return m_boat.transform.position;
    }

    public float GetThrustLeverStep()
    {
        // TODO implement
        return 0.0f;
    }

    public float GetHeadingAngle()
    {
        // TODO implement
        return 0.0f;
    }

    public float GetFuelLevel()
    {
        return m_boat.CurrentFuel;
    }

    public float GetBoatHealth()
    {
        return m_boat.CurrentHealth;
    }

    public bool CheckFuelLevel()
    {
        return m_boat.CurrentFuel < (m_boat.MaxFuel * 0.2f);
    }

    public bool CheckBoathHealth()
    {
        return m_boat.CurrentHealth < (m_boat.MaxHealth * 0.2f);
    }

    public float GetMainSailTrim()
    {
        // TODO implement
        return 0.0f;
    }

    public float GetFrontSailTrim()
    {
        // TODO implement
        return 0.0f;
    }

    public bool CheckButterflyModeEnabled()
    {
        return m_boat.IsInButterfly;
    }

    public bool CheckFenderEnabled()
    {
        // TODO implement
        return false;
    }

    public bool CheckFenderAlarm()
    {
        // TODO implement
        return false;
    }

    public bool CheckSailModeEnabled()
    {
        return m_boat.IsInSailMode; // true = sail mode, false = motor mode
    }

    public float GetSteeringWheelAngle()
    {
        // TODO implement
        return 0.0f;
    }

    public float GetApparentWindSpeed()
    {
        // TODO implement
        return 0.0f;
    }

    public float GetApparentWindAngle()
    {
        // TODO implement
        return 0.0f;
    }

    // WIND DATA /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public float GetTrueWindSpeed()
    {
        // TODO implement
        return 0.0f;
    }

    public float GetTrueWindAngle()
    {
        // TODO implement
        return 0.0f;
    }
}
