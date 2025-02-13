using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RomCheckController : MonoBehaviour
{

    [SerializeField] float _lerpDelay = 0.5f;
    [SerializeField] float _lerpDuration = 0.5f;
    [SerializeField] AudioSource _musicSource;

    private TextMeshProUGUI _text;
    //private AudioSource _sfxSource;
    private Image _background;

    // Start is called before the first frame update
    void Start()
    {
        //_sfxSource = GetComponent<AudioSource>();
        _text = GetComponentInChildren<TextMeshProUGUI>();
        _background = GetComponentInChildren<Image>();

        StartCoroutine(LerpText());
    }

    // Update is called once per frame
    IEnumerator LerpText()
    {
        yield return new WaitForSeconds(_lerpDelay);
        float t = 0;
        float start = 0;
        float end = 1;
        while (t < 1)
        {
            t += Time.deltaTime / _lerpDuration;
            _text.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        _musicSource.Play();
        StartCoroutine(LerpBackground());
    }

    IEnumerator LerpBackground(){
        yield return new WaitForSeconds(_lerpDelay);
        float t = 0;
        float start = 1;
        float end = 0;
        //bool playSfx = true;
        _text.alpha = 0;
        while (t < 1)
        {
            t += Time.deltaTime / _lerpDuration;
            _background.color = new Color(1, 1, 1, Mathf.Lerp(start, end, t));
            yield return null;
            //if (playSfx && t > 0.5f)
            //{
            //    _sfxSource.Play();
            //    playSfx = false;
            //}
        }
        Destroy(gameObject);
    }
}
