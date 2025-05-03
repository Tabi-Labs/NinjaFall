using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName= "ScriptableObjects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public Sprite portrait;                      // Imagen principal
    // Set default value at 1.0f
    [Range(0.0f, 1.0f)]
    public float portraitLuminosity = 1.0f;      // Luminosidad del portrait
    public RuntimeAnimatorController portraitAnimator;  // Animator del portrait
    public Sprite text;                          // Imagen del texto
    public Color textOutlineColor;               // Color del borde del texto
    public Color heartColor;                     // Color del corazón en el HUD
    public Material mat;                         // Material personalizado
    public string[] victoryPhrases;              // Frases cuando el peronaje sale ganador
}
