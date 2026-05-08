using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

[DisallowMultipleComponent]
[RequireComponent(typeof(AkGameObj))]
public class MoneyAudio : MonoBehaviour
{
    [Header("Wwise Events")]
    [SerializeField] private WwiseEvent missionMoneyGainEvent;
    [SerializeField] private WwiseEvent missionMoneySpendEvent;

    public void PlayMoneyGain()
    {
        if (missionMoneyGainEvent == null)
        {
            Debug.LogWarning("missionMoneyGainEvent ist nicht zugewiesen.");
            return;
        }

        missionMoneyGainEvent.Post(gameObject);
    }

    public void PlayMoneySpend()
    {
        if (missionMoneySpendEvent == null)
        {
            Debug.LogWarning("missionMoneySpendEvent ist nicht zugewiesen.");
            return;
        }

        missionMoneySpendEvent.Post(gameObject);
    }
}