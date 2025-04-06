using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class VFXManager
{

    public static void PlayVFX(string VFX_Name, VFXType type, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        var VFXs = Resources.Load<VFXDict>("VFX/VFXDict");
        GameObject vfxPrefab = VFXs.GetVFX(VFX_Name);
        if (vfxPrefab == null)
        {
            Debug.LogError($"VFX Prefab with name {VFX_Name} not found in the dictionary.");
            return;
        }
        PlayVFX(vfxPrefab, type,position, rotation, parent);
    }
    private static void PlayVFX(GameObject vfxPrefab, VFXType type, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        switch (type)
        {
            case VFXType.Animation:
                vfxPrefab.transform.SetParent(parent); 
                vfxPrefab.transform.position = position;
                vfxPrefab.transform.rotation = rotation; 
                var clipLength = vfxPrefab.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;
                DOVirtual.DelayedCall(clipLength, () =>
                {
                    vfxPrefab.SetActive(false);
                });

                break;
             case VFXType.ParticleSystem:
                vfxPrefab.transform.position = position;
                vfxPrefab.transform.rotation = rotation; 
                vfxPrefab.transform.SetParent(parent, true); 
                break; 
        }
    }
}

public enum VFXType
{
    Animation,
    ParticleSystem
}
