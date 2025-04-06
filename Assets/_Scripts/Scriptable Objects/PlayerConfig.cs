using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerConfig", menuName= "ScriptableObjects/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public PlayerInput playerInput;         // Referencia al PlayerInput del jugador
    public CharacterData characterData;     // Datos del personaje seleccionado
    public int playerIndex;                 // Índice del jugador
}
