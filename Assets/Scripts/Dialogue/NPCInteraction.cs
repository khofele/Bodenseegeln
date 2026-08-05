using UnityEngine;
using UnityEngine.InputSystem;

using Quest;

namespace Dialogue
{
    public enum NPCIconState
    {
        None = 0,
        Talk = 1,
        Quest = 2
    }

    public class NPCInteraction : CompassTargetBase //inherits from CompassTargetBase to be used as icons on the compass
    {
        [SerializeField] private bool m_showGizmos = true;

        [Header("NPC")]
        [SerializeField] private NPCData m_npc = null;

        [Header("Detection")]
        [SerializeField] private float m_maxBoatSpeed = 0.5f;
        [SerializeField] private float m_visibilityDistance = 20f;

        [Header("Parking")]
        [SerializeField] private bool m_requireCorrectParking = false;
        [SerializeField] private float m_allowedAngleTolerance = 25f;

        [Header("Parking Zone")]
        [SerializeField] private ParkingZoneVisualizer m_parkingZone = null;
        [SerializeField] private float m_zoneVisibilityDistance = 40f;

        [Header("References")]
        [SerializeField] private Transform m_boatTransform = null;

        [Header("UI")]
        [SerializeField] private GameObject m_interactionIcon = null;
        [SerializeField] private GameObject m_talkIcon = null;
        [SerializeField] private GameObject m_questIcon = null;

        [Header("Input")]
        [SerializeField] private InputActionReference m_interactAction = null;

        internal NPCData NPC => m_npc;


        private Rigidbody m_boatRigidbody = null;
        private bool m_isPlayerInRange = false;
        private bool m_isInteractable = false;
        private bool m_lastInteractableState = false;
        private NPCIconState m_lastIconState = NPCIconState.None;


        //worldposition for positioning on compass
        public override Vector2 GetPosition()
        {
            Vector3 p = transform.position;
            return new Vector2(p.x, p.z);
        }

        //if an icon should be shown on the compass
        public override bool ShouldShowIcon()
        {
            return GetCompassIconState() != NPCIconState.None;
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
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.attachedRigidbody == m_boatRigidbody)
            {
                m_isPlayerInRange = true;

                //for playing sound when entering the triggerbox
                bool _playSound = (DialogueStateManager.Instance.GetBestDialogueBranch(m_npc)) != null && GameManager.Instance.CurrentState != GameStates.DIALOGMODE;
                if (_playSound)
                {
                    QuestManager.Instance.MissionFeedbackAudio.PlayGoalReached(); //Audio play enter triggerbox sound
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.attachedRigidbody == m_boatRigidbody)
            {
                m_isPlayerInRange = false;
            }
        }

        private void Update()
        {
            CheckConditions();
            NPCIconState _state = GetIconState();
            UpdateIconState(_state); //switching between talkIcon <-> questIcon <-> noIcon
            UpdateInteractionIcon(); //only the interaction icon
            UpdateParkingZone();

            if (m_isInteractable && m_interactAction.action.WasPressedThisFrame())
            {
                StartDialogue();
            }
        }

        //check if boat is close enough and slow enough and correctly parked (if it requires correct parking) and if the NPC has a valid branch = if the NPC is curretly ready to talk
        private void CheckConditions()
        {
            if (m_boatRigidbody == null)
            {
                m_boatRigidbody = m_boatTransform.GetComponent<Rigidbody>();
            }

            float _speed = m_boatRigidbody.linearVelocity.magnitude;
            bool _isSlowEnough = _speed <= m_maxBoatSpeed;
            bool _isCorrectlyParked = IsBoatCorrectlyParked();

            NPCDialogueBranch _currentBranch = DialogueStateManager.Instance.GetBestDialogueBranch(m_npc);
            bool _hasDialogue = _currentBranch != null;

            m_isInteractable = m_isPlayerInRange && _isSlowEnough && _isCorrectlyParked && _hasDialogue;
        }

        //park in direction of the parking slot or 180° rotated
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

        private void UpdateParkingZone()
        {
            if (m_parkingZone == null)
            {
                return;
            }

            float _dist = Vector3.Distance(m_boatTransform.position, transform.position);
            bool _visible = _dist <= m_zoneVisibilityDistance && GetCompassIconState() != NPCIconState.None;

            m_parkingZone.SetVisible(_visible);

            if (!_visible)
            {
                return;
            }

            m_parkingZone.SetCorrectParking(m_isInteractable);
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

        //what icon should be visualized on compass and map, quest or talk or none
        internal NPCIconState GetCompassIconState()
        {
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

            return NPCIconState.None;
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
            //must be in range
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

            //3) all other cases = no icon
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
