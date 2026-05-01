using UnityEngine;
using Quest;

namespace Dialogue
{
    public class DialogueManager : Manager<DialogueManager>
    {
        private NPCData m_currentNPC;
        private DialogueNode m_currentNode;

        [SerializeField]
        private DialogueUI m_UI;

        internal void StartDialogue(NPCData _npc)
        {
            m_currentNPC = _npc;
            GameManager.Instance.SetState(GameStates.DIALOGMODE);

            m_UI.Show(true);

            int _startNode = DialogueStateManager.Instance.GetStartNodeConsideringState(_npc); //DialogueStateManager.Instance.GetStartNode(_npc);
            SetNode(_startNode);
        }

        private void SetNode(int _nodeID)
        {
            m_currentNode = m_currentNPC.GetNodeByID(_nodeID);

            if (m_currentNode == null)
            {
                EndDialogue();
                return;
            }

            m_UI.DisplayNode(m_currentNPC, m_currentNode);
        }

        internal void ChooseResponse(DialogueResponse _response)
        {
            //execute action first
            HandleAction(_response);

            //only save valid nodes -> -1 is only for closing the dialoge, but should not be saved for future dialogues
            if (_response.NextNode >= 0)
            {
                DialogueStateManager.Instance.SetNode(m_currentNPC.NPCID, _response.NextNode);
                SetNode(_response.NextNode);
            }
            else
            {
                EndDialogue();
                return;
            }
        }

        private void HandleAction(DialogueResponse _response)
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
                    break;
                case DialogueActionType.CompleteQuest:
                    QuestManager.Instance.CompleteQuestFromDialogue();
                    break;
            }
        }

        internal void EndDialogue()
        {
            m_UI.Show(false);

            QuestManager.Instance.ConfirmPendingQuest();
            GameManager.Instance.SetState(GameStates.MOTORMODE);
            m_currentNPC = null;
            m_currentNode = null;
        }
    }
}
