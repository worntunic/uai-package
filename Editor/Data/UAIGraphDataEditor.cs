using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UAI.AI.Edit
{
    [CustomEditor(typeof(UAIGraphData))]
    public class UAIGraphDataEditor : Editor
    {
        SerializedProperty contextProp;
        private void OnEnable()
        {
            contextProp = serializedObject.FindProperty("context");
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //base.OnInspectorGUI();
            EditorGUILayout.PropertyField(contextProp);
            UAIGraphData graphData = (UAIGraphData)target;
            GUI.enabled = graphData.context != null;
            if (GUILayout.Button("Load Graph Editor"))
            {
                UtilityGraphWindow.OpenEditorWindow(graphData);
            }
            GUI.enabled = true;
            serializedObject.ApplyModifiedProperties();
            /*if (DrawDefaultInspector())
            {
            }*/


        }
    }
}

