using System;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "VFXDict", menuName = "VFX/VFXDict", order = 1)]
public class VFXDict : ScriptableObject
{   
    [SerializeField] 
    VFX[] vfx;
    Dictionary<string, PoolingManager> vfx_list;
    PoolingManager poolingManager;
    public void InitializeList(){
        vfx_list = new Dictionary<string, PoolingManager>();

        foreach(VFX item in vfx){
            var pool = new PoolingManager();
            pool.InitializePool(item.GetVFX(), null, 5);
            vfx_list.Add(item.GetId(), pool);

        }
    }

    public GameObject GetVFX(string id){
        PoolingManager value;
        if(vfx_list.TryGetValue(id, out value)){
            return value.GetObject();
        }else{
            Debug.LogWarning("The requested VFX " + id + " does not exist or cannot be found");
            return null;
        }
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
