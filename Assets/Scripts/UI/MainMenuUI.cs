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
    [SerializeField] private Slider m_sliderVolume = null;

    [Header("WWise References")]
    [SerializeField] private WwiseRTPC m_masterVolumeRTPC = null;

    private void StartGame()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void EndGame()
    {
        Application.Quit();
    }

    private void SetVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp(volume, 0.0f, 100.0f);

        if(m_masterVolumeRTPC != null)
        {
            m_masterVolumeRTPC.SetGlobalValue(clampedVolume);
        }

        // TODO Lautstärkewert global speichern für alle Menüs
    }

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        m_btnStartGame.onClick.AddListener(StartGame);
        m_btnEndGame.onClick.AddListener(EndGame);
        m_sliderVolume.onValueChanged.AddListener((float volume) => SetVolume(volume));
    }
}
