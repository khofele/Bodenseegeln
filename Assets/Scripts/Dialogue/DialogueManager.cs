using UnityEngine;

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

            int _startNode = DialogueStateManager.Instance.GetStartNode(_npc);
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

            //DialogueStateManager.Instance.SetNode(m_currentNPC.NPCID, _response.NextNode);

            //if (_response.NextNode < 0)
            //{
            //    EndDialogue();
            //    return;
            //}

            //SetNode(_response.NextNode);
        }

        internal void EndDialogue()
        {
            m_UI.Show(false);

            GameManager.Instance.SetState(GameStates.MOTORMODE);
            m_currentNPC = null;
            m_currentNode = null;
        }
    }
}
