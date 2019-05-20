using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(PipeManager))]
public class PipeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PipeManager pipeManager = (PipeManager)target;
        if (GUILayout.Button("Draw Pipes"))
        {
            pipeManager.DrawLevel();
        }
    }
}