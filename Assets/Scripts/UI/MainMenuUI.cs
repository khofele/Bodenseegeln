using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WwiseRTPC = AK.Wwise.RTPC;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button m_btnStartGame = null;
    [SerializeField] private Button m_btnEndGame = null;

    [Header("Audio Controller")]
    [SerializeField] private MenuAudioController m_menuAudioController = null;

    private void StartGame()
    {
        m_menuAudioController.PlayStartGame();
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void EndGame()
    {
        m_menuAudioController.PlayEndGame();
        Application.Quit();
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        m_btnStartGame.onClick.AddListener(StartGame);
        m_btnEndGame.onClick.AddListener(EndGame);
    }

    private void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;

        m_menuAudioController.StartMenuAmbient();
    }

    private void OnDisable()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        m_menuAudioController.StopMenuAmbient();
    }
}
