using UnityEngine;

public class GameManager : Manager
{
    // TODO Default-Modus wieder auf PAUSED setzen --> nur für Testzwecke
    private GameStates m_currentGameState = GameStates.MOTORMODE; // TODO Block inputs on start
    // TODO maybe method for setting current game state outside of game manager
    
    [SerializeField] private UIManager m_uiManager = null;

    public GameStates CurrentGameState
    {
        get
        {
            return m_currentGameState;
        }
    }

    public void Start()
    {
        
    }

    public void Update()
    {
        
    }
}
