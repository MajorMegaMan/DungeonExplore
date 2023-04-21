using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorFindObject : EditorWindow
{
    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;

    // Add menu named "My Window" to the Window menu
    [MenuItem("Custom/My Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        EditorFindObject window = (EditorFindObject)EditorWindow.GetWindow(typeof(EditorFindObject));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField("Text Field", myString);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();

        if (GUILayout.Button("Find Object"))
        {
            Selection.activeObject = GameObject.Find(myString);
        }

        if (GUILayout.Button("Find Debug Object"))
        {
            var debugObj = FindObjectOfType<DebugMessages>();
            if (debugObj != null)
            {
                Selection.activeObject = debugObj.gameObject;
            }
        }
    }
}
