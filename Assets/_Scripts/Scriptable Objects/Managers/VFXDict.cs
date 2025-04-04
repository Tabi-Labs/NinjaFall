using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "VFXDict", menuName = "VFX/VFXDict", order = 1)]
public class VFXDict : ScriptableObject
{   
    [SerializeField] 
    VFX[] vfx;
    Dictionary<string, GameObject> vfx_list;

    public void InitializeList(){
        vfx_list = new Dictionary<string, GameObject>();

        foreach(VFX item in vfx){
            vfx_list.Add(item.GetId(), item.GetVFX());
        }
    }

    public GameObject GetVFX(string id){
        GameObject value;
        if(vfx_list.TryGetValue(id, out value)){
            return value;
        }else{
            Debug.LogWarning("The requested VFX " + id + " does not exist or cannot be found");
            return null;
        }
    }

    void OnEnable()
    {
        InitializeList();
    }
}

[Serializable]
public class VFX{
    [SerializeField]
    private string id;
    [SerializeField]
    private GameObject vfxPrefab;

    public string GetId(){
        return id;
    }

    public GameObject GetVFX(){
        return vfxPrefab;
    }
}
