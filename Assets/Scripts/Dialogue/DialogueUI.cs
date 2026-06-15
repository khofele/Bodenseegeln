using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject m_root;
        [SerializeField] private TextMeshProUGUI m_nameText;
        [SerializeField] private TextMeshProUGUI m_dialogueText;
        [SerializeField] private Image m_portrait;
        [SerializeField] private Image m_background;

        [Header("Responses")]
        [SerializeField] private Transform m_buttonContainer;
        [SerializeField] private Button m_buttonPrefab;

        [Header("Typing")]
        [SerializeField] private float m_typingSpeed = 0.02f;
        [SerializeField] private InputActionReference m_scipAction = null;

        private Coroutine m_typingRoutine;
        private bool m_skipTyping;

        private void OnEnable()
        {
            if (m_scipAction != null)
            {
                m_scipAction.action.Enable();
            }
        }

        internal void Show(bool _show)
        {
            m_root.SetActive(_show);
        }

        internal void DisplayNode(NPCData _npc, DialogueNode _node)
        {
            m_nameText.text = _npc.Name;
            m_portrait.sprite = _npc.Portrait;
            m_background.sprite = _npc.Background;

            if (m_typingRoutine != null)
            {
                StopCoroutine(m_typingRoutine);
            }

            m_typingRoutine = StartCoroutine(TypeText(_node.Text));
            CreateResponseButtons(_node);
        }

        private IEnumerator TypeText(string _text)
        {
            m_skipTyping = false;
            m_dialogueText.text = "";

            foreach (char c in _text)
            {
                if (m_skipTyping)
                {
                    m_dialogueText.text = _text;
                    yield break;
                }

                m_dialogueText.text += c;
                yield return new WaitForSeconds(m_typingSpeed);
            }
        }

        internal void SkipTyping()
        {
            m_skipTyping = true;
        }

        private void CreateResponseButtons(DialogueNode _node)
        {
            //clear old buttons
            foreach (Transform child in m_buttonContainer)
            {
                Destroy(child.gameObject);
            }

            List<DialogueResponse> _responses = _node.GetResponses();

            foreach (DialogueResponse response in _responses)
            {
                Button _button = Instantiate(m_buttonPrefab, m_buttonContainer);

                TextMeshProUGUI _text = _button.GetComponentInChildren<TextMeshProUGUI>();
                _text.text = response.Text;
                _button.onClick.AddListener(() =>
                {
                    DialogueManager.Instance.ChooseResponse(response);
                });
            }
        }

        private void Update()
        {
            //if changes at input system needed, then change here
            if (m_scipAction.action.triggered)
            {
                SkipTyping();
            }
        }
    }
}
