using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

using Quest;
using MapUI;

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
    [SerializeField] private MapUIController m_mapUI = null;

    [Header("Screens")]
    [SerializeField] private GameObject m_motorScreen = null;
    [SerializeField] private GameObject m_questScreen = null;
    [SerializeField] private GameObject m_sailingScreen = null;

    [Header("MotorScreen")]
    [SerializeField] private Image m_leftThrustBarFill = null;
    [SerializeField] private Image m_rightThrustBarFill = null;
    [SerializeField] private GameObject m_leftThrustArrow = null;
    [SerializeField] private GameObject m_rightThrustArrow = null;
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
    [SerializeField] private float m_windRadius = 120f;

    [Header("StatusIndicators")]
    [SerializeField] private Image m_fuelBar = null;
    [SerializeField] private Image m_healthBar = null;
    [SerializeField] private Image m_healthDamageBar = null;
    [SerializeField] private GameObject m_fuelNormalIcon = null;
    [SerializeField] private GameObject m_fuelLowIcon = null;
    [SerializeField] private GameObject m_healthNormalIcon = null;
    [SerializeField] private GameObject m_healthLowIcon = null;
    [SerializeField] private float m_healthDamageDelay = 0.5f;
    [SerializeField] private float m_healthDamageSpeed = 2.5f;
    [SerializeField] private Image m_damageOverlay = null;

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
    [SerializeField] private InputActionReference m_toggleMapAction = null;

    [Header("Notifications")]
    [SerializeField] private NotificationTextUI m_notificationText = null;
    [SerializeField][TextArea] private string m_fenderDamageMessage = "Zu schnelles fahren mit eingeschalteten Fendern verursacht Schaden. Schalte sie aus oder fahre langsamer";

    private UIScreen m_currentScreen = UIScreen.Motor;
    private float m_lastHealth;
    private bool m_isWaitingForDamageBar;
    private float m_damageTimer;
    private bool m_hasShownFenderDamageMessage = false;
    private bool m_lastFenderAlarmState = false;


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

        if (m_toggleMapAction != null)
        {
            m_toggleMapAction.action.Enable();
        }

        UpdateScreenState();
    }

    private void Update()
    {
        HandleScreenSwitching();
        HandleMapToggle();

        UpdateMotorScreen();
        UpdateQuestScreen();
        UpdateSailingScreen();
        UpdateCharInfo();
        UpdateStatusIndicators();
        UpdateSailingPositions();
        UpdateFenderDamageMessage();
    }
    /// <summary>
    /// Key Inputs to toggle screens and map
    /// </summary>
    //with input actions switching through the 3 screens back and forth
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

    //handle toggle to open map, when map is not open
    private void HandleMapToggle()
    {
        if (m_toggleMapAction == null)
        {
            return;
        }

        if (m_toggleMapAction.action.WasPressedThisDynamicUpdate())
        {
            m_mapUI.ToggleMap();
        }
    }

    /// <summary>
    /// Screens on bottom right of the screen (motor and quest and sailing screen)
    /// </summary>
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

            m_trueWindText.text = $"{_tWind:0.0} KN";
        }

        if (m_apparentWindText != null)
        {
            float _aWind = UIManager.Instance.GetApparentWindSpeed();

            m_apparentWindText.text = $"{_aWind:0.0} KN";
        }

        UpdateWindArrows();
    }

    /// <summary>
    /// Char Info on bottom left corner of the screen (average quest result and money)
    /// </summary>
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

    /// <summary>
    /// Status Indicators on the bottom right of the screen (fuel and health/damage)
    /// </summary>
    private void UpdateStatusIndicators()
    {
        float _fuel = UIManager.Instance.GetFuelLevel();
        float _normalizedFuel = Mathf.Clamp01(_fuel / m_boatTransform.GetComponent<BoatController>().MaxFuel);
        float _health = UIManager.Instance.GetBoatHealth();
        float _normalizedHealth = Mathf.Clamp01(_health / m_boatTransform.GetComponent<BoatController>().MaxHealth);
        bool _fuelLow = UIManager.Instance.CheckFuelLevel();
        bool _healthLow = UIManager.Instance.CheckBoathHealth();

        if (m_fuelBar != null)
        {
            m_fuelBar.fillAmount = _normalizedFuel;
        }

        if (m_healthBar != null)
        {
            m_healthBar.fillAmount = _normalizedHealth;
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
            m_isWaitingForDamageBar = true;
            m_damageTimer = m_healthDamageDelay;
        }

        if (m_isWaitingForDamageBar)
        {
            m_damageTimer -= Time.deltaTime;

            if (m_damageTimer <= 0f)
            {
                m_isWaitingForDamageBar = false;
            }
        }

        if (!m_isWaitingForDamageBar && m_healthDamageBar != null)
        {
            m_healthDamageBar.fillAmount = Mathf.MoveTowards(m_healthDamageBar.fillAmount, _normalizedHealth, m_healthDamageSpeed * Time.deltaTime);
        }

        if (_health > m_lastHealth && m_healthDamageBar != null)
        {
            m_healthDamageBar.fillAmount = _normalizedHealth;
        }

        if (m_damageOverlay != null && m_healthDamageBar != null)
        {
            float _difference = m_healthDamageBar.fillAmount - m_healthBar.fillAmount;

            float _alpha = Mathf.InverseLerp(0.10f, 0.20f, _difference) * 0.8f; //only *o.8f because 100% visibility is too intense -> change here if still too much

            Color _color = m_damageOverlay.color;
            _color.a = _alpha;

            m_damageOverlay.color = _color;
        }

        m_lastHealth = _health;
    }

    /// <summary>
    /// Sailing Positions on the bottom left of the screen (main sail and front sail bars and butterfly and fender and motor/sail mode and steering wheel)
    /// </summary>
    private void UpdateSailingPositions()
    {
        float _main = UIManager.Instance.GetMainSailTrim();
        float _front = UIManager.Instance.GetFrontSailTrim();

        //normalize sail angles (0° -> 90° => 0 -> 1) => COULD make better by making MaxSailAngle property instead of hardcoding 90°
        float _normalizedMain = Mathf.Clamp01(_main / 90f);
        float _normalizedFront = Mathf.Clamp01(_front / 90f);

        if (m_mainSailBar != null)
        {
            m_mainSailBar.fillAmount = _normalizedMain;
        }

        if (m_frontSailBar != null)
        {
            m_frontSailBar.fillAmount = _normalizedFront;
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

    private void UpdateFenderDamageMessage()
    {
        if (m_hasShownFenderDamageMessage)
        {
            return;
        }

        bool _alarm = UIManager.Instance.CheckFenderAlarm();
        bool _startedTakingDamage = _alarm && !m_lastFenderAlarmState;

        if (_startedTakingDamage && !m_hasShownFenderDamageMessage)
        {
            m_notificationText?.Show(m_fenderDamageMessage);

            m_hasShownFenderDamageMessage = true;
        }

        m_lastFenderAlarmState = _alarm;
    }

    /// <summary>
    /// helper methods
    /// </summary>
    private void UpdateThrustLever()
    {
        float _thrust = UIManager.Instance.GetThrustLeverStep();

        //normalize absolute thrust (0 -> 3)
        float _fill = Mathf.Clamp01(Mathf.Abs(_thrust)/3f);

        bool _isPositive = _thrust > 0.05f;
        bool _isNegative = _thrust < -0.05f;
        bool _isZero = !_isPositive && !_isNegative;

        //fill bar logic
        if (m_leftThrustBarFill != null)
        {
            m_leftThrustBarFill.fillAmount = _isNegative ? _fill : 0f;
        }

        if (m_rightThrustBarFill != null)
        {
            m_rightThrustBarFill.fillAmount = _isPositive ? _fill : 0f;
        }

        if (m_leftThrustArrow != null)
        {
            m_leftThrustArrow.SetActive(_isNegative || _isZero);

            RectTransform _leftArrowRect = m_leftThrustArrow.GetComponent<RectTransform>();
            RectTransform _leftBarRect = m_leftThrustBarFill.rectTransform;

            float _leftWidth = _leftBarRect.rect.width;

            //0 fill = center (x = 0), 1 fill = full left (x = -width)
            float _x = -(_leftWidth * m_leftThrustBarFill.fillAmount);
            _leftArrowRect.anchoredPosition = new Vector2(_x, _leftArrowRect.anchoredPosition.y);
        }

        if (m_rightThrustArrow != null)
        {
            m_rightThrustArrow.SetActive(_isPositive || _isZero);

            RectTransform _rightArrowRect = m_rightThrustArrow.GetComponent<RectTransform>();
            RectTransform _rightBarRect = m_rightThrustBarFill.rectTransform;

            float _rightWidth = _rightBarRect.rect.width;

            //0 fill = center (x = 0), 1 fill = full left (x = +width)
            float _x = (_rightWidth * m_rightThrustBarFill.fillAmount);
            _rightArrowRect.anchoredPosition = new Vector2(_x, _rightArrowRect.anchoredPosition.y);
        }
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

        float _correctedAngle = (_angle + 180f) % 360f; //angles must be flipped left<->right, front<->back

        float _rad = _correctedAngle * Mathf.Deg2Rad;

        Vector2 _offset = new Vector2(Mathf.Sin(_rad), Mathf.Cos(_rad)) * m_windRadius;

        _arrow.anchoredPosition = _offset;

        _arrow.localEulerAngles = new Vector3(0, 0, -_correctedAngle); //currently arrows point down on Image -> when pointig up/left/right = +180f/+90f/-90f
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
