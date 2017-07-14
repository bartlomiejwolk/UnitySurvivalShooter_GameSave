using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CompleteProject
{
    [CustomEditor(typeof(SaveManager))]
    public class SaveManagerEditor : Editor
    {
        private SaveManager Script { get; set; }

        private void OnEnable()
        {
            Script = (SaveManager)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();

            DrawSaveButton();
            DrawLoadButton();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawSaveButton()
        {
            GUIContent label = new GUIContent("Save");
            bool savePressed = GUILayout.Button(label, GUILayout.Width(100));

            if (savePressed)
            {
                Script.Save();
            }
        }

        private void DrawLoadButton()
        {
            GUIContent label = new GUIContent("Load");
            bool loadPressed = GUILayout.Button(label, GUILayout.Width(100));

            if (loadPressed)
            {
                Script.Load();
            }
        }

    }

}
