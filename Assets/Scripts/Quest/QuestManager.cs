using Dialogue;
using System.Collections.Generic;
using UnityEngine;
using Dialogue;

namespace Quest
{
    public enum QuestState
    {
        None = 0,
        Offered = 1,
        Active = 2,
        Completed = 3,
        Failed = 4
    }

    public struct QuestResult
    {
        public QuestData Quest;

        public float TimeCircles;
        public float DamageCircles;
        public float FuelCircles;

        public float AverageCircles;

        public float TimeValue;
        public float DamageValue;
        public float FuelValue;

        public int MoneyReward;
    }

    public class QuestManager : Manager<QuestManager>
    {
        [Header("References")]
        [SerializeField] private QuestResultUI m_questResultUI = null;
        [SerializeField] private NotificationTextUI m_notificationTextUI = null;
        [SerializeField] private MissionsFeedbackAudioController m_missionsFeedbackAudioController = null;  // Audio class reference

        private QuestData m_activeQuest = null;
        private QuestData m_pendingQuest = null;
        private bool m_isQuestRunning = false;
        private int m_currentQuestStep = 0;
        private bool m_type2ReadyToComplete = false;
        private Transform m_currentQuestTarget = null;
        private QuestState m_state = QuestState.None;
        private bool m_pendingGameWon = false;
        
        //private float m_questStartTime;

        //---Tracking---
        private float m_timeElapsed;
        private float m_totalDamage;
        private float m_totalFuelUsed;
        //private float m_lastBoatHealth; //for delta tracking
        //private float m_lastFuel; //for delta tracking

        private HashSet<QuestData> m_completedQuests = new();

        public bool HasActiveQuest => m_activeQuest != null;
        public QuestData ActiveQuest => m_activeQuest;
        public bool IsQuestRunning => m_isQuestRunning;
        public QuestState State => m_state;
        public int CurrentQuestStep => m_currentQuestStep;
        public MissionsFeedbackAudioController MissionFeedbackAudio => m_missionsFeedbackAudioController;


        private void Update()
        {
            CheckPendingGameWon();

            if (!m_isQuestRunning)
            {
                return;
            }

            TrackTime();
        }

        private void TrackTime()
        {
            //maybe change later if bad for performance
            m_timeElapsed += Time.deltaTime;
        }

        internal float GetMissionTime()
        {
            return m_timeElapsed;
        }

        internal Transform GetCurrentQuestTarget()
        {
            return m_currentQuestTarget;
            //if (!m_isQuestRunning || m_activeQuest == null)
            //{
            //    return null;
            //}

            //switch (m_activeQuest.QuestType)
            //{
            //    case QuestType.Type1_DialogueOtherNPC:
            //    case QuestType.Type2_DialogueSameNPC:
            //    case QuestType.Type3_DirectCompletion:
            //        return GetQuestInteractionTarget();
            //    case QuestType.Type4_MultiStep:
            //        if (CanCompleteQuest(m_activeQuest))
            //        {
            //            return GetQuestGiverTarget();
            //        }
            //        return GetQuestInteractionTarget();
            //    default:
            //        return null;
            //}
        }

        private void RefreshQuestTarget()
        {
            m_currentQuestTarget = null;

            if (!m_isQuestRunning || m_activeQuest == null)
            {
                return;
            }

            switch (m_activeQuest.QuestType)
            {
                case QuestType.Type1_DialogueOtherNPC:
                case QuestType.Type2_DialogueSameNPC:
                case QuestType.Type3_DirectCompletion:
                    m_currentQuestTarget = GetQuestInteractionTarget();
                    break;
                case QuestType.Type4_MultiStep:
                    if (CanCompleteQuest(m_activeQuest))
                    {
                        m_currentQuestTarget = GetQuestGiverTarget();
                    }
                    else
                    {
                        m_currentQuestTarget = GetNextIncompleteQuestInteractionTarget();
                    }
                    break;
            }
        }

