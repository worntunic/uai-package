using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UAI.AI.Edit
{
    [CustomEditor(typeof(UAIGraphData))]
    public class UAIGraphDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {

            //base.OnInspectorGUI();
            UAIGraphData graphData = (UAIGraphData)target;
            if (GUILayout.Button("Load Graph Editor"))
            {
                UtilityGraphWindow.OpenEditorWindow(graphData);
            }
            if (DrawDefaultInspector())
            {
            }


        }
    }
}

