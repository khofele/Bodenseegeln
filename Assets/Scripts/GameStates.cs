using UnityEngine;

public enum GameStates
{
    PAUSED = 0, // Menu
    SAILMODE = 1, // Sailing
    MOTORMODE = 2, // Driving the boat with motor
    HUMANMODE = 3, // Walking around -> currently not implemented but prepared for future updates
    DIALOGMODE = 4, // Dialog and other overlay UI
    GAMEWON = 5, // Game won
    GAMEOVER = 6 // Game over
}
