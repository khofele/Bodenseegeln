using Quest;
using System.Collections.Generic;
using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class GameManager : Manager<GameManager>
{
    private GameStates m_currentGameState = GameStates.SAILMODE; // TODO Block inputs on start -> handle this in InputManager
    // TODO maybe method for setting current game state outside of game manager
    
    [SerializeField] private UIManager m_uiManager = null;

    [SerializeField] private int m_money = 0;

    [Header("Wwise Money Sounds")]
    [SerializeField] private WwiseEvent m_moneyGainSound;
    [SerializeField] private WwiseEvent m_moneySpendSound;

    private Dictionary<QuestData, float> m_questResults = new();
    private float m_averageQuestResult = 0f;
    private int m_resetCost = 100;

    public GameStates CurrentState => m_currentGameState;
    public int Money => m_money;
    public int ResetCost => m_resetCost;

    public void SetState(GameStates _newState)
    {
        m_currentGameState = _newState;

        Debug.Log($"Game State changed to: {_newState}");
    }

    public void AddMoney(int _amount)
    {
        m_money += _amount;
        Debug.Log($"[GameManager] Money added: {_amount} | Total: {m_money}");
        
        // Play the Wwise money gain even
        m_moneyGainSound.Post(gameObject);
    }

    public void DecreaseMoney(int _amount)
    {
        m_money -= _amount;
        Debug.Log($"[GameManager] Money decreased: {_amount} | Total: {m_money}");

        // Play the Wwise money gain even
        //TODO Money decrease Sound m_moneyGainSound.Post(gameObject);
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

        Debug.Log($"[GameManager] saved result for {_quest.QuestName}: {_averageCircles}");
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

        Debug.Log($"[GameManager] new average quest result: {m_averageQuestResult}");
    }

    public void Update()
    {
        // TODO if statement später ändern je nach game over oder game won
        // TODO trigger Game Won im Questsystem
        if (m_currentGameState == GameStates.GAMEOVER || m_currentGameState == GameStates.GAMEWON)
        {
            // stop game
            Time.timeScale = 0.0f;

            // TODO show game over screen
            // TODO show game won screen
        }
        //else if(m_currentGameState == GameStates.GAMEWON)
        //{
        //}
    }
}
