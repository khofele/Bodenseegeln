using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionReference m_boatMoveAction;

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (GameManager.Instance.CurrentState == GameStates.DIALOGMODE)
        {
            if (m_boatMoveAction.action.enabled)
            {
                m_boatMoveAction.action.Disable();
            }
        }
        else
        {
            if (!m_boatMoveAction.action.enabled)
            {
                m_boatMoveAction.action.Enable();
            }
        }

        //TODO: handle camera movement and other game states
    }
}
