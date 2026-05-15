using Quest;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Dialogue
{
    public enum NPCIconState
    {
        None = 0,
        Talk = 1,
        Quest = 2
    }

    public class NPCInteraction : MonoBehaviour
    {
        [SerializeField] private bool m_showGizmos = true;

        [Header("NPC")]
        [SerializeField] private NPCData m_npc = null;

        [Header("Detection")]
        [SerializeField] private float m_maxBoatSpeed = 0.5f;
        [SerializeField] private float m_visibilityDistance = 20f;

        [Header("References")]
        /*[SerializeField]*/ private Rigidbody m_boatRigidbody = null;
        [SerializeField] private Transform m_boatTransform = null;

        [Header("UI")]
        [SerializeField] private GameObject m_interactionIcon = null;
        [SerializeField] private GameObject m_talkIcon = null;
        [SerializeField] private GameObject m_questIcon = null;

        [Header("Input")]
        [SerializeField] private InputActionReference m_interactAction = null;

        internal NPCData NPC => m_npc;

        private bool m_isPlayerInRange = false;
        private bool m_isInteractable = false;
        private bool m_lastInteractableState = false;
        private NPCIconState m_lastIconState = NPCIconState.None;

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
            NPCIconState _state = GetIconState();
            UpdateIconState(_state); //switching between talkIcon <-> questIcon <-> noIcon
            UpdateInteractionIcon(); //only the interaction icon

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

            NPCDialogueBranch _currentBranch = DialogueStateManager.Instance.GetBestDialogueBranch(m_npc);
            bool _hasDialogue = _currentBranch != null;

            m_isInteractable = m_isPlayerInRange && _isSlowEnough && _hasDialogue;

            if (m_isInteractable)
            {
                //Debug.LogWarning("[QuestInteraction.CheckDockConditions] is interactable");
            }

            //TODO FUTURE: && IsBoatCorrectlyParked
        }

        private void UpdateInteractionIcon()
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

        private NPCIconState GetIconState()
        {
            //1) must be in range
            float _dist = Vector3.Distance(m_boatTransform.position, transform.position);
            if (_dist > m_visibilityDistance)
            {
                return NPCIconState.None;
            }

            NPCDialogueBranch _bestBranch = DialogueStateManager.Instance.GetBestDialogueBranch(m_npc);

            if (_bestBranch != null)
            {
                //1) quest icon
                if (_bestBranch.State == NPCDialogueState.QuestReadyToComplete)
                {
                    return NPCIconState.Quest;
                }

                if (_bestBranch.State == NPCDialogueState.QuestAvailable && !QuestManager.Instance.IsQuestRunning)
                {
                    return NPCIconState.Quest;
                }

                //2) talk icon
                if (_bestBranch.State == NPCDialogueState.ReadyToTalk)
                {
                    return NPCIconState.Talk;
                }
            }

            //if (_bestBranch == null)
            //{
            //    return NPCIconState.None;
            //}

            ////2) talk icon
            //if (_bestBranch.State == NPCDialogueState.ReadyToTalk)
            //{
            //    return NPCIconState.Talk;
            //}

            ////3) quest icon
            //if (_bestBranch.State == NPCDialogueState.QuestAvailable && !QuestManager.Instance.IsQuestRunning)
            //{
            //    return NPCIconState.Quest;
            //}

            //4) all other cases = no icon
            return NPCIconState.None;
        }

        private void UpdateIconState (NPCIconState _state)
        {
            //only react if state actually changed
            if (m_lastIconState == _state)
            {
                return;
            }
            m_lastIconState = _state;

            //reset all
            if (m_talkIcon != null)
            {
                m_talkIcon.SetActive(false);
            }

            if (m_questIcon != null)
            {
                m_questIcon.SetActive(false);
            }

            //enable only current
            switch (_state)
            {
                case NPCIconState.Talk:
                    m_talkIcon?.SetActive(true);
                    break;
                case NPCIconState.Quest:
                    m_questIcon?.SetActive(true);
                    break;
                case NPCIconState.None:
                default:
                    break;
            }
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
