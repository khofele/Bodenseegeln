using UnityEngine;
using UnityEngine.UI;

public abstract class CompassTargetBase : MonoBehaviour //parent class for all scripts that are components on objects that should be visulized on the compass
{
    [Header("Compass")]
    [SerializeField] private Sprite m_iconOverride;

    protected Image m_image;

    public abstract Vector2 GetPosition();

    public virtual bool ShouldShowIcon()
    {
        return true;
    }

    public virtual Sprite GetIcon()
    {
        if (m_iconOverride != null)
        {
            return m_iconOverride;
        }

        if (Compass.Instance != null)
        {
            return Compass.Instance.DefaultIcon;
        }

        return null;
    }

    protected virtual void OnEnable()
    {
        if (Compass.Instance != null)
        {
            Compass.Instance.Register(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (Compass.Instance != null)
        {
            Compass.Instance.Unregister(this);
        }
    }

    private void Start()
    {
        if (Compass.Instance != null)
        {
            Compass.Instance.Register(this);
        }
    }

    public void BindImage(Image _img)
    {
        m_image = _img;

        if (m_image == null)
        {
            return;
        }

        m_image.sprite = GetIcon();
        m_image.enabled = true;
    }

    public void SetVisible(bool _visible)
    {
        if (m_image == null)
        {
            return;
        }

        m_image.enabled = _visible;
    }

    public Image Image => m_image;
}