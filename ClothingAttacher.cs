using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

[CustomEditor(typeof(SkinnedMeshRenderer), true)]
public class ClothingAttacher : Editor
{
    public override void OnInspectorGUI()
    {
        SkinnedMeshRenderer t = (target as SkinnedMeshRenderer);
        // TODO: Fix: The UI that is drawn looks off
        base.OnInspectorGUI();
        if (GUILayout.Button("Attach"))
        {
            Transform root = t.transform.parent;
            t.bones = SetupBones(t, root);
            t.rootBone = root.transform.Find("Armature").Find("Hips");
            t.sharedMesh.RecalculateBounds();
        }
    }
    private Transform[] SetupBones(SkinnedMeshRenderer renderer, Transform newRoot)
    {
        Transform[] bones = renderer.bones;
        for (int i = 0; i < renderer.bones.Length; i++)
        {
            Transform foundBone = FindBone(newRoot, renderer.bones[i]);
            if (foundBone != null)
            {
                Debug.Log("Assigning: " + foundBone.name);
                bones[i] = foundBone;
            }
            else
            {
                Debug.LogError(renderer.bones[i].name + " not found");
                Transform parentOfNotFoundBone = FindBone(newRoot, renderer.bones[i].parent);
                renderer.bones[i].SetParent(parentOfNotFoundBone,false);
            }
        }
        return bones;
    }
    private Transform FindBone(Transform root, Transform bone)
    {
        foreach (Transform child in root)
        {
            Debug.Log("At " + child.name);
            if (child.name == bone.name)
                return child;
            else
            {
                Transform foundBone = FindBone(child, bone);
                if (foundBone != null)
                    return foundBone;
                else
                    continue;
            }
        }
        return null;
    }
}
