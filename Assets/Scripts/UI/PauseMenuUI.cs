using System;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public static event Action OnResumeButtonClicked;

    [Header("UI Components")]
    [SerializeField] private Button m_btnResumeGame = null;
    [SerializeField] private Button m_btnEndGame = null;

    [Header("Audio Controller")]
    [SerializeField] private MenuAudioController m_menuAudioController = null;

    private void EndGame()
    {
        m_menuAudioController.PlayEndGame();
        Application.Quit();
    }

    private void Start()
    {
        m_btnResumeGame.onClick.AddListener(() => OnResumeButtonClicked?.Invoke());
        m_btnResumeGame.onClick.AddListener(m_menuAudioController.PlayResumeGame);

        m_btnEndGame.onClick.AddListener(EndGame);
    }

    private void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnDisable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
