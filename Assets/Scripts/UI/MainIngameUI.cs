using Quest;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Quest;

public class MainIngameUI : MonoBehaviour
{
    private enum UIScreen
    {
        Motor = 0,
        Quest = 1,
        Sailing = 2
    }

    [Header("Scene References")]
    [SerializeField] private Transform m_boatTransform = null;

    [Header("Screens")]
    [SerializeField] private GameObject m_motorScreen = null;
    [SerializeField] private GameObject m_questScreen = null;
    [SerializeField] private GameObject m_sailingScreen = null;

    [Header("MotorScreen")]
    [SerializeField] private Image m_thrustBarFill = null;
    [SerializeField] private RectTransform m_thrustHandle = null;
    [SerializeField] private TMP_Text m_motorSpeedText = null;
    [SerializeField] private TMP_Text m_motorDistanceText = null;
    [SerializeField] private TMP_Text m_motorAngleText = null;

    [Header("QuestScreen")]
    [SerializeField] private TMP_Text m_missionTimeText = null;
    [SerializeField] private TMP_Text m_missionDistanceText = null;
    [SerializeField] private TMP_Text m_missionAngleText = null;
    [SerializeField] private TMP_Text m_targetPlaceText = null;

    [Header("SailingScreen")]
    [SerializeField] private TMP_Text m_headingText = null;
    [SerializeField] private TMP_Text m_sailingSpeedText = null;
    [SerializeField] private TMP_Text m_trueWindText = null;
    [SerializeField] private TMP_Text m_apparentWindText = null;
    [SerializeField] private RectTransform m_boatCenter = null;
    [SerializeField] private RectTransform m_trueWindArrow = null;
    [SerializeField] private RectTransform m_apparentWindArrow = null;
    [SerializeField] private float m_windRadius = 120f; //THE HELL should this be??????

    [Header("StatusIndicators")]
    [SerializeField] private Image m_fuelBar = null;
    [SerializeField] private Image m_healthBar = null;
    [SerializeField] private GameObject m_fuelNormalIcon = null;
    [SerializeField] private GameObject m_fuelLowIcon = null;
    [SerializeField] private GameObject m_healthNormalIcon = null;
    [SerializeField] private GameObject m_healthLowIcon = null;
    [SerializeField] private Image m_healthDamageOverlay = null;

    [Header("SailingPositions")]
    [SerializeField] private Image m_mainSailBar = null;
    [SerializeField] private Image m_frontSailBar = null;
    [SerializeField] private GameObject m_butterflyOn = null;
    [SerializeField] private GameObject m_butterflyOff = null;
    [SerializeField] private GameObject m_fenderOn = null;
    [SerializeField] private GameObject m_fenderOff = null;
    [SerializeField] private GameObject m_fenderAlarm = null;
    [SerializeField] private GameObject m_sailModeIcon = null;
    [SerializeField] private GameObject m_motorModeIcon = null;
    [SerializeField] private RectTransform m_rudderImage = null;

    [Header("CharInfo")]
    [SerializeField] private Image m_averageQuestFill = null;
    [SerializeField] private TMP_Text m_moneyText = null;

    [Header("Input")]
    [SerializeField] private InputActionReference m_nextScreenAction = null;
    [SerializeField] private InputActionReference m_previousScreenAction = null;

    private UIScreen m_currentScreen = UIScreen.Motor;
    private float m_lastHealth;
    private bool m_damageFlashActive;
    private float m_damageFlashTimer; 

    private void OnEnable()
    {
        if (m_nextScreenAction != null)
        {
            m_nextScreenAction.action.Enable();
        }

        if (m_previousScreenAction != null)
        {
            m_previousScreenAction.action.Enable();
        }

        UpdateScreenState();
    }

    private void Update()
    {
        HandleScreenSwitching();

        UpdateMotorScreen();
        UpdateQuestScreen();
        UpdateSailingScreen();
        UpdateCharInfo();
        UpdateStatusIndicators();
        UpdateSailingPositions();
    }

    private void HandleScreenSwitching()
    {
        if (m_nextScreenAction != null && m_nextScreenAction.action.WasPressedThisFrame())
        {
            NextScreen();
        }

        if (m_previousScreenAction != null && m_previousScreenAction.action.WasPressedThisFrame())
        {
            PreviousScreen();
        }
    }

    private void NextScreen()
    {
        m_currentScreen++;

        if ((int)m_currentScreen > 2)
        {
            m_currentScreen = UIScreen.Motor;
        }

        UpdateScreenState();

        Debug.Log($"[MainIngameUI] switched to {m_currentScreen}");
    }

    private void PreviousScreen()
    {
        m_currentScreen--;

        if ((int)m_currentScreen < 0)
        {
            m_currentScreen = UIScreen.Sailing;
        }

        UpdateScreenState();

        Debug.Log($"[MainIngameUI] switched to {m_currentScreen}");
    }

