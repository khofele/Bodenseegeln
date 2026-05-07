using UnityEngine;
using UnityEngine.InputSystem;

public class MoneyAudioTester : MonoBehaviour
{
    [SerializeField] private MoneyAudio moneyAudio;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            moneyAudio.PlayMoneyGain();
            Debug.Log("Test: mission_moneyGain");
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            moneyAudio.PlayMoneySpend();
            Debug.Log("Test: mission_moneySpend");
        }
    }
}