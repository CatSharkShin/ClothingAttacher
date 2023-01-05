using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using UnityEditor.Animations;

namespace FoxClient
{
    public class CatsManager : EditorWindow
    {
        [MenuItem("Tools/CatsManager")]
        public static void ShowWindow()
        {
            GetWindow<CatsManager>("CatsManager");
        }
        GUIStyle H1 = new GUIStyle();

        // Attacher
        GameObject avatar;
        GameObject clothing;
        string postfix = "_X";
        bool keepGlobalPos = false;

        // Info
        int removedCount = 0;
        List<string> attachedClothes = new List<string>();
        string maxPostfix = "";

        private void OnLostFocus()
        {
            attachedClothes = new List<string>();
            removedCount = 0;

        }
        private void OnSelectionChange()
        {
            CheckForPostFix();
        }
        private void CheckForPostFix()
        {
            if (clothing != null)
            {
                Dictionary<string, int> count = new Dictionary<string, int>();
                foreach (Transform child in GetChildren(clothing.transform))
                {
                    foreach (string part in child.name.Split('_'))
                    {
                        foreach (string partpart in part.Split(' '))
                            if (count.ContainsKey(partpart))
                                count[partpart] = count[partpart] + 1;
                            else
                                count[partpart] = 1;
                    }
                }
                string max = count.Keys.ToList()[0];
                foreach (KeyValuePair<string, int> frequency in count)
                {
                    if (frequency.Value > count[max])
                        max = frequency.Key;
                }
                if (count[max] > 1)
                    maxPostfix = max;
                else
                    maxPostfix = "";
            }
            else
                maxPostfix = "";
        }
        private void OnHierarchyChange()
        {
            CheckForPostFix();
        }
        private void OnGUI()
        {
            H1.fontSize = 24;

            avatar = EditorGUILayout.ObjectField(new GUIContent("Avatar"), avatar, typeof(GameObject), true) as GameObject;
            if (avatar == null)
            {
                EditorGUILayout.HelpBox("You need to put in an avatar", MessageType.Error);
            }

            
            StartCenter();
            GUILayout.Label("Clothing attacher", H1);
            EndCenter();

            clothing = EditorGUILayout.ObjectField(new GUIContent("Clothing Prefab"), clothing, typeof(GameObject), true) as GameObject;

            Transform armature = null;
            if (clothing != null)
            {
                armature = clothing.transform.Find("Armature");
                if (armature == null)
                {
                    foreach (Transform child in clothing.transform)
                    {
                        if (child.name.ToLower().Contains("armature"))
                            armature = child;
                    }

                    if (armature == null)
                    {
                        EditorGUILayout.HelpBox("Armature not found under the clothing prefab", MessageType.Error);
                    }
                }
            }

            StartAlignLeft();
            GUILayout.Label("PostFix");
            postfix = GUILayout.TextField(postfix, GUILayout.MinWidth(Screen.width / 3));

            if (maxPostfix != "")
                EditorGUILayout.HelpBox($"Probable postfix: _{maxPostfix}", MessageType.Error);
            if (GUILayout.Button("Fix Named Bones"))
            {
                removedCount = 0;
                foreach (Transform bone in GetChildren(armature))
                {
                    if (bone.name.EndsWith(postfix))
                    {
                        removedCount++;
                        bone.name = bone.name.Remove(bone.name.Length - postfix.Length, postfix.Length);
                    }
                }
                CheckForPostFix();
            }
            EndAlignLeft();

            StartAlignLeft();
            EditorGUILayout.HelpBox("When bones are named 'Hips_X', Leg_X, Knee_X,\n" +
                                    "The postfixes(_X) need to be removed"
                                    , MessageType.Info, true);
            EndAlignLeft();

            if (removedCount > 0)
                EditorGUILayout.HelpBox($"Fixed {removedCount} bones"
                                        , MessageType.Info, true);

            keepGlobalPos = GUILayout.Toggle(keepGlobalPos, "Keep Global Positions");

            if (GUILayout.Button("Attach Clothing"))
            {
                attachedClothes = new List<string>();
                if (clothing != null && armature != null)
                {
                    List<SkinnedMeshRenderer> movedClothes = new List<SkinnedMeshRenderer>();

                    // Fix Model Import scale
                    foreach (SkinnedMeshRenderer cloth in clothing.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        ModelImporter mi = (ModelImporter)ModelImporter.GetAtPath(AssetDatabase.GetAssetPath(cloth.sharedMesh));
                        float scaleDifference = armature.localScale.x / avatar.transform.Find("Armature").localScale.x;
                        mi.globalScale = scaleDifference;
                        EditorUtility.SetDirty(mi);
                        mi.SaveAndReimport();
                    }

                    // Unpack Prefab
                    if (PrefabUtility.IsPartOfAnyPrefab(clothing))
                        PrefabUtility.UnpackPrefabInstance(clothing, PrefabUnpackMode.Completely, InteractionMode.UserAction);


                    foreach (SkinnedMeshRenderer cloth in clothing.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        if (cloth.GetComponents<Component>().Length > 2)
                            continue;
                        float scaleDifference = armature.localScale.x / avatar.transform.Find("Armature").localScale.x;
                        cloth.transform.parent = avatar.transform;
                        cloth.bones = SetupBones(cloth, avatar.transform, keepGlobalPos);
                        movedClothes.Add(cloth);
                        attachedClothes.Add(cloth.name);
                        cloth.rootBone = avatar.transform.Find("Armature").transform.Find("Hips");
                    }
                    DestroyImmediate(clothing);
                    foreach (var cloth in movedClothes)
                    {
                        cloth.localBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(3, 3, 3));
                    }
                }
                CheckForPostFix();
            }
            if (clothing == null)
            {
                EditorGUILayout.HelpBox("You need to put in a clothing prefab", MessageType.Error);
            }
            foreach (var clothName in attachedClothes)
            {
                EditorGUILayout.HelpBox($"Attached {clothName}"
                                        , MessageType.Info, true);
            }
            if (GUI.changed)
            {
                CheckForPostFix();
            }
        }
        public void StartAlignLeft()
        {
            GUILayout.BeginHorizontal();
        }
        public void EndAlignLeft()
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        public void StartCenter()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
        }
        public void EndCenter()
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        public IEnumerable<Transform> GetChildren(Transform root)
        {
            foreach (Transform child in root)
            {
                yield return child;
                foreach (Transform childchild in GetChildren(child))
                {
                    yield return childchild;
                }
            }
        }

        private Transform[] SetupBones(SkinnedMeshRenderer renderer, Transform newRoot, bool keepGlobal)
        {
            //Debug.Log("Setting up bones for "+ renderer.transform.name);
            Transform[] bones = renderer.bones;
            for (int i = 0; i < renderer.bones.Length; i++)
            {
                Transform foundBone = FindBone(newRoot, renderer.bones[i]);
                if (foundBone != null)
                {
                    //Debug.Log("Assigning: " + foundBone.name);
                    bones[i] = foundBone;
                }
                else
                {
                    Debug.LogWarning(renderer.bones[i].name + " not found");
                    Transform parentOfNotFoundBone = FindBone(newRoot, renderer.bones[i].parent);
                    renderer.bones[i].SetParent(parentOfNotFoundBone, keepGlobal);
                    //renderer.bones[i].position *= scaleDifference;
                }
            }
            return bones;
        }
        private Transform FindBone(Transform root, Transform bone)
        {
            foreach (Transform child in root)
            {
                //Debug.Log("At " + child.name);
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
}