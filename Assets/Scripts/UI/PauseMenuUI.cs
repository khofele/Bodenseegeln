using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    public static event Action OnResumeButtonClicked;

    [Header("UI Components")]
    [SerializeField] private Button m_btnResumeGame = null;
    [SerializeField] private Button m_btnEndGame = null;

    private void EndGame()
    {
        Application.Quit();
    }

    private void Start()
    {
        m_btnResumeGame.onClick.AddListener(() => OnResumeButtonClicked?.Invoke());
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
