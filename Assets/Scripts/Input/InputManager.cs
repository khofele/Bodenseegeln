using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionReference m_boatMoveAction;

    private bool m_lastDialogModeState = true; //for now while gane starts in SAILMODE this must be true -> must may be false when later start game in UI

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
        UpdateCursorState(GameManager.Instance.CurrentState == GameStates.DIALOGMODE);
    }

    private void UpdateCursorState(bool _isDialogMode)
    {
        //cursor only visible in DIALOGUEMODE -> Press ESC to exit the game and use the mouse with cursor ouside the game window
        if (_isDialogMode == m_lastDialogModeState)
        {
            return;
        }

        m_lastDialogModeState = _isDialogMode;

        if (_isDialogMode)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined; //cursor stays inside the game window
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; //mouse movement still works for camera but cursor cant escape the game window
        }
    }
}
