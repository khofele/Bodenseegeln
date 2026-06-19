using UnityEngine;

[System.Serializable]
public class RTPCMapping
{
    [Header("Unity Input Range")]
    public float unityMin = 0.0f;
    public float unityMax = 100.0f;

    [Header("Wwise Output Range")]
    public float wwiseMin = 0.0f;
    public float wwiseMax = 100.0f;

    public float Map(float unityValue)
    {
        if (Mathf.Approximately(unityMin, unityMax))
        {
            return wwiseMin;
        }

        float normalizedValue = Mathf.InverseLerp(unityMin, unityMax, unityValue);
        float mappedValue = Mathf.Lerp(wwiseMin, wwiseMax, normalizedValue);

        float minValue = Mathf.Min(wwiseMin, wwiseMax);
        float maxValue = Mathf.Max(wwiseMin, wwiseMax);

        return Mathf.Clamp(mappedValue, minValue, maxValue);
    }
}
