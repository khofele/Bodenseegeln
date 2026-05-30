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

        [Header("Parking")]
        [SerializeField] private bool m_requireCorrectParking = false;
        [SerializeField] private float m_allowedAngleTolerance = 25f;

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
            bool _isCorrectlyParked = IsBoatCorrectlyParked();

            m_isInteractable = !m_hasBeenUsed && m_isPlayerInRange && _isSlowEnough && _isCorrectlyParked
                && QuestManager.Instance.IsQuestRunning && QuestManager.Instance.ActiveQuest == m_quest;

            if (m_isInteractable)
            {
                //Debug.LogWarning("[QuestInteraction.CheckDockConditions] is interactable");
            }

            //TODO FUTURE: && IsBoatCorrectlyParked
        }

        private bool IsBoatCorrectlyParked()
        {
            if (!m_requireCorrectParking)
            {
                return true;
            }

            if (m_boatTransform == null)
            {
                return false;
            }

            float _angle = Vector3.SignedAngle(transform.forward, m_boatTransform.forward, Vector3.up);
            _angle = Mathf.Abs(_angle);

            bool _isForwardCorrect = _angle <= m_allowedAngleTolerance;
            bool _isBackwardCorrect = Mathf.Abs(_angle - 180f) <= m_allowedAngleTolerance;

            return _isForwardCorrect || _isBackwardCorrect;
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

            //trigger box
            Gizmos.color = m_isInteractable ? Color.darkGreen : Color.darkOrange;

            BoxCollider _col = GetComponent<BoxCollider>();
            if (_col != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(_col.center, _col.size);
            }

            //parking direction visualization
            if (!m_requireCorrectParking)
            {
                return;
            }

            Gizmos.matrix = Matrix4x4.identity;

            Vector3 _origin = transform.position + Vector3.up * 0.25f;
            float _lineLength = 2f;

            //forward direction
            Vector3 _forward = transform.forward * _lineLength;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(_origin, _origin + _forward);

            //forward arrow head
            Vector3 _arrowRight = Quaternion.Euler(0f, 150f, 0f) * transform.forward * 0.4f;
            Vector3 _arrowLeft = Quaternion.Euler(0f, -150f, 0f) * transform.forward * 0.4f;

            Gizmos.DrawLine(_origin + _forward, _origin + _forward + _arrowRight);
            Gizmos.DrawLine(_origin + _forward, _origin + _forward + _arrowLeft);

            //backward direction (valid too)
            Vector3 _backward = -transform.forward * _lineLength;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(_origin, _origin + _backward);

            //tolerance visualization
            Gizmos.color = Color.yellow;

            Vector3 _forwardLeft = Quaternion.Euler(0f, -m_allowedAngleTolerance, 0f) * transform.forward * _lineLength;
            Vector3 _forwardRight = Quaternion.Euler(0f, m_allowedAngleTolerance, 0f) * transform.forward * _lineLength;
            Vector3 _backwardLeft = Quaternion.Euler(0f, 180f - m_allowedAngleTolerance, 0f) * transform.forward * _lineLength;
            Vector3 _backwardRight = Quaternion.Euler(0f, 180f + m_allowedAngleTolerance, 0f) * transform.forward * _lineLength;

            Gizmos.DrawLine(_origin, _origin + _forwardLeft);
            Gizmos.DrawLine(_origin, _origin + _forwardRight);

            Gizmos.DrawLine(_origin, _origin + _backwardLeft);
            Gizmos.DrawLine(_origin, _origin + _backwardRight);
        }
    }
}
