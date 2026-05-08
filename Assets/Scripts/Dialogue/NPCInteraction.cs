using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public class NPCInteraction : MonoBehaviour
    {
        [SerializeField] private bool m_showGizmos = true;

        [Header("NPC")]
        [SerializeField] private NPCData m_npc = null;

        [Header("Detection")]
        [SerializeField] private float m_maxBoatSpeed = 0.5f;
        [SerializeField] private float m_visibilityDistance = 20f;

        [Header("References")]
        [SerializeField] private Rigidbody m_boatRigidbody = null;
        [SerializeField] private Transform m_boatTransform = null;

        [Header("UI")]
        [SerializeField] private GameObject m_interactionIcon = null;
        [SerializeField] private GameObject m_possibleQuestIcon = null;

        [Header("Input")]
        [SerializeField] private InputActionReference m_interactAction = null;

        private bool m_isPlayerInRange = false;
        private bool m_isInteractable = false;
        private bool m_lastInteractableState = false;

        private void OnEnable()
        {
            if (m_interactAction != null)
            {
                m_interactAction.action.Enable();
            }

            if (m_boatRigidbody == null)
            {
                m_boatRigidbody = m_boatTransform.GetComponent<Rigidbody>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == m_boatRigidbody)
            {
                m_isPlayerInRange = true;
                Debug.Log("[QuestInteraction.OnTriggerEnter] Boat entered trigger");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody == m_boatRigidbody)
            {
                m_isPlayerInRange = false;
                Debug.Log("[QuestInteraction.OnTriggerExit] Boat left trigger");
            }
        }

        private void Update()
        {
            CheckConditions();
            UpdateUI();
            //UpdatePossibleQuestIcon();

            if (m_isInteractable && m_interactAction.action.WasPressedThisFrame())
            {
                Debug.Log("[QuestInteraction.Update] Try to complete Quest");
                StartDialogue();
            }
        }

        private void CheckConditions()
        {
            if (m_boatRigidbody == null)
            {
                m_boatRigidbody = m_boatTransform.GetComponent<Rigidbody>();
                //return;
            }

            float _speed = m_boatRigidbody.linearVelocity.magnitude;
            bool _isSlowEnough = _speed <= m_maxBoatSpeed;

            m_isInteractable = m_isPlayerInRange && _isSlowEnough;

            if (m_isInteractable)
            {
                //Debug.LogWarning("[QuestInteraction.CheckDockConditions] is interactable");
            }

            //TODO FUTURE: && IsBoatCorrectlyParked
        }

        private void UpdateUI()
        {
            if (m_interactionIcon == null)
            {
                return;
            }

            //check that toggle is only set when state really changed
            if (m_lastInteractableState != m_isInteractable)
            {
                m_lastInteractableState = m_isInteractable;
                m_interactionIcon.SetActive(m_isInteractable);
            }

            if (!m_isInteractable)
            {
                return;
            }

            //set icon above dock
            Camera _cam = Camera.main;
            if (_cam == null)
            {
                return;
            }

            Vector3 _worldPos = transform.position + Vector3.up * 2f;
            Vector3 _screenPos = _cam.WorldToScreenPoint(_worldPos);

            //only set if in front of camera
            if (_screenPos.z > 0)
            {
                m_interactionIcon.transform.position = _screenPos;
            }
        }

        private void StartDialogue()
        {
            if (m_npc == null)
            {
                Debug.LogWarning($"[NPCInteraction] No NPC assigned");
                return;
            }

            Debug.Log($"[NPCInteraction] Start dialogue with {m_npc.Name}");

            DialogueManager.Instance.StartDialogue(m_npc);
        }

        private void UpdatePossibleQuestIcon() //= the icon that is above the NPC to mark that he can give a quest
        {
            //if (m_activeQuestIcon == null)
            //{
            //    return;
            //}

            //bool _questActive = QuestManager.Instance.ActiveQuest == m_quest;
            //if (!_questActive)
            //{
            //    m_activeQuestIcon.SetActive(false);
            //    return;
            //}

            //float _dist = Vector3.Distance(m_boatTransform.position, transform.position);
            //m_activeQuestIcon.SetActive(_dist <= m_visibilityDistance);

        }

        //---------------
        //--- GIZMOS ---
        //---------------
        private void OnDrawGizmos()
        {
            if (!m_showGizmos)
            {
                return;
            }

            Gizmos.color = m_isInteractable ? Color.darkGreen : Color.darkOrange;

            BoxCollider _col = GetComponent<BoxCollider>();
            if (_col == null)
            {
                return;
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_col.center, _col.size);
        }
    }
}
