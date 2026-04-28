using Dialogue;
using UnityEngine;

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

        public int TimeStars;
        public int DamageStars;
        public int FuelStars;

        public float TimeValue;
        public float DamageValue;
        public float FuelValue;

        public int AverageStars;
        public int MoneyReward;
    }

    public class QuestManager : Manager<QuestManager>
    {
        [Header("References")]
        [SerializeField] private QuestResultUI m_questResultUI = null;
        private QuestData m_activeQuest = null;
        private QuestData m_pendingQuest = null;
        private bool m_isQuestRunning = false;
        private int m_currentQuestStep = 0;
        private QuestState m_state = QuestState.None;
        
        //private float m_questStartTime;

        //---Tracking---
        private float m_timeElapsed;
        private float m_totalDamage;
        private float m_totalFuelUsed;
        private float m_lastBoatHealth; //for delta tracking
        private float m_lastFuel; //for delta tracking

        public bool HasActiveQuest => m_activeQuest != null;
        public QuestData ActiveQuest => m_activeQuest;
        public bool IsQuestRunning => m_isQuestRunning;
        public QuestState State => m_state;
        public int CurrentQuestStep => m_currentQuestStep;


        private void Update()
        {
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

            //m_questStartTime = Time.time;

            //reset tracking
            m_timeElapsed = 0f;
            m_totalDamage = 0f;
            m_totalFuelUsed = 0f;
            //TEMP: replace once real system exist
            m_lastBoatHealth = 100f;
            m_lastFuel = 100f;

            Debug.Log($"[QuestManager] Quest started: {_quest.QuestName}");
        }

        //internal void CompleteQuest()
        //{
        //    if (m_activeQuest == null || !m_isQuestRunning)
        //    {
        //        Debug.LogWarning("[QuestManager] No active quest to complete");
        //        return;
        //    }

        //    Debug.Log($"[QuestManager] Quest completed: {m_activeQuest.QuestName}");

        //    m_activeQuest = null;
        //    m_isQuestRunning = false;
        //    m_state = QuestState.Completed;
        //}

        internal void HandleQuestInteraction(QuestData _quest)
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
                    HandleType4_MultiStep();
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

            DialogueManager.Instance.StartDialogue(m_activeQuest.QuestGiverNPC);
        }

        private void HandleType3_DirectCompletion()
        {
            Debug.Log("[QuestManager] TYPE 3: Direct completion");
            QuestResult _result = FinishQuestAndGetResult();
            ShowResultUI(_result);
        }

        private void HandleType4_MultiStep()
        {
            Debug.Log("[QuestManager] TYPE 4: Step interaction");

            m_currentQuestStep++;

            Debug.Log($"[QuestManager] Quest step progressed: {m_currentQuestStep}");

            if (m_currentQuestStep >= m_activeQuest.QuestSteps)
            {
                Debug.Log("[QuestManager] TYPE 4: Final step reached -> go to quest giver NPC");
            }
        }

        //only temporarly -> will be changed once type 4 gets handled correctly
        internal bool CanCompleteType4()
        {
            return m_currentQuestStep >= m_activeQuest.QuestSteps;
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
            float _damageValue = m_totalDamage + 40f; //TEMP: added value for testing stars
            float _fuelValue = m_totalFuelUsed + 3f; //TEMP: added value for testing stars

            int _timeStars = CalculateStars(_timeValue, m_activeQuest.m_timeThresholds, true);
            int _damageStars = CalculateStars(_damageValue, m_activeQuest.m_damageThresholds, true);
            int _fuelStars = CalculateStars(_fuelValue, m_activeQuest.m_fuelThresholds, true);

            int _averageStars = Mathf.RoundToInt((_timeStars + _damageStars + _fuelStars) / 3f);
            int _money = _averageStars switch
            {
                3 => m_activeQuest.m_moneyRewards.threeStars,
                2 => m_activeQuest.m_moneyRewards.twoStars,
                1 => m_activeQuest.m_moneyRewards.oneStar,
                _ => m_activeQuest.m_moneyRewards.zeroStars
            };

            QuestResult _result = new QuestResult
            {
                Quest = m_activeQuest,
                
                TimeStars = _timeStars,
                DamageStars = _damageStars,
                FuelStars = _fuelStars,

                TimeValue = _timeValue,
                DamageValue = _damageValue,
                FuelValue = _fuelValue,

                AverageStars = _averageStars,
                MoneyReward = _money
            };

            Debug.Log($"[QuestManager] Quest finished: {m_activeQuest.QuestName}");
            m_activeQuest = null;
            m_isQuestRunning = false;

            return _result;
        }

        private int CalculateStars(float _value, StarThresholds _thresholds, bool _isLowerBetter = true)
        {
            if (_isLowerBetter)
            {
                if (_value <= _thresholds.threeStars)
                {
                    return 3;
                }

                if (_value <= _thresholds.twoStars)
                {
                    return 2;
                }

                if (_value <= _thresholds.oneStar)
                {
                    return 1;
                }

                return 0;
            }
            else
            {
                if (_value >= _thresholds.threeStars)
                {
                    return 3;
                }

                if (_value >= _thresholds.twoStars)
                {
                    return 2;
                }

                if (_value >= _thresholds.oneStar)
                {
                    return 1;
                }

                return 0;
            }
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
