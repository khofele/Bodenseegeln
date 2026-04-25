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

            m_timeText.text = $"Time: {_result.TimeScore:0}/3";
            m_damageText.text = $"Damage: {_result.DamageScore:0}/3";
            m_fuelText.text = $"Fuel: {_result.FuelScore:0}/3";
            m_moneyText.text = $"Reward: {_result.moneyReward}";

            m_root.SetActive(true);
        }

        private void OnClose()
        {
            m_root.SetActive(false);

            Debug.Log("[QuestResultUI] Closed result screen");

            //TODO: add reward money to players money
        }
    }
}