    private void UpdateScreenState()
    {
        if (m_motorScreen != null)
        {
            m_motorScreen.SetActive(m_currentScreen == UIScreen.Motor);
        }

        if (m_questScreen != null)
        {
            m_questScreen.SetActive(m_currentScreen == UIScreen.Quest);
        }

        if (m_sailingScreen != null)
        {
            m_sailingScreen.SetActive(m_currentScreen == UIScreen.Sailing);
        }
    }

    private void UpdateMotorScreen()
    {
        UpdateThrustLever();

        if (m_motorSpeedText != null)
        {
            float _speed = UIManager.Instance.GetBoatSpeed();
            m_motorSpeedText.text = $"{_speed:0.0} KN";
        }

        if (m_motorDistanceText != null)
        {
            float _distance = GetQuestTargetDistance();
            m_motorDistanceText.text = _distance < 0 ? "--" : $"{_distance:0} m";
        }

        if (m_motorAngleText != null)
        {
            float _angle = GetQuestTargetAngle();
            m_motorAngleText.text = _angle < 0 ? "--" : $"{_angle:0}°";
        }
    }

    private void UpdateQuestScreen()
    {
        if (!QuestManager.Instance.IsQuestRunning)
        {
            if (m_missionTimeText != null)
            {
                m_missionTimeText.text = "--";
            }

            if (m_targetPlaceText != null)
            {
                m_targetPlaceText.text = "--";
            }

            if (m_missionDistanceText != null)
            {
                m_missionDistanceText.text = "--";
            }

            if (m_missionAngleText != null)
            {
                m_missionAngleText.text = "--";
            }

            return;
        }

        QuestData _activeQuest = QuestManager.Instance.ActiveQuest;

        if (_activeQuest == null)
        {
            return;
        }

        if (m_missionTimeText != null)
        {
            float _missionTime = QuestManager.Instance.GetMissionTime();

            System.TimeSpan _time = System.TimeSpan.FromSeconds(_missionTime);

            m_missionTimeText.text = _time.ToString(@"hh\:mm\:ss");
        }

        if (m_missionDistanceText != null)
        {
            float _distance = GetQuestTargetDistance();
            m_missionDistanceText.text = _distance < 0 ? "--" : $"{_distance:0} m";
        }

        if (m_missionAngleText != null)
        {
            float _angle = GetQuestTargetAngle();
            m_missionAngleText.text = _angle < 0 ? "--" : $"{_angle:0}°";
        }

        if (m_targetPlaceText != null)
        {
            m_targetPlaceText.text = _activeQuest.QuestTargetPlace;
        }
    }

    private void UpdateSailingScreen()
    {
        if (m_headingText != null)
        {
            float _heading = UIManager.Instance.GetHeadingAngle();

            m_headingText.text = $"{_heading:0}°";
        }

        if (m_sailingSpeedText != null)
        {
            float _speed = UIManager.Instance.GetBoatSpeed();

            m_sailingSpeedText.text = $"{_speed:0.0} KN";
        }

        if (m_trueWindText != null)
        {
            float _tWind = UIManager.Instance.GetTrueWindSpeed();

            m_trueWindText.text = $"{_tWind:0.0} m/s";
        }

        if (m_apparentWindText != null)
        {
            float _aWind = UIManager.Instance.GetApparentWindSpeed();

            m_apparentWindText.text = $"{_aWind:0.0} m/s";
        }

        UpdateWindArrows();
    }

    private void UpdateCharInfo()
    {
        if (m_averageQuestFill != null)
        {
            float _average = GameManager.Instance.GetAverageQuestResult();

            m_averageQuestFill.fillAmount = _average / 5f;
        }

        if (m_moneyText != null)
        {
            int _money = GameManager.Instance.Money;

            m_moneyText.text = $"{_money}€";
        }
    }

    private void UpdateStatusIndicators()
    {
        float _fuel = UIManager.Instance.GetFuelLevel();
        float _health = UIManager.Instance.GetBoatHealth();
        bool _fuelLow = UIManager.Instance.CheckFuelLevel();
        bool _healthLow = UIManager.Instance.CheckBoathHealth();

        if (m_fuelBar != null)
        {
            m_fuelBar.fillAmount = _fuel;
        }

        if (m_healthBar != null)
        {
            m_healthBar.fillAmount = _health;
        }

        if (m_fuelLowIcon != null)
        {
            m_fuelLowIcon.SetActive(_fuelLow);
        }

        if (m_fuelNormalIcon != null)
        {
            m_fuelNormalIcon.SetActive(!_fuelLow);
        }

        if (m_healthLowIcon != null)
        {
            m_healthLowIcon.SetActive(_healthLow);
        }

        if (m_healthNormalIcon != null)
        {
            m_healthNormalIcon.SetActive(!_healthLow);
        }

        if (_health < m_lastHealth)
        {
            m_damageFlashActive = true;
            m_damageFlashTimer = 0.25f;
        }

        m_lastHealth = _health;

        if (m_damageFlashActive && m_healthDamageOverlay != null)
        {
            m_healthDamageOverlay.fillAmount = (_health / 1f);
            m_healthDamageOverlay.enabled = true;

            m_damageFlashTimer -= Time.deltaTime;
            if (m_damageFlashTimer <= 0f)
            {
                m_damageFlashActive = false;
                m_healthDamageOverlay.enabled = false;
            }
        }
    }

