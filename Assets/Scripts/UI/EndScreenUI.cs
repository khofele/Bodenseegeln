using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndScreenUI : MonoBehaviour
{
    private GameManager m_gameManager = null;

    [SerializeField] private GameObject m_endscreenConfettiParent = null; // TODO Konfetti auslösen fixen
    [SerializeField] private BoatGameOverController m_boatGameOverController = null;

    [Header("UI Components")]
    [SerializeField] private Button m_btnEndGame = null;
    [SerializeField] private TextMeshProUGUI m_txtTitle = null;

    private void Awake()
    {
        m_gameManager = FindFirstObjectByType<GameManager>();

        if (m_gameManager.CurrentState == GameStates.GAMEWON)
        {
            foreach(ParticleSystem p in m_endscreenConfettiParent.GetComponentsInChildren<ParticleSystem>())
            {
                p.Play();
            }
            m_txtTitle.text = "Du hast gewonnen!";
        }
        else
        {
            m_txtTitle.text = "Du hast verloren!";
            m_boatGameOverController.IsRotating = true;
        }
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        m_btnEndGame.onClick.AddListener(EndGame);
    }

    private void EndGame()
    {
        Application.Quit();
    }
}