        private Transform GetQuestInteractionTarget()
        {
            QuestInteraction[] _interactions = FindObjectsByType<QuestInteraction>(FindObjectsSortMode.None);

            for (int i = 0; i < _interactions.Length; i++)
            {
                if (_interactions[i] == null)
                {
                    continue;
                }

                if (_interactions[i].Quest == m_activeQuest)
                {
                    return _interactions[i].transform;
                }
            }

            return null;
        }

        private Transform GetNextIncompleteQuestInteractionTarget()
        {
            QuestInteraction[] _interactions = FindObjectsByType<QuestInteraction>(FindObjectsSortMode.None);

            for (int i = 0; i < _interactions.Length; i++)
            {
                if (_interactions[i] == null)
                {
                    continue;
                }

                if (_interactions[i].Quest != m_activeQuest)
                {
                    continue;
                }

                if (_interactions[i].HasBeenUsed)
                {
                    continue;
                }

                return _interactions[i].transform;
            }

            return null;
        }

        private Transform GetQuestGiverTarget()
        {
            NPCInteraction[] _interactions = FindObjectsByType<NPCInteraction>(FindObjectsSortMode.None);

            for (int i = 0; i < _interactions.Length; i++)
            {
                if (_interactions[i] == null)
                {
                    continue;
                }

                if (_interactions[i].NPC == m_activeQuest.QuestGiverNPC)
                {
                    return _interactions[i].transform;
                }
            }

            return null;
        }

        //call this as "QuestManager.Instance.RegisterDamage(damage);" where the damage is handled
        internal void RegisterDamage(float _damageAmount)
        {
            if (!m_isQuestRunning)
            {
                return;
            }

            m_totalDamage += _damageAmount;

            Debug.Log($"[QuestManager] Damage registered: {_damageAmount} | Total: {m_totalDamage}");
        }

        //call this as "QuestManager.Instance.RegisterFuelUsed(amount);" where the fuel usage is handled
        internal void RegisterFuelUsed(float _fuelAmount)
        {
            if (!m_isQuestRunning)
            {
                return;
            }

            m_totalFuelUsed += _fuelAmount;

            Debug.Log($"[QuestManager] Damage registered: {_fuelAmount} | Total: {m_totalFuelUsed}");
        }

        internal bool IsQuestCompleted(QuestData _quest)
        {
            return m_completedQuests.Contains(_quest);
        }

        internal IEnumerable<QuestData> GetCompletedQuests() //was List<QuestData> but IEnumerable with foreach is better for performance
        {
            return m_completedQuests;
            //list is worse for performance
            //return new List<QuestData>(m_completedQuests);
        }

        internal bool CanStartQuest(QuestData _quest) //later also use for DialogueSystem
        {
            return m_activeQuest == null && m_pendingQuest == null;
        }

        internal void SetPendingQuest(QuestData _quest)
        {
            if (_quest == null)
            {
                Debug.LogWarning("[QuestManager] Tried to set NULL pending quest");
                return;
            }

            if (!CanStartQuest(_quest))
            {
                Debug.LogWarning("[QuestManager] Already has active or pending quest");
                return;
            }

            m_pendingQuest = _quest;

            Debug.Log($"[QuestManager] Pending quest set: {_quest.QuestName}");
        }

        internal void ConfirmPendingQuest()
        {
            if (m_pendingQuest == null)
            {
                return;
            }

            StartQuest(m_pendingQuest);
            m_pendingQuest = null;
        }

        internal void StartQuest(QuestData _quest)
        {
            if (_quest == null)
            {
                Debug.LogWarning("[QuestManager] Tried to start NULL quest");
                return;
            }

            if (m_activeQuest != null)
            {
                Debug.LogWarning("[QuestManager] Already has active quest");
                return;
            }

            m_activeQuest = _quest;
            m_isQuestRunning = true;
            m_state = QuestState.Active;
            m_currentQuestStep = 0;
            m_type2ReadyToComplete = false;
            m_missionsFeedbackAudioController.PlayMissionAccept();  // Mission Accept Play Audio

            RefreshQuestTarget();

            //reset tracking
            m_timeElapsed = 0f;
            m_totalDamage = 0f;
            m_totalFuelUsed = 0f;
            ////TEMP: replace once real system exist
            //m_lastBoatHealth = 100f;
            //m_lastFuel = 100f;

            Debug.Log($"[QuestManager] Quest started: {_quest.QuestName}");
        }

