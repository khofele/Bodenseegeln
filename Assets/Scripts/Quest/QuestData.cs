using Dialogue;
using UnityEngine;

namespace Quest
{
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
        [SerializeField] private string m_questName;
        [SerializeField] private QuestType m_questType = QuestType.None;
        [SerializeField] private NPCData _questGiverNPC = null;
        [SerializeField] private NPCData m_targetNPC = null;
        [SerializeField] private int m_questSteps = 2;

        [Header("Rewards")]
        public int moneyPerStar = 50;

        public string QuestName => m_questName;
        internal QuestType QuestType => m_questType;
        internal NPCData QuestGiverNPC => _questGiverNPC;
        internal NPCData TargetNPC => m_targetNPC;
        internal int QuestSteps => m_questSteps;
    }
}
