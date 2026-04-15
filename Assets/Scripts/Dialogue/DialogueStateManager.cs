using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{
    public class DialogueStateManager : Manager<DialogueStateManager>
    {
        private Dictionary<NPC_ID, int> m_npcStates = new();

        internal int GetStartNode(NPCData _npc)
        {
            if (m_npcStates.TryGetValue(_npc.NPCID, out int _node))
            {
                return _node;
            }

            return _npc.StartNodeID;
        }

        internal void SetNode(NPC_ID _id, int _nodeID)
        {
            m_npcStates[_id] = _nodeID;
        }
    }
}
