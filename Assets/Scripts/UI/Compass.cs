using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    [SerializeField] private RawImage m_compassImage;
    [SerializeField] private Transform m_boatTransform;
    /*[SerializeField]*/ private float m_maxDiatance = 2000f; //doesnt need this, but better to have it for scaling icons -> so just set it to cover entire map

    [SerializeField] private GameObject m_IconPrefab;

    private List<QuestMarker> m_questMarkers = new List<QuestMarker>();

    private float m_compassUnit;

    //just temporary references -> TODO: change them to REAL REFERENCES IN GAME
    //public QuestMarker one;

    private void Start()
    {
        m_compassUnit = m_compassImage.rectTransform.rect.width / 360f;

        //also only for testing -> TODO: HANDLE WITH REAL INGAME STUFF
        //AddQuestMarker(one);
    }

    private void Update()
    {
        m_compassImage.uvRect = new Rect(m_boatTransform.localEulerAngles.y / 360f, 0f, 1f, 1f);

        for (int i = 0; i < m_questMarkers.Count; i++)
        {
            m_questMarkers[i].m_image.rectTransform.anchoredPosition = GetPosOnCompass(m_questMarkers[i]);

            float _distance = Vector2.Distance(new Vector2(m_boatTransform.transform.position.x, m_boatTransform.transform.position.z), m_questMarkers[i].Position);

            Debug.Log($"[Compass] distance = {_distance}, of max distance = {m_maxDiatance}, so scle = 1 - ({(_distance / m_maxDiatance)})");
            
            float _scale = 0f;

            if (_distance < m_maxDiatance)
            {
                _scale = 1f - (_distance / m_maxDiatance);
            }

            m_questMarkers[i].m_image.rectTransform.localScale = Vector3.one * _scale;
        }
    }

    public void AddQuestMarker (QuestMarker _marker)
    {
        GameObject _newMarker = Instantiate(m_IconPrefab, m_compassImage.transform);
        _marker.m_image = _newMarker.GetComponent<Image>();
        _marker.m_image.sprite = _marker.Icon;

        m_questMarkers.Add(_marker);
    }

    private Vector2 GetPosOnCompass (QuestMarker _marker)
    {
        Vector2 _boatPos = new Vector2(m_boatTransform.transform.position.x, m_boatTransform.transform.position.z);
        Vector2 _boatFwd = new Vector2(m_boatTransform.transform.forward.x, m_boatTransform.transform.forward.z);

        float _angle = Vector2.SignedAngle(_marker.Position - _boatPos, _boatFwd);

        return new Vector2(m_compassUnit * _angle, 0f);
    }
}
