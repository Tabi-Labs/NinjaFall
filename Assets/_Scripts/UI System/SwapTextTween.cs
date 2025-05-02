using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class SwapTextTween : MonoBehaviour
{

    [SerializeField]
    private string[] texts = new string[] { "Text 1", "Text 2" };

    [SerializeField]
    private bool loop = false;

    [SerializeField]
    private float duration = 1.0f;

    private TextMeshProUGUI tmp;

    // Start is called before the first frame update
    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        tmp.text = texts[0];
        StartCoroutine(SwapText(0));
    }

    public IEnumerator SwapText(int currentIndex)
    {
        int index = currentIndex;
        while (true)
        {
            tmp.DOFade(0, duration / 2).OnComplete(() =>
            {
                index = (index + 1) % texts.Length;
                tmp.text = texts[index];
                tmp.DOFade(1, duration / 2);
            });

            yield return new WaitForSeconds(duration);
            
            if (!loop && index == 0)
            {
                break;
            }
        }
    }
    
}
