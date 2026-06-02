using System;
using System.Collections.Generic;
using UnityEngine;
using Quest;

public enum NPC_ID //TODO: add correct value for ID
{
    None = 0,
    Testian = 1,//just placeholder names
    NPC1 = 2,
    NPC2 = 3,
    NPC3 = 4,
    NPC4 = 5
}

public enum DialogueActionType
{
    None = 0,
    StartQuest = 1,
    CompleteQuest = 2
}

namespace Dialogue
{
    [CreateAssetMenu(fileName = "NPCData", menuName = "Dialogue/NPCData")]
    public class NPCData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField, Tooltip("for now just placeholder -> later correct NPC ID")]
        private NPC_ID m_NPCID = NPC_ID.None;
        [SerializeField]
        private string m_NPCName = "";
        [SerializeField]
        private Sprite m_NPCPortrait = null;
        [SerializeField]
        private Sprite m_NPCBackground = null;

        [Header("Dialogue")]
        [SerializeField] private List<NPCDialogueBranch> m_dialogueBranches = new();
        [SerializeField]
        private List<DialogueNode> m_nodes = new List<DialogueNode>();

        internal NPC_ID NPCID => m_NPCID;
        internal string Name => m_NPCName;
        internal Sprite Portrait => m_NPCPortrait;
        internal Sprite Background => m_NPCBackground;
        internal List<NPCDialogueBranch> DialogueBranches => m_dialogueBranches;

        public DialogueNode GetNodeByID(int _id)
        {
            return m_nodes.Find(node => node.NodeID == _id);
        }
    }

    [Serializable]
    public class DialogueNode
    {
        [SerializeField]
        private int m_NodeID;
        /*[SerializeField]*/ private QuestConditionSet m_unlockConditions = new();
        [SerializeField, TextArea(1, 6)]
        private string m_text;
        [SerializeField]
        private List<DialogueResponse> m_responses;

        internal int NodeID => m_NodeID;
        internal QuestConditionSet UnlockConditions => m_unlockConditions;
        internal string Text => m_text;

        internal List<DialogueResponse> GetResponses()
        {
            if (m_responses == null)
            {
                return new List<DialogueResponse>();
            }

            if (m_responses.Count > 4)
            {
                Debug.LogWarning($"Node {m_NodeID} has more than 4 responses!");
            }

            return m_responses.GetRange(0, Mathf.Min(4, m_responses.Count));
        }

        internal bool IsUnlocked()
        {
            if (m_unlockConditions == null)
            {
                return true;
            }

            return m_unlockConditions.Evaluate();
        }
    }

    [Serializable]
    public class DialogueResponse
    {
        [SerializeField]
        private string m_responseText;
        [SerializeField, Tooltip("-1 = end dialogue")]
        private int m_nextNodeID; //-1 = end
        [SerializeField] private DialogueActionType m_actionType = DialogueActionType.None;
        [SerializeField] private QuestData m_quest = null;

        internal int NextNode => m_nextNodeID;
        internal string Text => m_responseText;
        internal DialogueActionType ActionType => m_actionType;
        internal QuestData Quest => m_quest;
    }

    [Serializable]
    public class NPCDialogueBranch
    {
        [Header("Branch")]
        [SerializeField] private string m_branchName;
        [SerializeField] private NPCDialogueState m_state;
        [SerializeField] private int m_priority = 0;

        [Header("Conditions")]
        [SerializeField] private QuestConditionSet m_activationConditions = new();

        [Header("Dialogue")]
        [SerializeField] private int m_startNodeID = 0;
        [SerializeField] private int m_repeatNodeID = -1;

        [Header("Quest")]
        [SerializeField, Tooltip("the quest of this NPC that matters for this branch")] private QuestData m_relatedQuest = null;

        internal string BranchName => m_branchName;
        internal NPCDialogueState State => m_state;
        internal int Priority => m_priority;
        internal QuestConditionSet ActivationConditions => m_activationConditions;
        internal int StartNodeID => m_startNodeID;
        internal int RepeatNodeID => m_repeatNodeID;
        internal QuestData RelatedQuest => m_relatedQuest;

        internal bool IsValid()
        {
            if (m_activationConditions == null)
            {
                return true;
            }

            return m_activationConditions.Evaluate();
        }
    }
}