        internal void HandleQuestInteraction(QuestData _quest, QuestInteraction _interaction)
        {
            if (m_activeQuest != _quest)
            {
                Debug.Log("[QuestManager] Interaction ignored (not active quest)");
                return;
            }

            Debug.Log($"[QuestManager] Handling interaction for quest: {_quest.QuestName}");

            switch (_quest.QuestType)
            {
                case QuestType.Type1_DialogueOtherNPC:
                    HandleType1_DialogueOtherNPC();
                    break;
                case QuestType.Type2_DialogueSameNPC:
                    HandleType2_DialogueSameNPC();
                    break;
                case QuestType.Type3_DirectCompletion:
                    HandleType3_DirectCompletion();
                    break;
                case QuestType.Type4_MultiStep:
                    HandleType4_MultiStep(_interaction);
                    break;
                default:
                    Debug.LogWarning("[QuestManager] Quest type not implemented yet");
                    break;
            }
        }

        private void HandleType1_DialogueOtherNPC()
        {
            Debug.Log("[QuestManager] TYPE 1: Start dialogue with target NPC");

            if (m_activeQuest.TargetNPC == null)
            {
                Debug.LogError("[QuestManager] Target NPC is NULL");
                return;
            }

            DialogueManager.Instance.StartDialogue(m_activeQuest.TargetNPC);
        }

        private void HandleType2_DialogueSameNPC()
        {
            Debug.Log("[QuestManager] TYPE 2: Start dialogue with quest giver NPC");

            if (m_activeQuest.QuestGiverNPC == null)
            {
                Debug.LogError("[QuestManager] Quest giver NPC is NULL");
                return;
            }

            m_type2ReadyToComplete = true;
            RefreshQuestTarget();
            Debug.Log("[QuestManager] Type 2 set to QuestReadyToComplete");

            DialogueManager.Instance.StartDialogue(m_activeQuest.QuestGiverNPC);
        }

        private void HandleType3_DirectCompletion()
        {
            Debug.Log("[QuestManager] TYPE 3: Direct completion");
            QuestResult _result = FinishQuestAndGetResult();
            ShowResultUI(_result);
        }

        private void HandleType4_MultiStep(QuestInteraction _interaction)
        {
            Debug.Log("[QuestManager] TYPE 4: Step interaction");

            if (_interaction == null)
            {
                Debug.LogWarning("[QuestManager] NULL interaction");
                return;
            }

            _interaction.MarkAsCompleted();

            m_currentQuestStep++;
            RefreshQuestTarget();
            ShowStepNotification();

            Debug.Log($"[QuestManager] Quest step progressed: {m_currentQuestStep}/{m_activeQuest.QuestSteps}");

            if (m_currentQuestStep >= m_activeQuest.QuestSteps)
            {
                RefreshQuestTarget();
                Debug.Log("[QuestManager] TYPE 4: Final step reached -> go to quest giver NPC");
            }
        }

        internal bool CanCompleteQuest(QuestData _quest)
        {
            if (m_activeQuest != _quest)
            {
                return false;
            }

            switch (_quest.QuestType)
            {
                case QuestType.Type4_MultiStep:
                    return m_currentQuestStep >= m_activeQuest.QuestSteps;
                case QuestType.Type2_DialogueSameNPC:
                    return m_type2ReadyToComplete;
                default:
                    return false;
            }
        }

        internal void CompleteQuestFromDialogue()
        {
            if (m_activeQuest == null || !m_isQuestRunning)
            {
                Debug.LogWarning("[QuestManager] No active quest to complete from dialogue");
                return;
            }

            Debug.Log("[QuestManager] Completing quest from dialogue");

            QuestResult _result = FinishQuestAndGetResult();
            ShowResultUI(_result);
        }

