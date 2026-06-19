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
        [Header("Audio References")]
        [SerializeField] private MissionsFeedbackAudioController m_missionsFeedbackAudioController = null;  // Audio class reference

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

        //displays text of node letter by letter, can be sciped by ScipTyping method
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

                m_missionsFeedbackAudioController.PlayMissionVoiceLineNpc(); //Audio play typing sound with voice lines

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

            //instantiate button prefab, add the text from the NPCData for this reponse, add onClick action
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
            //when scipAction is triggered then typing stops and text is immeditely fully shown
            if (m_scipAction.action.triggered)
            {
                SkipTyping();
            }
        }
    }
}
