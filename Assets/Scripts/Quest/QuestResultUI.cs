using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Quest
{
    public class QuestResultUI : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject m_root;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI m_timeText;
        [SerializeField] private TextMeshProUGUI m_damageText;
        [SerializeField] private TextMeshProUGUI m_fuelText;
        [SerializeField] private TextMeshProUGUI m_moneyText;

        [Header("Stars")]
        [SerializeField] private Image[] m_timeStars;
        [SerializeField] private Image[] m_damageStars;
        [SerializeField] private Image[] m_fuelStars;
        [SerializeField] private Sprite m_filledStar;
        [SerializeField] private Sprite m_emptyStar;

        [Header("Button")]
        [SerializeField] private Button m_closeButton;

        private QuestResult m_currentReult;

        private void Awake()
        {
            m_closeButton.onClick.AddListener(OnClose);
        }

        internal void Show(QuestResult _result)
        {
            m_currentReult = _result;

            m_timeText.text = FormatTime(_result.TimeValue);
            m_damageText.text = _result.DamageValue.ToString();
            m_fuelText.text = _result.FuelValue.ToString();
            m_moneyText.text = _result.MoneyReward.ToString();

            SetStars(m_timeStars, _result.TimeStars);
            SetStars(m_damageStars, _result.DamageStars);
            SetStars(m_fuelStars, _result.FuelStars);

            if (_result.Quest.QuestType == QuestType.Type3_DirectCompletion)
            {
                GameManager.Instance.SetState(GameStates.DIALOGMODE);
            }

            m_root.SetActive(true);
        }

        private void SetStars(Image[] _stars, int _amount)
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                if (i < _amount)
                {
                    _stars[i].sprite = m_filledStar;
                }
                else
                {
                    _stars[i].sprite = m_emptyStar;
                }
            }
        }

        private string FormatTime(float _seconds)
        {
            int _h = Mathf.FloorToInt(_seconds / 3600f);
            int _m = Mathf.FloorToInt((_seconds % 3600f) / 60f);
            int _s = Mathf.FloorToInt(_seconds % 60f);

            return $"{_h:00}:{_m:00}:{_s:00}";
        }

        private void OnClose()
        {
            GameManager.Instance.AddMoney(m_currentReult.MoneyReward);

            if (m_currentReult.Quest.QuestType == QuestType.Type3_DirectCompletion)
            {
                GameManager.Instance.SetState(GameStates.MOTORMODE);
            }

            m_root.SetActive(false);

            Debug.Log("[QuestResultUI] Closed result screen");
        }
    }
}
