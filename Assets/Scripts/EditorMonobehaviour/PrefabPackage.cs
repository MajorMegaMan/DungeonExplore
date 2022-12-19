using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

[ExecuteInEditMode]
public class PrefabPackage : MonoBehaviour
{
    // Using Update rather than awake or start as the gameobject still needs to fully instantiate before it can be deleted.
    private void Update()
    {
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null)
        {
            AddToScene();
        }
    }

    void AddToScene()
    {
        UnityEditor.PrefabUtility.UnpackPrefabInstance(gameObject, UnityEditor.PrefabUnpackMode.OutermostRoot, UnityEditor.InteractionMode.AutomatedAction);

        List<Transform> children = new List<Transform>();
        int count = transform.childCount;
        for(int i = 0; i < count; i++)
        {
            children.Add(transform.GetChild(i));
        }

        for (int i = 0; i < children.Count; i++)
        {
            children[i].parent = transform.parent;
        }

        DestroyImmediate(gameObject);
    }
}
