using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

using Quest;
using Dialogue;


namespace MapUI
{
    public class MapUIController : MonoBehaviour
    {
        [Header("GameManager")]
        [SerializeField] private GameManager m_gameManager = null;

        [Header("Audio References")]
        [SerializeField] private EnvironmentAudioController m_environmentAudioController = null;  // Audio class reference

        [Header("Input")]
        [SerializeField] private InputActionReference m_toggleMapAction = null;

        [Header("UI")]
        [SerializeField] private GameObject m_root = null;
        [SerializeField] private RectTransform m_mapImage = null;

        [Header("Icons")]
        [SerializeField] private RectTransform m_boatIcon = null;
        [SerializeField] private GameObject m_dockIconPrefab = null;
        [SerializeField] private GameObject m_questIconPrefab = null;
        [SerializeField] private GameObject m_talkIconPrefab = null;

        [Header("Container")]
        [SerializeField] private Transform m_dockContainer = null;
        [SerializeField] private Transform m_questContainer = null;
        [SerializeField] private Transform m_talkContainer = null;

        [Header("Info")]
        [SerializeField] private TMP_Text m_moneyText = null;
        [SerializeField] private Image m_averageQuestFill = null;

        [Header("World")]
        [SerializeField] private Transform m_boatTransform = null;
        [SerializeField] private Transform m_docksRoot = null;
        [SerializeField] private Transform m_npcsRoot = null;
        [SerializeField] private Transform m_questsRoot = null;
        [SerializeField] private float m_dockQuestMergeDistance = 50f;

        [Header("Map Bounds")]
        [SerializeField] private Vector2 m_worldMin = Vector2.zero;
        [SerializeField] private Vector2 m_worldMax = Vector2.zero;

        private bool m_isOpen = false;
        private GameStates m_previousState;
        private Dictionary<DockInteraction, MapDockIconUI> m_dockIcons = new Dictionary<DockInteraction, MapDockIconUI>();

        internal void ToggleMap()
        {
            if (m_isOpen)
            {
                CloseMap();
            }
            else
            {
                OpenMap();
            }
        }

        private void OpenMap()
        {
            if(m_gameManager.CurrentState == GameStates.PAUSED)
            {
                return;
            }

            m_isOpen = true;
            m_previousState = GameManager.Instance.CurrentState;

            GameManager.Instance.SetState(GameStates.PAUSED);
            m_root.SetActive(true);
            m_environmentAudioController.PlayOpenMap(); //Audio play open map sound

            RefreshMap();
        }

        private void CloseMap()
        {
            m_isOpen = false;

            m_root.SetActive(false);
            GameManager.Instance?.SetState(m_previousState);
            m_environmentAudioController.PlayCloseMap(); //Audio play close map sound
        }

        private void RefreshMap()
        {
            m_dockIcons.Clear();
            ClearContainer(m_dockContainer);
            ClearContainer(m_questContainer);
            ClearContainer(m_talkContainer);

            UpdateBoatIcon();
            UpdateInfoTexts();

            CreateDockIcons();
            CreateQuestIcons();
            CreateNPCIcons();
        }

        private void ClearContainer(Transform _container)
        {
            for (int i = _container.childCount - 1; i >= 0; i--)
            {
                Destroy(_container.GetChild(i).gameObject);
            }
        }

        //place and rotate boat icon correctly on map according to worldposition of the boat
        private void UpdateBoatIcon()
        {
            if (m_boatTransform == null)
            {
                return;
            }

            m_boatIcon.anchoredPosition = WorldToMapPosition(m_boatTransform.position);

            m_boatIcon.localEulerAngles = new Vector3(0f, 0f, -m_boatTransform.eulerAngles.y);
        }

        private void UpdateInfoTexts()
        {
            if (m_averageQuestFill != null)
            {
                float _average = GameManager.Instance.GetAverageQuestResult();

                m_averageQuestFill.fillAmount = _average / 5f;
            }

            if (m_moneyText != null)
            {
                int _money = GameManager.Instance.Money;

                m_moneyText.text = $"{_money}€";
            }
        }

