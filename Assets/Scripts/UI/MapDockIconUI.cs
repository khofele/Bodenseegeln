using UnityEngine;

namespace MapUI
{
    public class MapDockIconUI : MonoBehaviour
    {
        //little extra script for dock icons on map with the 3 extra parts for indicating possible quests, repair and refuel at the dock

        [SerializeField] private GameObject m_fuelIcon;
        [SerializeField] private GameObject m_repairIcon;
        [SerializeField] private GameObject m_questIcon;

        internal void SetFuelVisible(bool _visible)
        {
            m_fuelIcon.SetActive(_visible);
        }

        internal void SetRepairVisible(bool _visible)
        {
            m_repairIcon.SetActive(_visible);
        }

        internal void SetQuestVisible(bool _visible)
        {
            m_questIcon.SetActive(_visible);
        }
    }
}
