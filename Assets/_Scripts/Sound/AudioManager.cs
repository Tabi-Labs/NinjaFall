using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[DefaultExecutionOrder(-1)]
public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioReferenceSO music_list;
    [SerializeField] private AudioReferenceSO speech_list;
    [SerializeField] private AudioReferenceSO sound_list;
    [Header("Sources")]
    [SerializeField] private AudioSource[] music_source;
    [SerializeField] private AudioSource speech_source;

	private float fade_duration = 0.5f;

    #region Singleton
    private static AudioManager _instance = null;
    public static AudioManager Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        _instance = this;

        music_list.InitializeList();
        speech_list.InitializeList();
        sound_list.InitializeList();
    }
    #endregion

    private void PlayAudio(int source, AudioClip clip, Vector3 position = new Vector3())
    {
        switch (source)
        {
            case 0:
                bool playing = music_source[0].isPlaying;
                
                music_source[!playing ? 1 : 0].DOFade(0f, fade_duration).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    music_source[!playing ? 1 : 0].Stop();
					music_source[playing ? 1 : 0].clip = clip;
					music_source[playing ? 1 : 0].DOFade(0.3f, fade_duration).SetEase(Ease.OutCubic).OnStart(() =>
					{
						music_source[playing ? 1 : 0].Play();
					});
                });
                break;
            case 1:
                speech_source.Stop();
                speech_source.pitch = Random.Range(0.8f, 1.2f);
                speech_source.PlayOneShot(clip);
                break;
            case 2:
                AudioSource.PlayClipAtPoint(clip, position);
                break;
        }
    }

    public static void PlayMusic(string clip_id)
    {
        AudioClip clip = _instance.music_list.GetAudioClip(clip_id);
        if (_instance == null || clip == null)
            return;
		_instance.fade_duration = 0.5f;
        _instance.PlayAudio(0, clip);
    }
	
	public static void PlayMusic(string clip_id, float duration)
    {
        AudioClip clip = _instance.music_list.GetAudioClip(clip_id);
        if (_instance == null || clip == null)
            return;
		_instance.fade_duration = duration;
        _instance.PlayAudio(0, clip);
    }

    public static void PlaySpeech(string clip_id)
    {
        AudioClip clip = _instance.speech_list.GetAudioClip(clip_id);
        if (_instance == null || clip == null)
            return;
        _instance.PlayAudio(1, clip);
    }

    public static void PlaySound(string clip_id)
    {
        AudioClip clip = _instance.sound_list.GetAudioClip(clip_id);
        if (_instance == null || clip == null)
            return;
        _instance.PlayAudio(2, clip);
    }

    public static void PlaySound(string clip_id, Transform transform)
    {
        AudioClip clip = _instance.sound_list.GetAudioClip(clip_id);
        if (_instance == null || clip == null)
            return;
        _instance.PlayAudio(2, clip, transform.position);
    }
}
