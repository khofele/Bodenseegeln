using UnityEngine;
using TMPro;
using System.Collections;


public class NotificationTextUI : MonoBehaviour
{
    //short notifiction text that is visible for a short amount of time

    [Header("Root")]
    [SerializeField] private GameObject m_root;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI m_text;

    [Header("Settings")]
    [SerializeField] private float m_showDuration = 3f;

    private Coroutine m_hideRoutine;

    internal void Show(string _message)
    {
        Debug.Log($"!!![NotificationTextUI] Show called");
        if (m_root == null || m_text == null)
        {
            Debug.LogWarning("[NotificationTextUI] missing references");
            return;
        }

        if (m_hideRoutine != null)
        {
            StopCoroutine(m_hideRoutine);
        }

        m_text.text = _message;
        m_root.SetActive(true);

        m_hideRoutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(m_showDuration);
        m_root.SetActive(false);
    }
}
