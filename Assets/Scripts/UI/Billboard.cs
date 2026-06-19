using UnityEngine;

//little script for Icons above docks and quests and NPCs to make it face the camera
public class Billboard : MonoBehaviour
{
    private Transform m_cam;

    private void LateUpdate()
    {
        if (m_cam == null)
        {
            if (Camera.main == null) return;
            m_cam = Camera.main.transform;
        }

        // look AT the camera (correct method)
        transform.LookAt(m_cam);

        // flip so it faces correctly (important for sprites)
        transform.Rotate(0f, 180f, 0f);
    }
}