        private void CreateDockIcons()
        {
            for (int i = 0; i < m_docksRoot.childCount; i++)
            {
                DockInteraction _dock = m_docksRoot.GetChild(i).GetComponent<DockInteraction>();

                if (_dock == null)
                {
                    continue;
                }

                GameObject _icon = Instantiate(m_dockIconPrefab, m_dockContainer);
                RectTransform _rect = _icon.GetComponent<RectTransform>();

                _rect.anchoredPosition = WorldToMapPosition(_dock.transform.position);

                MapDockIconUI _dockUI = _icon.GetComponent<MapDockIconUI>();

                m_dockIcons.Add(_dock, _dockUI);
                _dockUI.SetFuelVisible(true); //currently is always true, because in current game docks always have refuel mechanic -> could be changed in future
                _dockUI.SetRepairVisible(true); //currently is always true, because in current game docks always have repair mechanic -> could be changed in future
                _dockUI.SetQuestVisible(false); //set when quest is close to dock
            }
        }

        private void CreateQuestIcons()
        {
            for (int i = 0; i < m_questsRoot.childCount; i++)
            {
                QuestInteraction _quest = m_questsRoot.GetChild(i).GetComponent<QuestInteraction>();

                if (_quest == null)
                {
                    continue;
                }

                if (!_quest.ShouldShowCompassIcon())
                {
                    continue;
                }

                DockInteraction _dock = FindNearbyDock(_quest.transform.position);

                if (_dock != null)
                {
                    if (m_dockIcons.TryGetValue(_dock, out MapDockIconUI _dockUI))
                    {
                        _dockUI.SetQuestVisible(true);
                    }

                    continue;
                }

                GameObject _icon = Instantiate(m_questIconPrefab, m_questContainer);
                RectTransform _rect = _icon.GetComponent<RectTransform>();

                _rect.anchoredPosition = WorldToMapPosition(_quest.transform.position);
            }
        }

        private void CreateNPCIcons()
        {
            for (int i = 0; i < m_npcsRoot.childCount; i++)
            {
                NPCInteraction _npc = m_npcsRoot.GetChild(i).GetComponent<NPCInteraction>();

                if (_npc == null)
                {
                    continue;
                }

                NPCIconState _state = _npc.GetCompassIconState();
                GameObject _icon = null;

                switch (_state)
                {
                    case NPCIconState.None:
                        continue;
                    case NPCIconState.Talk:
                        _icon = Instantiate(m_talkIconPrefab, m_talkContainer);
                        break;
                    case NPCIconState.Quest:
                        DockInteraction _dock = FindNearbyDock(_npc.transform.position);

                        if (_dock != null)
                        {
                            if (m_dockIcons.TryGetValue(_dock, out MapDockIconUI _dockUI))
                            {
                                _dockUI.SetQuestVisible(true);
                            }

                            continue;
                        }

                        _icon = Instantiate(m_questIconPrefab, m_talkContainer);
                        break;
                }

                RectTransform _rect = _icon.GetComponent<RectTransform>();

                _rect.anchoredPosition = WorldToMapPosition(_npc.transform.position);
            }
        }

        private Vector2 WorldToMapPosition (Vector3 _worldPosition)
        {
            float _normalizedX = Mathf.InverseLerp(m_worldMin.x, m_worldMax.x, _worldPosition.x);
            float _normalizedY = Mathf.InverseLerp(m_worldMin.y, m_worldMax.y, _worldPosition.z);
            float _mapWidt = m_mapImage.rect.width;
            float _mapHeight = m_mapImage.rect.height;

            return new Vector2((_normalizedX * _mapWidt) - (_mapWidt * 0.5f), (_normalizedY * _mapHeight) - (_mapHeight * 0.5f));
        }

        private DockInteraction FindNearbyDock(Vector3 _position)
        {
            for (int i = 0; i < m_docksRoot.childCount; i++)
            {
                DockInteraction _dock = m_docksRoot.GetChild(i).GetComponent<DockInteraction>();

                if (_dock == null)
                {
                    continue;
                }

                float _distance = Vector3.Distance(_position, _dock.transform.position);

                if (_distance <= m_dockQuestMergeDistance)
                {
                    return _dock;
                }
            }

            return null;
        }
    }
}
