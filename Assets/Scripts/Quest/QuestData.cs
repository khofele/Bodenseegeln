using Dialogue;
using UnityEngine;

namespace Quest
{
    [System.Serializable] 
    public struct StarThresholds
    {
        public float threeStars;
        public float twoStars;
        public float oneStar;
        public float zeroStars;
    }

    [System.Serializable]
    public struct MoneyRewards
    {
        public int threeStars;
        public int twoStars;
        public int oneStar;
        public int zeroStars;
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
        [SerializeField] private int m_questSteps = 2;

        [Header("Reward Ranges")]
        [SerializeField] internal RewardRange m_timeRange;
        [SerializeField] internal RewardRange m_damageRange;
        [SerializeField] internal RewardRange m_fuelRange;
        [SerializeField] internal RewardRange m_moneyRange;

        //[Header("Time (seconds)")]
        //[SerializeField] internal StarThresholds m_timeThresholds;

        //[Header("Damage")]
        //[SerializeField] internal StarThresholds m_damageThresholds;

        //[Header("Fuel")]
        //[SerializeField] internal StarThresholds m_fuelThresholds;

        //[Header("Money Rewards")]
        //[SerializeField] internal MoneyRewards m_moneyRewards;


        public string QuestName => m_questName;
        internal string QuestTargetPlace => m_questTargetPlace;
        internal QuestType QuestType => m_questType;
        internal NPCData QuestGiverNPC => m_questGiverNPC;
        internal NPCData TargetNPC => m_targetNPC;
        internal int QuestSteps => m_questSteps;
    }
}
