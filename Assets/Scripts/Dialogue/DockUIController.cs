using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dialogue;

public class DockUIController : MonoBehaviour
{
    [SerializeField] private MissionsFeedbackAudioController m_missionsFeedbackAudioController = null;  // Audio class reference

    [Header("Costs")]
    [SerializeField] private int m_fullFuelCost = 100;
    [SerializeField] private int m_fullRepairCost = 100;

    [Header("Cost UI")]
    [SerializeField] private TMP_Text m_fuelCostText = null;
    [SerializeField] private TMP_Text m_repairCostText = null;
    [SerializeField] private string m_WarningFuelText = null;
    [SerializeField] private string m_WarningRepairText = null;

    [Header("Buttons")]
    //[SerializeField] private Button m_talkButton = null;
    [SerializeField] private Button m_repairButton = null;
    [SerializeField] private Button m_fuelButton = null;
    [SerializeField] private Button m_closeButton = null;

    private DockInteraction m_currentDock = null;
    private BoatController m_currentBoat = null;

    public void Init(DockInteraction _dock)
    {
        m_currentDock = _dock;
        m_currentBoat = _dock.CurrentBoat.GetComponent<BoatController>();

        m_repairCostText.gameObject.SetActive(false);
        m_fuelCostText.gameObject.SetActive(false);

        //clear old listeners
        //m_talkButton.onClick.RemoveAllListeners();
        m_repairButton.onClick.RemoveAllListeners();
        m_fuelButton.onClick.RemoveAllListeners();
        m_closeButton.onClick.RemoveAllListeners();

        //add listeners
        //m_talkButton.onClick.AddListener(OnTalkClicked);
        m_repairButton.onClick.AddListener(OnRepairClicked);
        m_fuelButton.onClick.AddListener(OnFuelClicked);
        m_closeButton.onClick.AddListener(OnCloseClicked);
    }

    private int CalculateFuelCost()
    {
        float _missingFuel = m_currentBoat.MaxFuel - m_currentBoat.CurrentFuel;
        float _missingPercent = _missingFuel / m_currentBoat.MaxFuel;
        return Mathf.CeilToInt(m_fullFuelCost * _missingPercent);
    }

    private int CalculateRepairCost()
    {
        float _missingHealth = m_currentBoat.MaxHealth - m_currentBoat.CurrentHealth;
        float _missingPercent = _missingHealth / m_currentBoat.MaxHealth;
        return Mathf.CeilToInt(m_fullRepairCost * _missingPercent);
    }

    public void ShowFuelCost()
    {
        int _cost = CalculateFuelCost();
        int _money = GameManager.Instance.Money;

        if (m_currentBoat.CurrentFuel >= m_currentBoat.MaxFuel)
        {
            return;
        }

        m_fuelCostText.gameObject.SetActive(true);

        if (_money >= _cost)
        {
            m_fuelCostText.color = Color.white;
            m_fuelCostText.text = $"Kosten: {_cost}";
        }
        else
        {
            m_fuelCostText.color = Color.indianRed;
            m_fuelCostText.text = $"Kosten: {_cost}€\n{m_WarningFuelText}";
        }
    }

    public void HideFuelCost()
    {
        m_fuelCostText.gameObject.SetActive(false);
    }

    private void RefreshFuelCostUI()
    {
        if (m_fuelCostText.gameObject.activeSelf)
        {
            HideFuelCost();
            ShowFuelCost();
        }
    }

    public void ShowRepairCost()
    {
        int _cost = CalculateRepairCost();
        int _money = GameManager.Instance.Money;

        if (m_currentBoat.CurrentHealth >= m_currentBoat.MaxHealth)
        {
            return;
        }

        m_repairCostText.gameObject.SetActive(true);

        if (_money >= _cost)
        {
            m_repairCostText.color = Color.white;
            m_repairCostText.text = $"Kosten: {_cost}";
        }
        else
        {
            m_repairCostText.color = Color.indianRed;
            m_repairCostText.text = $"Kosten: {_cost}€\n{m_WarningRepairText}";
        }
    }

    public void HideRepairCost()
    {
        m_repairCostText.gameObject.SetActive(false);
    }

    private void RefreshRepairCostUI()
    {
        if (m_repairCostText.gameObject.activeSelf)
        {
            HideRepairCost();
            ShowRepairCost();
        }
    }

    //private void OnTalkClicked()
    //{
    //    m_currentDock.StartDialogue();
    //}

    private void OnRepairClicked()
    {
        if (m_currentBoat == null)
        {
            return;
        }

        float _missingHealth = m_currentBoat.MaxHealth - m_currentBoat.CurrentHealth;
        if (_missingHealth <= 0f)
        {
            Debug.Log("[DockUIController.OnRepairClicked] boat already fully repaired");
            return;
        }

        int _cost = CalculateRepairCost();
        int _money = GameManager.Instance.Money;

        //enough money -> repair full
        if (_money >= _cost)
        {
            m_currentBoat.CurrentHealth = m_currentBoat.MaxHealth;
            GameManager.Instance.DecreaseMoney(_cost);

            //update repair cost
            RefreshRepairCostUI();

            Debug.Log($"[DockUIController.OnRepairClicked] fully repaired for {_cost}€");
            return;
        }

        //not enough money -> partial repair
        float _healthPerEuro = m_currentBoat.MaxHealth / m_fullRepairCost;
        float _healthToAdd = _money * _healthPerEuro;
        float _newHealth = Mathf.Min(m_currentBoat.CurrentHealth + _healthToAdd, m_currentBoat.MaxHealth);

        m_currentBoat.CurrentHealth = _newHealth;
        GameManager.Instance.DecreaseMoney(_money);
        m_missionsFeedbackAudioController.PlayMoneyZero();  // Play Audio Money Zero

        Debug.Log("[DockUIController.OnRepairClicked] not enough money -> partially repaired until money reached 0€");

        //Update repair cost
        RefreshRepairCostUI();
    }

    private void OnFuelClicked()
    {
        if (m_currentBoat == null)
        {
            return;
        }

        float _missingFuel = m_currentBoat.MaxFuel - m_currentBoat.CurrentFuel;
        if (_missingFuel <= 0f)
        {
            Debug.Log("[DockUIController.OnFuelClicked] boat already full");
            return;
        }

        int _cost = CalculateFuelCost();
        int _money = GameManager.Instance.Money;

        //enough money -> refuel full
        if (_money >= _cost)
        {
            m_currentBoat.CurrentFuel = m_currentBoat.MaxFuel;
            GameManager.Instance.DecreaseMoney(_cost);

            //update fuel cost
            RefreshFuelCostUI();

            Debug.Log($"[DockUIController.OnFuelClicked] fully refueled for {_cost}€");
            return;
        }

        //not enough money -> partial refuel
        float _fuelPerEuro = m_currentBoat.MaxFuel / m_fullFuelCost;
        float _fuelToAdd = _money * _fuelPerEuro;
        float _newFuel = Mathf.Min(m_currentBoat.CurrentFuel + _fuelToAdd, m_currentBoat.MaxFuel);

        m_currentBoat.CurrentFuel = _newFuel;
        GameManager.Instance.DecreaseMoney(_money);
        m_missionsFeedbackAudioController.PlayMoneyZero();  // Play Audio Money Zero

        Debug.Log("[DockUIController.OnFuelClicked] not enough money -> partially refueled until money reached 0€");

        //Update fuel cost
        RefreshFuelCostUI();
    }

    private void OnCloseClicked()
    {
        m_currentDock.CloseDockUI();
    }
}
