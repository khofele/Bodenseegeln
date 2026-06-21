using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using WwiseEvent = AK.Wwise.Event;

using Quest;
using System;

public class GameManager : Manager<GameManager>
{
    [Header("UI")]
    [SerializeField] private UIManager m_uiManager = null;
    [SerializeField] private PauseMenuUI m_pauseMenu = null;

    [SerializeField] private int m_money = 0;

    [Header("Input Action References")]
    [SerializeField] private InputActionReference m_openPauseMenuAction = null;

    [Header("Wwise Money Sounds")]
    [SerializeField] private WwiseEvent m_moneyGainSound;
    [SerializeField] private WwiseEvent m_moneySpendSound;

    private GameStates m_currentGameState = GameStates.MOTORMODE;
    private GameStates m_prevGameState;

    private Dictionary<QuestData, float> m_questResults = new();
    private float m_averageQuestResult = 0f;
    private int m_resetCost = 100;

    public static event Action<GameStates, GameStates> OnGameStateChanged;
    public static event Action<GameStates> OnStateChangedToMotormode;

    public GameStates CurrentState => m_currentGameState;
    public int Money => m_money;
    public int ResetCost => m_resetCost;

    public void SetState(GameStates _newState)
    {
        GameStates prevGameState = m_currentGameState;

        m_currentGameState = _newState;

        Debug.Log($"Game State changed to: {_newState}");

        OnGameStateChanged?.Invoke(prevGameState, _newState);
    }

    private void HandleStateChange(GameStates _prevGameState, GameStates _newGameState)
    {
        if (_newGameState == GameStates.GAMEOVER || _newGameState == GameStates.GAMEWON)
        {
            SceneManager.LoadScene("GameWonOver");
            return;
        }

        if (_newGameState == GameStates.PAUSED)
        {
            Time.timeScale = 0.0f;
            return;
        }

        if(_newGameState == GameStates.MOTORMODE || _newGameState == GameStates.SAILMODE)
        {
            OnStateChangedToMotormode?.Invoke(_newGameState);
        }

        Time.timeScale = 1.0f;
    }

    public void AddMoney(int _amount)
    {
        m_money += _amount;
        Debug.Log($"[GameManager] Money added: {_amount} | Total: {m_money}");
        
        m_moneyGainSound.Post(gameObject); //Audio play gain money sound
    }

    public void DecreaseMoney(int _amount)
    {
        m_money -= _amount;
        Debug.Log($"[GameManager] Money decreased: {_amount} | Total: {m_money}");
    }

    public float GetAverageQuestResult()
    {
        return m_averageQuestResult;
    }

    public void RegisterQuestResult(QuestData _quest, float _averageCircles)
    {
        if (_quest == null)
        {
            Debug.LogWarning("[GameManager] tried to register NULL quest result");
            return;
        }

        //only update if quest not already saved
        m_questResults[_quest] = _averageCircles;

        RecalculateAverageQuestResult();
    }

    private void RecalculateAverageQuestResult()
    {
        if (m_questResults.Count == 0)
        {
            m_averageQuestResult = 0f;
            return;
        }

        float _sum = 0f;

        foreach (float _value in m_questResults.Values)
        {
            _sum += _value;
        }

        m_averageQuestResult = _sum / m_questResults.Count;
    }

    private void PauseGame()
    {
        m_prevGameState = m_currentGameState;
        SetState(GameStates.PAUSED);
        m_pauseMenu.gameObject.SetActive(true);
    }

    private void ResumeGame()
    {
        if(m_prevGameState == GameStates.SAILMODE || m_prevGameState == GameStates.MOTORMODE)
        {
            SetState(m_prevGameState);
        }
        else
        {
            SetState(GameStates.SAILMODE);
        }

        m_pauseMenu.gameObject.SetActive(false);
    }

    private void GetPausedInput()
    {
        if(m_currentGameState != GameStates.GAMEWON && m_currentGameState != GameStates.GAMEOVER && m_currentGameState != GameStates.DIALOGMODE)
        {
            if (m_openPauseMenuAction.action.triggered == true)
            {
                if (m_currentGameState == GameStates.PAUSED)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    public void OnEnable()
    {
        if(m_openPauseMenuAction != null)
        {
            m_openPauseMenuAction.action.Enable();
        }

        if(m_pauseMenu != null)
        {
            PauseMenuUI.OnResumeButtonClicked += ResumeGame;
        }

        OnGameStateChanged += HandleStateChange;
    }

    public void OnDisable()
    {
        if (m_openPauseMenuAction != null)
        {
            m_openPauseMenuAction.action.Disable();
        }

        if (m_pauseMenu != null)
        {
            PauseMenuUI.OnResumeButtonClicked -= ResumeGame;
        }

        OnGameStateChanged -= HandleStateChange;
    }

    public void Update()
    {
        GetPausedInput();
    }
}
