using UnityEngine;

namespace Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField]
        private NPCData m_npc;

        internal void TryInteract()
        {
            if (!IsDocked())
            {
                return;
            }

            DialogueManager.Instance.StartDialogue(m_npc);
        }

        private bool IsDocked()
        {
            //TODO: check if boat is docked at port
            //placeholder
            return true;
        }
    }
}