    private void UpdateSailingPositions()
    {
        float _main = UIManager.Instance.GetMainSailTrim();
        float _front = UIManager.Instance.GetFrontSailTrim();
        if (m_mainSailBar != null)
        {
            m_mainSailBar.fillAmount = _main;
        }

        if (m_frontSailBar != null)
        {
            m_frontSailBar.fillAmount = _front;
        }

        bool _butterfly = UIManager.Instance.CheckButterflyModeEnabled();
        if (m_butterflyOn != null)
        {
            m_butterflyOn.SetActive(_butterfly);
        }

        if (m_butterflyOff != null)
        {
            m_butterflyOff.SetActive(!_butterfly);
        }

        bool _fender = UIManager.Instance.CheckFenderEnabled();
        bool _alarm = UIManager.Instance.CheckFenderAlarm();
        if (m_fenderOn != null)
        {
            m_fenderOn.SetActive(_fender && !_alarm);
        }

        if (m_fenderOff != null)
        {
            m_fenderOff.SetActive(!_fender);
        }

        if (m_fenderAlarm != null)
        {
            m_fenderAlarm.SetActive(_alarm);
        }

        bool _sailMode = UIManager.Instance.CheckSailModeEnabled();
        if (m_sailModeIcon != null)
        {
            m_sailModeIcon.SetActive(_sailMode);
        }

        if (m_motorModeIcon != null)
        {
            m_motorModeIcon.SetActive(!_sailMode);
        }

        if (m_rudderImage != null)
        {
            float _angle = UIManager.Instance.GetSteeringWheelAngle();
            m_rudderImage.localEulerAngles = new Vector3(0, 0, -_angle);
        }
    }

    private void UpdateThrustLever()
    {
        float _thrust = UIManager.Instance.GetThrustLeverStep();

        float _normalized = Mathf.InverseLerp(-3f, 3f, _thrust);

        bool _isPositive = _thrust > 0.05f;
        bool _isNegative = _thrust < -0.05f;
        bool _isZero = !_isPositive && !_isNegative;

        //fill bar logic
        if (m_thrustBarFill != null)
        {
            if (_isPositive)
            {
                m_thrustBarFill.fillAmount = (int)Image.OriginHorizontal.Left;
            }
            else
            {
                m_thrustBarFill.fillAmount = (int)Image.OriginHorizontal.Right;
            }
        }

        //handle position
        if (m_thrustHandle != null)
        {
            RectTransform _parent = (RectTransform)m_thrustHandle.parent;
            float _width = _parent.rect.width;
            float _x = (_normalized - 0.5f) * _width;

            m_thrustHandle.anchoredPosition = new Vector2(_x, m_thrustHandle.anchoredPosition.y);
        }

        //TODO: arrow visual state
    }

    private void UpdateWindArrows()
    {
        if (m_boatCenter == null)
        {
            return;
        }

        float _trueAngle = UIManager.Instance.GetTrueWindAngle();
        float _apparentAngle = UIManager.Instance.GetApparentWindAngle();

        SetArrow(m_trueWindArrow, _trueAngle);
        SetArrow(m_apparentWindArrow, _apparentAngle);
    }

    private void SetArrow(RectTransform _arrow, float _angle)
    {
        if (_arrow == null)
        {
            return;
        }

        float _rad = _angle * Mathf.Deg2Rad;

        Vector2 _offset = new Vector2(Mathf.Sin(_rad), Mathf.Cos(_rad) * m_windRadius);

        _arrow.anchoredPosition = _offset;

        _arrow.localEulerAngles = new Vector3(0, 0, -_angle);
    }

    private float GetQuestTargetDistance()
    {
        Transform _target = QuestManager.Instance.GetCurrentQuestTarget();

        if (_target == null)
        {
            return -1f;
        }

        Vector3 _boatPos = UIManager.Instance.GetBoatPosition();

        return Vector3.Distance(_boatPos, _target.position);
    }

    private float GetQuestTargetAngle()
    {
        Transform _target = QuestManager.Instance.GetCurrentQuestTarget();

        if (_target == null)
        {
            return -1f;
        }

        Vector3 _boatPos = UIManager.Instance.GetBoatPosition();

        Vector3 _direction = _target.position - _boatPos;

        float _angle = Vector3.SignedAngle(m_boatTransform.forward, _direction, Vector3.up);

        if (_angle < 0f)
        {
            _angle += 360f;
        }

        return _angle;
    }
}
