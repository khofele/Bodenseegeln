using UnityEngine;
using UnityEngine.InputSystem;

namespace Quest
{
    public class QuestInteraction : CompassTargetBase
    {
        [SerializeField] private bool m_showGizmos = true;

        [Header("Quest")]
        [SerializeField] private QuestData m_quest = null;

        [Header("Detection")]
        [SerializeField] private float m_maxBoatSpeed = 0.5f;
        [SerializeField] private float m_visibilityDistance = 20f;

        [Header("References")]
        /*[SerializeField]*/ private Rigidbody m_boatRigidbody = null;
        [SerializeField] private Transform m_boatTransform = null;

        [Header("UI")]
        [SerializeField] private GameObject m_interactionIcon = null;
        [SerializeField] private GameObject m_activeQuestIcon = null;

        [Header("Input")]
        [SerializeField] private InputActionReference m_interactAction = null;

        internal QuestData Quest => m_quest;
        internal bool HasBeenUsed => m_hasBeenUsed;

        private bool m_isPlayerInRange = false;
        private bool m_isInteractable = false;
        private bool m_lastInteractableState = false;
        private bool m_hasBeenUsed = false;

        public override Vector2 GetPosition()
        {
            Vector3 p = transform.position;
            return new Vector2(p.x, p.z);
        }

        public override bool ShouldShowIcon()
        {
            return ShouldShowCompassIcon();
        }

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

            if (Compass.Instance != null)
            {
                Compass.Instance.Register(this);
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
            UpdateActiveQuestIcon();

            if (m_isInteractable && m_interactAction.action.WasPressedThisFrame())
            {
                Debug.Log("[QuestInteraction.Update] Try to complete Quest");
                TryCompleteQuest();
            }
        }

        private void CheckConditions()
        {
            if (m_boatRigidbody == null)
            {
                m_boatRigidbody = m_boatTransform.GetComponent<Rigidbody>();
            }

            float _speed = m_boatRigidbody.linearVelocity.magnitude;
            bool _isSlowEnough = _speed <= m_maxBoatSpeed;

            m_isInteractable = !m_hasBeenUsed && m_isPlayerInRange && _isSlowEnough 
                && QuestManager.Instance.IsQuestRunning && QuestManager.Instance.ActiveQuest == m_quest;

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

        private void UpdateActiveQuestIcon() //= the icon that is above the active quest destination to mark the quest target
        {
            if (m_activeQuestIcon == null)
            {
                return;
            }

            bool _questActive = !m_hasBeenUsed && QuestManager.Instance.ActiveQuest == m_quest;
            if (!_questActive)
            {
                m_activeQuestIcon.SetActive(false);
                return;
            }

            float _dist = Vector3.Distance(m_boatTransform.position, transform.position);
            m_activeQuestIcon.SetActive(_dist <= m_visibilityDistance);
        }

        internal bool ShouldShowCompassIcon()
        {
            return !m_hasBeenUsed && QuestManager.Instance.ActiveQuest == m_quest;
        }

        private void TryCompleteQuest()
        {
            if (!QuestManager.Instance.IsQuestRunning)
            {
                Debug.Log("[QuestInteraction] No active quest");
                return;
            }

            if (QuestManager.Instance.ActiveQuest != m_quest)
            {
                Debug.Log("[QuestInteraction] Wrong quest");
                return;
            }

            Debug.Log("[QuestInteraction] Quest interaction notified QuestManager about interaction");

            QuestManager.Instance.HandleQuestInteraction(m_quest, this);
        }

        internal void MarkAsCompleted()
        {
            m_hasBeenUsed = true;

            if (m_interactionIcon != null)
            {
                m_interactionIcon.SetActive(false);
            }

            if (m_activeQuestIcon != null)
            {
                m_activeQuestIcon.SetActive(false);
            }

            Debug.Log("[QuestInteraction] interaction completed and disabled");
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
