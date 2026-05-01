using UnityEngine;

public class GameManager : Manager<GameManager>
{
    private GameStates m_currentGameState = GameStates.SAILMODE; // TODO Block inputs on start -> handle this in InputManager
    // TODO Robin: HIER GAME STATE ÄNDERN GameStates.SAILMODE bzw. GameStates.MOTORMODE
    // TODO maybe method for setting current game state outside of game manager
    
    [SerializeField] private UIManager m_uiManager = null;

    [SerializeField] private int m_money = 0;

    public GameStates CurrentState => m_currentGameState;

    public void SetState(GameStates _newState)
    {
        m_currentGameState = _newState;

        Debug.Log($"Game State changed to: {_newState}");
    }

    public void AddMoney(int _amount)
    {
        m_money += _amount;
        Debug.Log($"[GameManager] Money added: {_amount} | Total: {m_money}");
    }

    public void Start()
    {
        
    }

    public void Update()
    {
        
    }
}
