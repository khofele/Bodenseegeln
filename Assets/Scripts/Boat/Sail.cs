using UnityEngine;

public class Sail : MonoBehaviour
{
    private bool m_isOpen = true;
    private bool m_isResettingRotation = false;

    public bool IsOpen
    {
        get { return m_isOpen; }
        set { m_isOpen = value; }
    }

    public bool IsResettingRotation
    {
        get { return m_isResettingRotation; }
        set { m_isResettingRotation = value; }
    }
}
