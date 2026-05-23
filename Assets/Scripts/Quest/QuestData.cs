using Dialogue;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

namespace Quest
{
    [System.Serializable]
    public class QuestStepText
    {
        [TextArea(2, 4)] public string m_text;
    }

    [System.Serializable]
    public struct RewardRange
    {
        public float fiveCircleValue;
        public float zeroCircleValue;
    }

    public enum QuestType
    {
        None = -1,
        Type1_DialogueOtherNPC = 0,
        Type2_DialogueSameNPC = 1,
        Type3_DirectCompletion = 2,
        Type4_MultiStep = 3
    }

    [CreateAssetMenu(fileName = "QuestData", menuName = "Quest/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("General")]
        [SerializeField] private string m_questName;
        [SerializeField] private string m_questTargetPlace;
        [SerializeField] private QuestType m_questType = QuestType.None;
        [SerializeField] private NPCData m_questGiverNPC = null;
        [SerializeField] private NPCData m_targetNPC = null;

        [Header("Game End")]
        [SerializeField] private bool m_isFinalQuest = false;
        
        [Header("Type 4 Multistep")]
        [SerializeField] private int m_questSteps = 2;
        [SerializeField] private List<QuestStepText> m_stepTexts = new();

        [Header("Reward Ranges")]
        [SerializeField] internal RewardRange m_timeRange;
        [SerializeField] internal RewardRange m_damageRange;
        [SerializeField] internal RewardRange m_fuelRange;
        [SerializeField] internal RewardRange m_moneyRange;



        public string QuestName => m_questName;
        internal string QuestTargetPlace => m_questTargetPlace;
        internal QuestType QuestType => m_questType;
        internal NPCData QuestGiverNPC => m_questGiverNPC;
        internal NPCData TargetNPC => m_targetNPC;
        internal bool IsFinalQuest => m_isFinalQuest;
        internal int QuestSteps => m_questSteps;
        internal List<QuestStepText> StepTexts => m_stepTexts;
    }
}
