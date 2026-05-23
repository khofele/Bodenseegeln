using UnityEngine;
using Quest;
using System;

namespace Dialogue
{
    public class DialogueManager : Manager<DialogueManager>
    {
        private NPCData m_currentNPC;
        private DialogueNode m_currentNode;
        private NPCDialogueBranch m_currentBranch;
        private DialogueResponse m_pendingResponse;

        [SerializeField]
        private DialogueUI m_UI;

        internal void StartDialogue(NPCData _npc)
        {
            m_currentNPC = _npc;
            GameManager.Instance.SetState(GameStates.DIALOGMODE);

            m_UI.Show(true);

            m_currentBranch = DialogueStateManager.Instance.GetBestDialogueBranch(_npc);
            if (m_currentBranch == null)
            {
                Debug.Log($"[DialogueManager] no valid branch for {_npc.Name}");
                EndDialogue();
                return;
            }

            int _savedRepeatNode = DialogueStateManager.Instance.GetStoredRepeatNode(_npc, m_currentBranch);
            int _startNode = _savedRepeatNode >= 0 ? _savedRepeatNode : m_currentBranch.StartNodeID;

            SetNode(_startNode);
        }

        private void SetNode(int _nodeID)
        {
            DialogueNode _node = m_currentNPC.GetNodeByID(_nodeID);

            if (_node == null)
            {
                EndDialogue();
                return;
            }

            if (!_node.IsUnlocked())
            {
                Debug.Log($"[DialogueManager] Node {_nodeID} locked");
                EndDialogue();
                return;
            }

            m_currentNode = _node;
            m_UI.DisplayNode(m_currentNPC, m_currentNode);
        }

        internal void ChooseResponse(DialogueResponse _response)
        {
            //execute action first
            bool _pauseDialogue = HandleAction(_response);

            if (_pauseDialogue)
            {
                m_pendingResponse = _response;
                return;
            }

            //only save valid nodes -> -1 is only for closing the dialoge, but should not be saved for future dialogues
            if (_response.NextNode >= 0)
            {
                //DialogueStateManager.Instance.SetNode(m_currentNPC.NPCID, _response.NextNode);
                SetNode(_response.NextNode);
            }
            else
            {
                EndDialogue();
                return;
            }
        }

        private bool HandleAction(DialogueResponse _response)
        {
            switch (_response.ActionType)
            {
                case DialogueActionType.StartQuest:
                    if (_response.Quest != null)
                    {
                        //Debug.Log($"[DialogueManager] Start Quest: {_response.Quest.QuestName}");
                        QuestManager.Instance.SetPendingQuest(_response.Quest);
                    }
                    else
                    {
                        Debug.Log($"[DialogueManager] Start Quest action but no quest assigned");
                    }
                    return false; //do not pause dialogue progression
                case DialogueActionType.CompleteQuest:
                    QuestManager.Instance.CompleteQuestFromDialogue();
                    return true; //pause dialogue progression
            }

            return false;
        }

        internal void ContinueDialogueAfterQuestReward()
        {
            if (m_pendingResponse == null)
            {
                return;
            }

            DialogueResponse _response = m_pendingResponse;
            m_pendingResponse = null;

            if (_response.NextNode >= 0)
            {
                SetNode(_response.NextNode);
            }
            else
            {
                EndDialogue();
            }
        }

        internal void EndDialogue()
        {
            m_UI.Show(false);

            if (m_currentNPC != null && m_currentBranch != null)
            {
                DialogueStateManager.Instance.SaveRepeatNode(m_currentNPC, m_currentBranch);
            }

            QuestManager.Instance.ConfirmPendingQuest();
            GameManager.Instance.SetState(GameStates.MOTORMODE);
            m_currentNPC = null;
            m_currentNode = null;
            m_currentBranch = null;
        }
    }
}