        internal QuestResult FinishQuestAndGetResult()
        {
            if (m_activeQuest == null || !m_isQuestRunning)
            {
                Debug.LogWarning("[QuestManager] No active quest to complete");
                return default;
            }

            float _timeValue = m_timeElapsed;
            float _damageValue = m_totalDamage; // + 40f; //TEMP: added value for testing
            float _fuelValue = m_totalFuelUsed; // + 3f; //TEMP: added value for testing

            float _timeCircles = CalculateCircles(_timeValue, m_activeQuest.m_timeRange);
            float _damageCircles = CalculateCircles(_damageValue, m_activeQuest.m_damageRange);
            float _fuelCircles = CalculateCircles(_fuelValue, m_activeQuest.m_fuelRange);

            float _averageCircles = (_timeCircles + _damageCircles + _fuelCircles) / 3f;
            float _moneyNormalized = _averageCircles / 5f;
            int _money = Mathf.RoundToInt(Mathf.Lerp(m_activeQuest.m_moneyRange.zeroCircleValue, m_activeQuest.m_moneyRange.fiveCircleValue, _moneyNormalized));

            QuestResult _result = new QuestResult
            {
                Quest = m_activeQuest,
                
                TimeCircles = _timeCircles,
                DamageCircles = _damageCircles,
                FuelCircles = _fuelCircles,

                AverageCircles = _averageCircles,

                TimeValue = _timeValue,
                DamageValue = _damageValue,
                FuelValue = _fuelValue,

                MoneyReward = _money
            };

            Debug.Log($"[QuestManager] Quest finished: {m_activeQuest.QuestName}");
            m_missionsFeedbackAudioController.PlayMissionComplete();  // Play Audio Mission Complete

            QuestData _completedQuest = m_activeQuest;
            m_completedQuests.Add(m_activeQuest);
            GameManager.Instance.RegisterQuestResult(m_activeQuest, _averageCircles);

            if (_completedQuest.IsFinalQuest)
            {
                m_pendingGameWon = true;
                Debug.Log("[QuestManager] Final quest completed");
            }

            m_activeQuest = null;
            m_isQuestRunning = false;
            m_currentQuestTarget = null;

            return _result;
        }

        private float CalculateCircles(float _value, RewardRange _range)
        {
            float _normalized = Mathf.InverseLerp(_range.zeroCircleValue, _range.fiveCircleValue, _value);

            return Mathf.Clamp01(_normalized) * 5f;
        }

        private void CheckPendingGameWon()
        {
            if (!m_pendingGameWon)
            {
                return;
            }

            GameStates _state = GameManager.Instance.CurrentState;

            if (_state != GameStates.MOTORMODE && _state != GameStates.SAILMODE)
            {
                return;
            }

            m_pendingGameWon = false;

            Debug.Log("[QuestManager] Final quet finished -> GAME WON");

            GameManager.Instance.SetState(GameStates.GAMEWON);
        }

        private void ShowStepNotification()
        {
            if (m_notificationTextUI == null)
            {
                return;
            }

            if (m_activeQuest == null)
            {
                return;
            }

            int _stepIndex = m_currentQuestStep - 1;

            if (_stepIndex < 0 || _stepIndex >= m_activeQuest.StepTexts.Count)
            {
                Debug.LogWarning("[QuestManager] missing step text");
                return;
            }

            string _text = m_activeQuest.StepTexts[_stepIndex].m_text;
            Debug.Log($"!!![QuestManager] text = {_text}");
            Debug.Log($"!!![QuestManager] jetzt call notificationTextUI.Show");
            m_notificationTextUI.Show(_text);
        }

        private void ShowResultUI(QuestResult _result)
        {
            if (m_questResultUI == null)
            {
                Debug.LogError("[QuestManager] No QuestResultUI assigned");
                return;
            }

            m_questResultUI.Show(_result);
        }
    }
}
