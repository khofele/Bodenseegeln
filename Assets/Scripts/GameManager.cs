using UnityEngine;

public class GameManager : Manager
{
    private GameStates m_currentGameState = GameStates.PAUSED; // TODO Block inputs on start
    // TODO maybe method for setting current game state outside of game manager
    
    [SerializeField] private UIManager m_uiManager = null;

    public void Start()
    {
        
    }

    public void Update()
    {
        
    }
}
