using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private bool m_lastDialogModeState = true; //while game starts in SAILMODE this must be true -> must may be false if start game in UI

    private void Update()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        UpdateCursorState(GameManager.Instance.CurrentState == GameStates.DIALOGMODE);
    }

    private void UpdateCursorState(bool _isDialogMode)
    {
        //cursor only visible in DIALOGUEMODE -> Press ESC two times to use the mouse with cursor ouside the game window
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
