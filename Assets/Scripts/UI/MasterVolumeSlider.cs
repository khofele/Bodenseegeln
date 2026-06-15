using UnityEngine;
using UnityEngine.UI;
using WwiseRTPC = AK.Wwise.RTPC;

public class MasterVolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider m_volumeSlider = null;

    [Header("WWise References")]
    [SerializeField] private WwiseRTPC m_masterVolumeRTPC = null;

    private void Start()
    {
        m_volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void OnEnable()
    {
        SetVolume(PlayerPrefs.GetFloat("MasterVolume"));
        Debug.Log("Master Volume " + PlayerPrefs.GetFloat("MasterVolume"));
    }

    public void SetVolume(float volume)
    {
        // TODO zwischen 0 und 100 oder 0 und 1?
        float clampedVolume = Mathf.Clamp(volume, 0.0f, 100.0f);

        if (m_masterVolumeRTPC != null)
        {
            m_masterVolumeRTPC.SetGlobalValue(clampedVolume);
        }

        m_volumeSlider.value = clampedVolume;

        Debug.Log("Master Volume Save " + PlayerPrefs.GetFloat("MasterVolume"));

        PlayerPrefs.SetFloat("MasterVolume", clampedVolume);
        PlayerPrefs.Save();
    }
}
