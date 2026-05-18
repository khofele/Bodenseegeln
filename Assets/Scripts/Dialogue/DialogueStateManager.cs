using Quest;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dialogue
{
    public enum NPCDialogueState
    {
        None = 0,
        ReadyToTalk = 1,
        QuestAvailable = 2,
        OwnQuestActive = 3,
        OtherQuestActive = 4,
        QuestReadyToComplete = 5,
        QuestCompleted = 6
    }

    public struct NPCDialogueProgressKey
    {
        public NPC_ID NPCID;
        public NPCDialogueState State;

        public NPCDialogueProgressKey (NPC_ID _npcID, NPCDialogueState _state)
        {
            NPCID = _npcID;
            State = _state;
        }
    }

    public class DialogueStateManager : Manager<DialogueStateManager>
    {
        private Dictionary<NPCDialogueProgressKey, int> m_repeatNodes = new();

        internal int GetStoredRepeatNode(NPCData _npc, NPCDialogueBranch _branch)
        {
            NPCDialogueProgressKey _key = new NPCDialogueProgressKey(_npc.NPCID, _branch.State);

            if (m_repeatNodes.TryGetValue(_key, out int _nodeID))
            {
                return _nodeID;
            }

            return -1;
        }

        internal void SaveRepeatNode(NPCData _npc, NPCDialogueBranch _branch)
        {
            if (_branch.RepeatNodeID < 0)
            {
                return;
            }

            NPCDialogueProgressKey _key = new NPCDialogueProgressKey(_npc.NPCID, _branch.State);

            m_repeatNodes[_key] = _branch.RepeatNodeID;
        }

        internal NPCDialogueBranch GetBestDialogueBranch(NPCData _npc)
        {
            if (_npc == null)
            {
                return null;
            }

            NPCDialogueBranch _bestBranch = null;
            int _highestPriority = int.MinValue;

            List<NPCDialogueBranch> _branches = _npc.DialogueBranches;

            for (int i = 0; i < _branches.Count; i++)
            {
                NPCDialogueBranch _branch = _branches[i];

                if (_branch == null)
                {
                    continue;
                }

                if (!_branch.IsValid())
                {
                    continue;
                }

                if (!IsBranchStateValid(_branch))
                {
                    continue;
                }

                if (_branch.Priority > _highestPriority)
                {
                    _highestPriority = _branch.Priority;
                    _bestBranch = _branch;
                }
            }

            return _bestBranch;
        }

        private bool IsBranchStateValid(NPCDialogueBranch _branch)
        {
            QuestManager _questManager = QuestManager.Instance;

            switch (_branch.State)
            {
                case NPCDialogueState.ReadyToTalk:
                    return true;
                case NPCDialogueState.QuestAvailable:
                    return !_questManager.HasActiveQuest;
                case NPCDialogueState.OwnQuestActive:
                    return _questManager.ActiveQuest == _branch.RelatedQuest;
                case NPCDialogueState.OtherQuestActive:
                    return _questManager.HasActiveQuest && _questManager.ActiveQuest != _branch.RelatedQuest;
                case NPCDialogueState.QuestReadyToComplete:
                    return _questManager.CanCompleteQuest(_branch.RelatedQuest);
                case NPCDialogueState.QuestCompleted:
                    return _questManager.IsQuestCompleted(_branch.RelatedQuest);
                default:
                    return false;
            }
        }
    }
}
