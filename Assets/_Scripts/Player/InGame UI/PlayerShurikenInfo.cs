
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShurikenInfo : MonoBehaviour
{
    [SerializeField] private Player _player;    
    [SerializeField] private Transform _shurikenUIContainer;

    private List<Image> _shurikenImages;
    private void Awake()
    {
        _shurikenImages = _shurikenUIContainer.GetComponentsInChildren<Image>().ToList();

        for (int i = 0; i < _shurikenImages.Count; i++)
        {
            _shurikenImages[i].DOFade(0, 2f).SetEase(Ease.InOutCubic);
        }
    }

    public void OnShurikenUpdate(Component component, object data)
    {
        var player = (Player)component;
        var shurikenCount = (int)data;

        if(player != _player) return;

        UpdateShurikenUI(shurikenCount);
     
    }
    private void UpdateShurikenUI(int shurikenCount)
    {
        DOTween.Kill(this, true);
        foreach(Image shurikenImage in _shurikenImages)
        {
            shurikenImage.color = new Color(shurikenImage.color.r, shurikenImage.color.g, shurikenImage.color.b, 0);
            shurikenImage.gameObject.SetActive(false);
        }

        
        for (int i = 0; i < _shurikenImages.Count - shurikenCount; i++)
        {
            _shurikenImages[i].gameObject.SetActive(true);
            _shurikenImages[i].DOFade(1f, 0.5f).SetEase(Ease.InOutCubic);
            //_shurikenImages[i].DOFade(0, 2f).SetEase(Ease.InOutCubic);
        }
    }

}
