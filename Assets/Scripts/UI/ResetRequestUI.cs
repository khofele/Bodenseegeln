using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetRequestUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI m_text = null;

    [Header("Input Action")]
    [SerializeField] private InputActionReference m_resetBoatAction = null;

    public void ShowScreen()
    {
        gameObject.SetActive(true);
        m_text.text = "Du musst abgeschleppt werden. Drücke R, um dein Boot abschleppen zu lassen!";
    }

    private void GetResetInput()
    {
        if(m_resetBoatAction.action.triggered == true)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if(gameObject.activeSelf == true)
        {
            GetResetInput();
        }
    }
}
