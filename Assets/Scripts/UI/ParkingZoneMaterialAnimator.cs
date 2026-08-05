using UnityEngine;

public class ParkingZoneMaterialAnimator : MonoBehaviour
{
    [SerializeField] private float m_scrollSpeed = 1f;

    private Renderer[] m_renderers;

    private void Awake()
    {
        m_renderers = GetComponentsInChildren<Renderer>();
    }

    private void Update()
    {
        foreach (Renderer renderer in m_renderers)
        {
            Material mat = renderer.material;

            Vector2 offset = mat.GetTextureOffset("_BaseMap");

            offset.y += Time.deltaTime * m_scrollSpeed;

            mat.SetTextureOffset("_BaseMap", offset);
        }
    }
}
