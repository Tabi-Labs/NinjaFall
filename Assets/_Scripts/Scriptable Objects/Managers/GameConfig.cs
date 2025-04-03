using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
public class GameConfig : ScriptableObject
{
    public int CurrentRound;
    public int MaxRounds = 3;
    public int MaxTime = 60;
    
}
