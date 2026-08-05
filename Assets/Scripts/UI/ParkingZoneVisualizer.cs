using UnityEngine;

public class ParkingZoneVisualizer : MonoBehaviour
{
    [Header("Borders")]
    [SerializeField] private Transform m_leftBorder;
    [SerializeField] private Transform m_rightBorder;
    [SerializeField] private Transform m_frontBorder;
    [SerializeField] private Transform m_backBorder;

    [Header("Rendering")]
    [SerializeField] private Renderer[] m_renderers;

    [Header("Appearance")]
    [SerializeField] private Color m_defaultColor = Color.blue;
    [SerializeField] private Color m_correctColor = Color.green;
    [SerializeField] private float m_borderHeight = 1f;
    [SerializeField] private float m_borderThickness = 0.05f;

    private void Awake()
    {
        AutoSizeToCollider();
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetCorrectParking(bool correct)
    {
        //Debug.Log($"SetCorrectParking called: {correct}");

        Color color = correct ? m_correctColor : m_defaultColor;

        foreach (Renderer r in m_renderers)
        {
            //r.material.color = color;

            r.material.SetColor("_BaseColor", color);
            r.material.SetColor("_EmissionColor", color);
        }
    }

    private void AutoSizeToCollider()
    {
        BoxCollider _collider = GetComponentInParent<BoxCollider>();

        if (_collider == null)
        {
            Debug.LogWarning("[ParkingZoneVisualizer] No parent BoxCollider found.");
            return;
        }

        Vector3 _center = _collider.center;

        float _width = _collider.size.x;
        float _depth = _collider.size.z;

        float _halfWidth = _width * 0.5f;
        float _halfDepth = _depth * 0.5f;

        float _y = 0f;//-m_borderHeight; //* 0.5f;

        // LEFT
        m_leftBorder.localPosition = _center + new Vector3(-_halfWidth, _y, 0f);
        m_leftBorder.localScale = new Vector3(m_borderThickness, m_borderHeight, _depth);

        // RIGHT
        m_rightBorder.localPosition = _center + new Vector3(_halfWidth, _y, 0f);
        m_rightBorder.localScale = new Vector3(m_borderThickness, m_borderHeight, _depth);

        // FRONT
        m_frontBorder.localPosition = _center + new Vector3(0f, _y, _halfDepth);
        m_frontBorder.localScale = new Vector3(_width, m_borderHeight, m_borderThickness);

        // BACK
        m_backBorder.localPosition = _center + new Vector3(0f, _y, -_halfDepth);
        m_backBorder.localScale = new Vector3(_width, m_borderHeight, m_borderThickness);
    }
}
