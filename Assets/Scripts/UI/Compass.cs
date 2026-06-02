using System.Collections.Generic;
using Dialogue;
using Quest;
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    public static Compass Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private RawImage m_compassImage;
    [SerializeField] private Transform m_cameraTransform;
    [SerializeField] private Transform m_boatTransform;
    [SerializeField] private GameObject m_iconPrefab;

    [Header("Default Icons")]
    [SerializeField] private Sprite m_defaultIcon;
    [SerializeField] private Sprite m_questIcon;
    [SerializeField] private Sprite m_talkIcon;

    [Header("Boat Direction")]
    [SerializeField] private Image m_boatDirectionIcon;

    private float m_maxDiatance = 2000f; //doesnt need this, but better to have it for scaling icons -> so just set it to cover entire map

    private List<CompassTargetBase> m_targets = new List<CompassTargetBase>();

    private float m_compassUnit;

    public Sprite DefaultIcon => m_defaultIcon;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_compassUnit = m_compassImage.rectTransform.rect.width / 360f;
    }

    private void Update()
    {
        if (m_cameraTransform == null || m_boatTransform == null)
        {
            return;
        }

        UpdateCompassRotation();
        UpdateBoatDirectionIcon();
        UpdateTargets();
    }

    private void UpdateCompassRotation()
    {
        m_compassImage.uvRect = new Rect(m_cameraTransform.localEulerAngles.y / 360f, 0f, 1f, 1f);
    }

    private void UpdateTargets()
    {
        for (int i = 0; i < m_targets.Count; i++)
        {
            CompassTargetBase _target = m_targets[i];

            if (_target == null)
            {
                continue;
            }

            if (_target.Image == null)
            {
                CreateUI(_target);
            }

            if (_target.Image == null)
            {
                continue;
            }

            bool _shouldShow = _target.ShouldShowIcon();

            _target.SetVisible(_shouldShow);

            if (!_shouldShow)
            {
                continue;
            }

            UpdateTargetIcon(_target);

            _target.Image.rectTransform.anchoredPosition = GetPos(_target);

            //scale based distance
            Vector2 _boatPos = new Vector2(m_cameraTransform.position.x, m_cameraTransform.position.z);
            float _distance = Vector2.Distance(_boatPos, _target.GetPosition());
            float _scale = 0f;

            if (_distance < m_maxDiatance)
            {
                _scale = 1f - (_distance / m_maxDiatance);
            }

            _target.Image.rectTransform.localScale = Vector3.one * _scale;
        }
    }

    private void UpdateTargetIcon(CompassTargetBase _target)
    {
        QuestInteraction _quest = _target as QuestInteraction;

        if (_quest != null)
        {
            _target.Image.sprite = m_questIcon;
            return;
        }

        NPCInteraction _npc = _target as NPCInteraction;

        if (_npc != null)
        {
            NPCIconState _state = _npc.GetCompassIconState();

            switch (_state)
            {
                case NPCIconState.Quest:
                    _target.Image.sprite = m_questIcon;
                    return;

                case NPCIconState.Talk:
                    _target.Image.sprite = m_talkIcon;
                    return;

                case NPCIconState.None:
                default:
                    return;
            }
        }

        _target.Image.sprite = _target.GetIcon();
    }

    private void UpdateBoatDirectionIcon()
    {
        if (m_boatDirectionIcon == null)
        {
            return;
        }

        if (m_boatTransform == null || m_cameraTransform == null)
        {
            return;
        }

        Vector2 _cameraForward = new Vector2(m_cameraTransform.forward.x, m_cameraTransform.forward.z);
        Vector2 _boatForward = new Vector2(m_boatTransform.forward.x, m_boatTransform.forward.z);

        float _angle = Vector2.SignedAngle(_boatForward, _cameraForward);

        m_boatDirectionIcon.rectTransform.anchoredPosition = new Vector2(m_compassUnit * _angle, 0f);
    }

    private void CreateUI(CompassTargetBase _target)
    {
        GameObject _go = Instantiate(m_iconPrefab, m_compassImage.transform);
        Image _img = _go.GetComponent<Image>();

        _target.BindImage(_img);
    }

    private Vector2 GetPos(CompassTargetBase _target)
    {
        Vector2 _boatPos = new Vector2(m_cameraTransform.position.x, m_cameraTransform.position.z);
        Vector2 _targetPos = _target.GetPosition();
        Vector2 _direction =_targetPos - _boatPos;
        Vector2 _boatForward = new Vector2(m_cameraTransform.forward.x, m_cameraTransform.forward.z);

        float _angle = Vector2.SignedAngle(_direction, _boatForward);

        return new Vector2(m_compassUnit * _angle, 0f);
    }

    public void Register(CompassTargetBase _target)
    {
        if (_target == null)
        {
            return;
        }

        if (!m_targets.Contains(_target))
        {
            m_targets.Add(_target);
        }
    }

    public void Unregister(CompassTargetBase _target)
    {
        if (_target == null)
        {
            return;
        }

        if (_target.Image != null)
        {
            Destroy(_target.Image.gameObject);
        }

        m_targets.Remove(_target);
    }
}

