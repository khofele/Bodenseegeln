using System.Collections.Generic;
using System.Linq;
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

        internal int GetStartNodeConsideringState(NPCData _npc)
        {
            Quest.QuestManager _questManager = Quest.QuestManager.Instance;

            //check quest state
            //completed quests
            if (_questManager.ActiveQuest == null)
            {
                foreach (var _quest in _questManager.GetCompletedQuests())
                {
                    if (_quest.QuestGiverNPC == _npc)
                    {
                        if (_npc.StartNodeIDOwnQuestCompleted >= 0)
                        {
                            return _npc.StartNodeIDOwnQuestCompleted;
                        }
                    }
                }

                //the for loop version is worse for performance (i just keep for record)
                //for (int i = 0; i < _questManager.GetCompletedQuests().Count; i++)
                //{
                //    if (_questManager.GetCompletedQuests()[i].QuestGiverNPC == _npc)
                //    {
                //        if (_npc.StartNodeIDOwnQuestCompleted >= 0)
                //        {
                //            return _npc.StartNodeIDOwnQuestCompleted;
                //        }
                //    }
                //}
            }
            //running quest
            if (_questManager.IsQuestRunning)
            {
                if (_questManager.ActiveQuest != null && _questManager.ActiveQuest.QuestGiverNPC == _npc)
                {
                    if (_npc.StartNodeIDOwnQuestActive >= 0)
                    {
                        return _npc.StartNodeIDOwnQuestActive;
                    }
                }
                else
                {
                    if (_npc.StartNodeIDOtherQuestActive >= 0)
                    {
                        return _npc.StartNodeIDOtherQuestActive;
                    }
                }

                //if (_npc.StartNodeIDWhenQuestActive >= 0)
                //{
                //    return _npc.StartNodeIDWhenQuestActive;
                //}
            }

            //check if already have a saved node
            if (m_npcStates.TryGetValue(_npc.NPCID, out int _savedNode))
            {
                return _savedNode;
            }

            //default
            return _npc.StartNodeID;
        }
    }
}
