using UnityEngine;

public class GameManager : Manager<GameManager>
{
    private GameStates m_currentGameState = GameStates.PAUSED; // TODO Block inputs on start -> handle this in InputManager
    // TODO maybe method for setting current game state outside of game manager
    
    [SerializeField] private UIManager m_uiManager = null;

    public GameStates CurrentState => m_currentGameState;

    public void SetState(GameStates _newState)
    {
        m_currentGameState = _newState;

        Debug.Log($"Game State changed to: {_newState}");
    }

    public void Start()
    {
        
    }

    public void Update()
    {
        
    }
}
