using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioReferenceList", menuName = "ScriptableObjects/AudioReferences", order = 4)]
public class AudioReferenceSO : ScriptableObject
{
    [SerializeField]
    AudioItem[] audio_references;

    Dictionary<string, AudioClip> audio_list;

    public void InitializeList(){
        audio_list = new Dictionary<string, AudioClip>();

        foreach(AudioItem item in audio_references){
            audio_list.Add(item.GetId(), item.GetClip());
        }
    }

    public AudioClip GetAudioClip(string id){
        AudioClip value;
        if(audio_list.TryGetValue(id, out value)){
            return value;
        }else{
            Debug.LogWarning("The requested audio clip " + id + " does not exist or cannot be found");
            return null;
        }
    }
}

[Serializable]
public class AudioItem{
    [SerializeField]
    private string id;
    [SerializeField]
    private AudioClip clip;

    public string GetId(){
        return id;
    }

    public AudioClip GetClip(){
        return clip;
    }
}