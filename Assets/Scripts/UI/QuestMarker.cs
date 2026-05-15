using UnityEngine;
using UnityEngine.UI;

public class QuestMarker : MonoBehaviour
{
    [SerializeField] private Sprite m_questIcon;
    /*[SerializeField] private*/ public Image m_image;

    //public Image Image => m_image;

    public Sprite Icon => m_questIcon;

    internal Vector2 Position { get { return new Vector2(transform.position.x, transform.position.z); }  }
}
