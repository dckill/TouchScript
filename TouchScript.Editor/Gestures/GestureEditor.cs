﻿/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System;
using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TouchScript.Editor.Gestures
{
    [CustomEditor(typeof (Gesture))]
    public class GestureEditor : UnityEditor.Editor
    {
        public const string TEXT_FRIENDLY_HEADER = "Gestures which can work together with this gesture.";

        private const string FRIENDLY_GESTURES_PROPERTY_NAME = "friendlyGestureIds";

        private GUIStyle foldoutStyle, boxStyle, headerStyle, gestureLabelStyle;
        private SerializedProperty serializedGestures;

        private bool shouldRecognizeShown;

        protected virtual void OnEnable()
        {
            hideFlags = HideFlags.HideAndDontSave;
            serializedGestures = serializedObject.FindProperty(FRIENDLY_GESTURES_PROPERTY_NAME);
        }

        protected bool drawFoldout(bool open, GUIContent header, Action content)
        {
            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(GUI.skin.box);
                boxStyle.margin = new RectOffset(0, 0, 1, 0);
                boxStyle.padding = new RectOffset(0, 0, 0, 0);
                boxStyle.contentOffset = new Vector2(0, 0);
                boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                boxStyle.alignment = TextAnchor.MiddleCenter;

                foldoutStyle = new GUIStyle(GUI.skin.FindStyle("ShurikenModuleBg"));
                foldoutStyle.padding = new RectOffset(10, 10, 10, 10);
//                foldoutStyle.margin = new RectOffset(-10, 0, 0, 0);
                foldoutStyle.contentOffset = new Vector2(-10, 0);

                headerStyle = new GUIStyle(GUI.skin.FindStyle("ShurikenModuleTitle"));
                headerStyle.contentOffset = new Vector2(3, -2);

                gestureLabelStyle = new GUIStyle(GUI.skin.label);
                gestureLabelStyle.fontSize = 10;
                gestureLabelStyle.padding = new RectOffset(-10, 0, 4, 0);
            }

            EditorGUIUtility.LookLikeInspector();
            GUILayout.BeginVertical("ShurikenEffectBg");

            open = GUI.Toggle(GUILayoutUtility.GetRect(0, 16), open, header, headerStyle);
            if (open)
            {
                GUILayout.BeginVertical(foldoutStyle);

                content();

                GUILayout.EndVertical();
            }
            GUILayout.EndVertical();

            return open;
        }

        public override void OnInspectorGUI()
        {
            shouldRecognizeShown = drawFoldout(shouldRecognizeShown, new GUIContent("Friendly gestures", TEXT_FRIENDLY_HEADER), () =>
                {
                    Gesture toRemove = null;
                    GUILayout.BeginVertical();
                    for (int i = 0; i < serializedGestures.arraySize; i++)
                    {
                        SerializedProperty item = serializedGestures.GetArrayElementAtIndex(i);
                        var gesture = EditorUtility.InstanceIDToObject(item.intValue) as Gesture;

                        Rect rect = EditorGUILayout.BeginHorizontal(boxStyle, GUILayout.Height(23));
                        EditorGUILayout.LabelField(string.Format("{0} @ {1}", gesture.GetType().Name, gesture.name), gestureLabelStyle, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("remove", GUILayout.Width(60), GUILayout.Height(16)))
                        {
                            toRemove = gesture;
                        }
                        else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                        {
                            EditorGUIUtility.PingObject(gesture);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    if (serializedGestures.arraySize > 0) GUILayout.Space(9);

                    Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, boxStyle, GUILayout.ExpandWidth(true));
                    GUI.Box(dropArea, "Drag a Gesture Here", boxStyle);
                    switch (Event.current.type)
                    {
                        case EventType.DragUpdated:
                            if (dropArea.Contains(Event.current.mousePosition)) DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            break;
                        case EventType.DragPerform:
                            if (dropArea.Contains(Event.current.mousePosition))
                            {
                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                DragAndDrop.AcceptDrag();

                                foreach (Object obj in DragAndDrop.objectReferences)
                                {
                                    if (obj is GameObject)
                                    {
                                        var go = obj as GameObject;
                                        Gesture[] gestures = go.GetComponents<Gesture>();
                                        foreach (Gesture gesture in gestures)
                                        {
                                            addGesture(gesture);
                                        }
                                    }
                                    else if (obj is Gesture)
                                    {
                                        addGesture(obj as Gesture);
                                    }
                                }

                                Event.current.Use();
                            }
                            break;
                    }
                    if (toRemove != null)
                    {
                        removeGesture(toRemove);
                    }
                });

            serializedObject.ApplyModifiedProperties();
        }

        private void addGesture(Gesture value)
        {
            if (value == target) return;

            for (int i = 0; i < serializedGestures.arraySize; i++)
            {
                SerializedProperty item = serializedGestures.GetArrayElementAtIndex(i);
                if (item.intValue == value.GetInstanceID()) return;
            }

            serializedGestures.arraySize++;
            serializedGestures.GetArrayElementAtIndex(serializedGestures.arraySize - 1).intValue = value.GetInstanceID();

            var so = new SerializedObject(value);
            var prop = so.FindProperty(FRIENDLY_GESTURES_PROPERTY_NAME);
            prop.arraySize++;
            prop.GetArrayElementAtIndex(prop.arraySize - 1).intValue = target.GetInstanceID();

            serializedObject.ApplyModifiedProperties();
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(value);
        }

        private void removeGesture(Gesture value)
        {
            var id = value.GetInstanceID();
            for (int i = 0; i < serializedGestures.arraySize; i++)
            {
                SerializedProperty item = serializedGestures.GetArrayElementAtIndex(i);
                if (item.intValue == id)
                {
                    serializedGestures.DeleteArrayElementAtIndex(i);

                    var so = new SerializedObject(value);
                    so.Update();
                    var prop = so.FindProperty(FRIENDLY_GESTURES_PROPERTY_NAME);
                    id = target.GetInstanceID();
                    for (int j = 0; j < prop.arraySize; j++)
                    {
                        item = prop.GetArrayElementAtIndex(j);
                        if (item.intValue == id)
                        {
                            prop.DeleteArrayElementAtIndex(j);
                            break;
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(value);
                    break;
                }
            }
        }

    }
}