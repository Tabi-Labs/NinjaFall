using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig _gameConfig;
    private List<Player> _players;
    private int _currentPlayersAlive;

    #region ------ UNITY CALLBACKS ------
    void Start()
    {
        //_gameConfig.CurrentRound = 0;
        _currentPlayersAlive = 2;
    }

    #endregion

    #region EVENT HANDLERS
    public void OnPlayerDied(Component sender, object data)
    {
        _currentPlayersAlive--;
        if (_currentPlayersAlive > 1) return;

        EndGame();
    }

    #endregion
    #region ROUNDS

    private void EndGame()
    {
        if(_gameConfig.CurrentRound >= _gameConfig.MaxRounds)
        {
            Debug.Log("Game Over");
            return;
        }
        NextRound();
        
    }
    private void NextRound()
    {
        _gameConfig.CurrentRound++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // Reset player positions
        
        /*         foreach (Player player in players)
        {
            //player.transform.position = player.StartPosition;
            //player.Reset();
        } */

        // Reset round timer
        //RoundTimer.Reset();

        // Start the next round
        //RoundManager.StartNextRound();
    }

    #endregion
}
