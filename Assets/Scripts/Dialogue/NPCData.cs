using System;
using System.Collections.Generic;
using UnityEngine;

public enum NPC_ID //TODO: add correct value for ID
{
    None = 0,
    Testian = 1,//just placeholder names
    NPC1 = 2,
    NPC2 = 3,
    NPC3 = 4,
    NPC4 = 5
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

        [Header("Dialogue")]
        [SerializeField]
        private int m_startNodeID = 0;
        [SerializeField]
        private List<DialogueNode> m_nodes = new List<DialogueNode>();

        internal NPC_ID NPCID => m_NPCID;
        internal int StartNodeID => m_startNodeID;
        internal string Name => m_NPCName;
        internal Sprite Portrait => m_NPCPortrait;

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
        [SerializeField, TextArea(1, 6)]
        private string m_text;
        [SerializeField]
        private List<DialogueResponse> m_responses;

        internal int NodeID => m_NodeID;
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
    }

    [Serializable]
    public class DialogueResponse
    {
        [SerializeField]
        private string m_responseText;
        [SerializeField, Tooltip("-1 = end dialogue")]
        private int m_nextNodeID; //-1 = end

        internal int NextNode => m_nextNodeID;
        internal string Text => m_responseText;

        //TODO: future DialogueAction
    }
}
