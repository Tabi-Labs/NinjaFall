using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

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
    //[SerializeField] private AudioSource sound_source;
    [SerializeField] private GameObject sound_prefab;
    private PoolingManager audio_pool = new PoolingManager();

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
        // DontDestroyOnLoad(gameObject);
        _instance = this;

        music_list.InitializeList();
        speech_list.InitializeList();
        sound_list.InitializeList();

        audio_pool.InitializePool(sound_prefab, sound_prefab.transform);
    }
    #endregion

    #region Play

    private IEnumerator MonitorAudioProgress(AudioSource audio_source)
    {
        if (audio_source == null) yield break;

        while (audio_source != null && (audio_source.time + 0.05f < audio_source.clip.length))
        { 
            yield return null;
        }

        if(audio_source != null)
            Destroy(audio_source.gameObject);
    }

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
                        music_source[!playing ? 1 : 0].clip = null;
                    });
                });
                break;
            case 1:
                speech_source.Stop();
                speech_source.pitch = Random.Range(0.8f, 1.2f);
                speech_source.PlayOneShot(clip);
                break;
            case 2:
                GameObject audio_source = audio_pool.GetObject();
                AudioSource source_component = audio_source.GetComponent<AudioSource>();
                if (source_component.isPlaying)
                {
                    Debug.LogWarning("The requested audio source " + audio_source.name + " is busy. Consider expanding the pooling");
                }
                audio_source.transform.position = position;
                source_component.clip = clip;
                source_component.Play();
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

    public static void PlaySound(string clip_id, Transform transform = null)
    {
        AudioClip clip = _instance.sound_list.GetAudioClip(clip_id);
        if (_instance == null || clip == null)
            return;
        _instance.PlayAudio(2, clip, transform.position);
    }
    #endregion

    #region Pause
    private void PauseAudio(int source)
    {
        switch (source)
        {
            case 0:
                bool playing = music_source[0].isPlaying;

                music_source[!playing ? 1 : 0].Pause();
                break;
            case 1:
                speech_source.Pause();
                break;
            case 2:
                foreach (GameObject gO in audio_pool.pool)
                {
                    AudioSource source_component = gO.GetComponent<AudioSource>();
                    if (source_component.isPlaying) { source_component.Pause(); }
                }
                break;
            default:
                var audios = GameObject.FindObjectsOfType<AudioSource>();
                foreach (var audio in audios)
                {
                    audio.GetComponent<AudioSource>().Pause();
                }
                break;
        }
    }

    public static void PauseMusic()
    {
        if (_instance == null)
            return;
        _instance.PauseAudio(0);
    }

    public static void PauseSpeech()
    {
        if (_instance == null)
            return;
        _instance.PauseAudio(1);
    }

    public static void PauseSound()
    {
        if (_instance == null)
            return;
        _instance.PauseAudio(2);
    }

    public static void PauseAll()
    {
        if (_instance == null)
            return;
        _instance.PauseAudio(3);
    }
    #endregion

    #region Resume
    private void ResumeAudio(int source)
    {
        switch (source)
        {
            case 0:
                bool playing = music_source[0].clip != null;

                music_source[!playing ? 1 : 0].UnPause();
                break;
            case 1:
                speech_source.UnPause();
                break;
            case 2:
                foreach (GameObject gO in audio_pool.pool)
                {
                    AudioSource source_component = gO.GetComponent<AudioSource>();
                    if (!source_component.isPlaying) { source_component.UnPause(); }
                }
                break;
            default:
                var audios = GameObject.FindObjectsOfType<AudioSource>();
                foreach (var audio in audios)
                {
                    audio.GetComponent<AudioSource>().UnPause();
                }
                break;
        }
    }

    public static void ResumeMusic()
    {
        if (_instance == null)
            return;
        _instance.ResumeAudio(0);
    }

    public static void ResumeSpeech()
    {
        if (_instance == null)
            return;
        _instance.ResumeAudio(1);
    }

    public static void ResumeSound()
    {
        if (_instance == null)
            return;
        _instance.ResumeAudio(2);
    }

    public static void ResumeAll()
    {
        if (_instance == null)
            return;
        _instance.ResumeAudio(3);
    }
    #endregion
}
