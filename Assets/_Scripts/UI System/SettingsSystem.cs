using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SettingsSystem : MonoBehaviour
{
    [Header("Logic Attributes")]
    [SerializeField] public AudioMixer mixer;

    [Header("UI Attributes")]
    [SerializeField] public Slider resSlider;
    [SerializeField] public TextMeshProUGUI resText;
    [SerializeField] public Toggle screenToggle;
    [SerializeField] public Slider mxSlider;
    [SerializeField] public Slider sfxSlider;

    private int[] horRes = { 1920, 1280, 960, 854 };
    private int[] verRes = { 1080, 720, 540, 480 };

    //Pref values
    private int idRes = 0;
    private bool screenMode = true;
    private float mxVol = 10;
    private float sfxVol = 10;

    #region Singleton
    private static SettingsSystem _instance = null;
    public static SettingsSystem Instance
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

    }

    private void Start()
    {
        LoadSettings();
        LoadUI();
    }
    #endregion

    public static void SaveSettings()
    {
        PlayerPrefs.SetInt("Resolution", _instance.idRes);
        PlayerPrefs.SetInt("Screen_Mode", _instance.screenMode ? 1 : 0);
        PlayerPrefs.SetFloat("MX_Volume", _instance.mxVol);
        PlayerPrefs.SetFloat("SFX_Volume", _instance.sfxVol);

        PlayerPrefs.Save();
    }

    public static void LoadSettings() 
    {
        _instance.idRes = PlayerPrefs.GetInt("Resolution", 0);
        _instance.screenMode = PlayerPrefs.GetInt("Screen_Mode", 1) == 1;
        _instance.mxVol = PlayerPrefs.GetFloat("MX_Volume", 10);
        _instance.sfxVol = PlayerPrefs.GetFloat("SFX_Volume", 10);
    }

    public static void LoadUI()
    {
        _instance.resSlider.value = _instance.idRes;
        _instance.screenToggle.isOn = _instance.screenMode;
        _instance.mxSlider.value = _instance.mxVol;
        _instance.sfxSlider.value = _instance.sfxVol;
    }

    public static void ChangeScreenResolution(float res)
    {
        _instance.idRes = (int)res;

        string first = _instance.idRes == 0 ? "  " : "< ";
        string last = _instance.idRes == _instance.horRes.Length - 1 ? "  " : " >";

        _instance.resText.text = first + _instance.horRes[_instance.idRes] + " x " + _instance.verRes[_instance.idRes] + last;

        Screen.SetResolution(_instance.horRes[(int)res], _instance.verRes[(int)res], _instance.screenMode);
    }

    public static void ChangeFullScreen(bool screen)
    {
        _instance.screenMode = screen;
        Screen.SetResolution(_instance.horRes[_instance.idRes], _instance.verRes[_instance.idRes], screen);
    }

    public static void ChangeMusicVolume(float volume)
    {
        _instance.mxVol = volume;
        float dB = Mathf.Log10(Mathf.Clamp(volume/10, 0.0001f, 1f)) * 20;
        _instance.mixer.SetFloat("MX_Volume", dB);
    }

    public static void ChangeSoundVolume(float volume)
    {
        _instance.sfxVol = volume;
        float dB = Mathf.Log10(Mathf.Clamp(volume / 10, 0.0001f, 1f)) * 20;
        _instance.mixer.SetFloat("SFX_Volume", dB);
    }
}
