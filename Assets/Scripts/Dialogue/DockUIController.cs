using UnityEngine;
using UnityEngine.UI;
using Dialogue;

public class DockUIController : MonoBehaviour
{
    [Header("Buttons")]
    //[SerializeField] private Button m_talkButton = null;
    [SerializeField] private Button m_repairButton = null;
    [SerializeField] private Button m_fuelButton = null;
    [SerializeField] private Button m_closeButton = null;

    private DockInteraction m_currentDock = null;

    public void Init(DockInteraction _dock)
    {
        m_currentDock = _dock;

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

    //private void OnTalkClicked()
    //{
    //    m_currentDock.StartDialogue();
    //}

    private void OnRepairClicked()
    {
        Debug.Log("[DockUIController.OnRepairClicked] -not implemented yet");
    }

    private void OnFuelClicked()
    {
        Debug.Log("[DockUIController.OnFuelClicked] -not implemented yet");
    }

    private void OnCloseClicked()
    {
        m_currentDock.CloseDockUI();
    }
}
