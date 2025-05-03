using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;

public class LivesCanvasHandler : MonoBehaviour
{

    public int _iconsAlpha = 0;
    public UnityEngine.UI.Image _icon;

    public List<RectTransform> playerPanels {get; private set;} = new List<RectTransform>();

    public static LivesCanvasHandler Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            RectTransform panel = transform.GetChild(i).GetComponent<RectTransform>();
            if (panel != null)
            {
                playerPanels.Add(panel);
                panel.gameObject.SetActive(false);
            }
        }
    }

    public void InitializePanel(int playerIndex, CharacterData characterData, int lives){
        if (playerIndex < 0 || playerIndex >= playerPanels.Count)
        {
            Debug.LogError("Player index out of range: " + playerIndex + ". Total panels: " + playerPanels.Count);
            return;
        }

        RectTransform panel = playerPanels[playerIndex];
        Color col = characterData.heartColor;
        col = new Color(col.r, col.g, col.b, _iconsAlpha);
        for (int i = 0; i < lives; i++)
        {
            Image lifeIcon = Instantiate(_icon, panel);
            lifeIcon.color = col;
        }
        panel.gameObject.SetActive(true);
    }

    public void UpdatePanels(int playerIndex, int lives)
    {
        if (playerIndex < 0 || playerIndex >= playerPanels.Count)
        {
            Debug.LogError("Player index out of range: " + playerIndex + ". Total panels: " + playerPanels.Count);
            return;
        }

        Debug.Log("Updating player " + playerIndex + " lives to " + lives);
        RectTransform panel = playerPanels[playerIndex];
        Image[] lifeIcons = panel.GetComponentsInChildren<Image>();
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (i >= lives)
            {
                lifeIcons[i].gameObject.SetActive(false);
            }
        }
    }
}
