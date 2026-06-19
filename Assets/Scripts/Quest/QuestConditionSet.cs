using UnityEngine;
using System.Collections.Generic;

namespace Quest
{
    public enum QuestConditionType
    {
        None = 0,
        IsCompleted = 1,
        IsNotCompleted = 2,
        IsCurrentActiveQuest = 3
    }

    [System.Serializable]
    public class QuestCondition
    {
        public QuestData m_quest;
        public QuestConditionType m_conditionType;
    }

    [System.Serializable]
    public class QuestConditionGroup
    {
        [Tooltip("conditions are linked with AND")] 
        public List<QuestCondition> m_conditions = new();
    }

    //Used for organizing dialogues in branches with states and conditions
    [System.Serializable]
    public class QuestConditionSet
    {
        [Tooltip("groups are linked with OR")]
        public List<QuestConditionGroup> m_groups = new();

        internal bool Evaluate()
        {
            //no groups = automatically valid
            if (m_groups == null || m_groups.Count == 0)
            {
                return true;
            }

            //OR between groups
            for (int i = 0; i < m_groups.Count; i++)
            {
                bool _groupValid = EvaluateGroup(m_groups[i]);

                if (_groupValid)
                {
                    return true;
                }
            }

            Debug.Log("[QuestConditionSet] no valid group found");
            return false;
        }

        private bool EvaluateGroup(QuestConditionGroup _group)
        {
            if (_group == null || _group.m_conditions == null)
            {
                return false;
            }

            //AND between conditions
            for (int i = 0; i < _group.m_conditions.Count; i++)
            {
                bool _conditionValid = EvaluateCondition(_group.m_conditions[i]);

                if (!_conditionValid)
                {
                    return false;
                }
            }
            return true;
        }

        private bool EvaluateCondition(QuestCondition _condition)
        {
            if (_condition == null)
            {
                return false;
            }

            if (_condition.m_quest == null)
            {
                Debug.LogWarning("[QuestConditionSet] condition has NULL quest");
                return false;
            }

            QuestManager _questManager = QuestManager.Instance;

            switch (_condition.m_conditionType)
            {
                case QuestConditionType.IsCurrentActiveQuest:
                    return _questManager.ActiveQuest == _condition.m_quest;
                case QuestConditionType.IsCompleted:
                    return _questManager.IsQuestCompleted(_condition.m_quest);
                case QuestConditionType.IsNotCompleted:
                    return !_questManager.IsQuestCompleted(_condition.m_quest);
                default:
                    return false;
            }
        }
    }
}
