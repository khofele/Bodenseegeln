using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WwiseRTPC = AK.Wwise.RTPC;

public class PauseMenuUI : MonoBehaviour
{
    public static event Action OnResumeButtonClicked;

    [Header("UI Components")]
    [SerializeField] private Button m_btnResumeGame = null;
    [SerializeField] private Button m_btnBackToMainMenu = null;
    [SerializeField] private Slider m_sliderVolume = null;

    [Header("WWise References")]
    [SerializeField] private WwiseRTPC m_masterVolumeRTPC = null;

    private void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void SetVolume(float volume)
    {
        float clampedVolume = Mathf.Clamp(volume, 0.0f, 100.0f);

        if (m_masterVolumeRTPC != null)
        {
            m_masterVolumeRTPC.SetGlobalValue(clampedVolume);
        }

        // TODO Lautstärkewert global speichern für alle Menüs
    }

    private void Start()
    {
        m_btnResumeGame.onClick.AddListener(() => OnResumeButtonClicked?.Invoke());
        m_btnBackToMainMenu.onClick.AddListener(BackToMainMenu);
        m_sliderVolume.onValueChanged.AddListener((float volume) => SetVolume(volume));
    }

    public void OnEnable()
    {
        Debug.Log("Pausem ON");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void OnDisable()
    {
        Debug.Log("Pausem OFF");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
