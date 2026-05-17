using UnityEngine;
using WwiseEvent = AK.Wwise.Event;

public class GameManager : Manager<GameManager>
{
    private GameStates m_currentGameState = GameStates.SAILMODE; // TODO Block inputs on start -> handle this in InputManager
    // TODO Robin: HIER GAME STATE ÄNDERN GameStates.SAILMODE bzw. GameStates.MOTORMODE
    // TODO maybe method for setting current game state outside of game manager
    
    [SerializeField] private UIManager m_uiManager = null;

    [SerializeField] private int m_money = 0;

    [Header("Wwise Money Sounds")]
    [SerializeField] private WwiseEvent m_moneyGainSound;
    [SerializeField] private WwiseEvent m_moneySpendSound;

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

    public void Start()
    {
        
    }

    public void Update()
    {
        
    }
}
