using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VFXManager
{

    public static void PlayVFX(string VFX_Name, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var VFXs = Resources.Load<VFXDict>("VFX/VFXDict");
        GameObject vfxPrefab = VFXs.GetVFX(VFX_Name);
        if (vfxPrefab == null)
        {
            Debug.LogError($"VFX Prefab with name {VFX_Name} not found in the dictionary.");
            return;
        }
        PlayVFX(vfxPrefab, position, rotation, parent);
    }
    private static void PlayVFX(GameObject vfxPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        GameObject.Instantiate(vfxPrefab, position, rotation, parent);
    }
}
