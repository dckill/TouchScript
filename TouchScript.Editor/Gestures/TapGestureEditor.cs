﻿using TouchScript.Gestures;
using UnityEditor;
using UnityEngine;

namespace TouchScript.Editor.Gestures
{

    [CustomEditor(typeof(TapGesture))]
    public class TapGestureEditor : GestureEditor
    {

        public const string TEXT_TIMELIMIT = "Tap fails if is pressed for too long.";
        public const string TEXT_DISTANCELIMIT = "Tap fails if fingers move too much.";
        public const string TEXT_COMBINETOUCHPOINTSINTERVAL = "When several fingers are used to perform a tap, touch points released not earlier than <Time - CombineInterval> are used to calculate gesture's final screen position. If set to 0, position of the last touch point is used.";

        private SerializedProperty timeLimit, distanceLimit, combineTouchPointsInterval;
        private bool advancedShown, useTimeLimit, useDistanceLimit;

        protected override void OnEnable()
        {
            base.OnEnable();

            timeLimit = serializedObject.FindProperty("timeLimit");
            distanceLimit = serializedObject.FindProperty("distanceLimit");
            combineTouchPointsInterval = serializedObject.FindProperty("combineTouchPointsInterval");

            Debug.Log(timeLimit.floatValue);
            useTimeLimit = !float.IsInfinity(timeLimit.floatValue);
            useDistanceLimit = !float.IsInfinity(distanceLimit.floatValue);
        }

        public override void OnInspectorGUI()
        {
            EditorGUIUtility.LookLikeInspector();

            var newTimelimit = GUILayout.Toggle(useTimeLimit, new GUIContent("Use Time Limit", TEXT_TIMELIMIT));
            if (newTimelimit)
            {
                if (newTimelimit != useTimeLimit) timeLimit.floatValue = 0;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(timeLimit, new GUIContent("Value (sec)"));
                EditorGUI.indentLevel--;
            }
            else
            {
                timeLimit.floatValue = float.PositiveInfinity;
            }
            useTimeLimit = newTimelimit;

            var newDistanceLimit = GUILayout.Toggle(useDistanceLimit, new GUIContent("Use Distance Limit", TEXT_DISTANCELIMIT));
            if (newDistanceLimit)
            {
                if (newDistanceLimit != useDistanceLimit) distanceLimit.floatValue = 0;
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(distanceLimit, new GUIContent("Value (cm)", TEXT_DISTANCELIMIT));
                EditorGUI.indentLevel--;
            }
            else
            {
                distanceLimit.floatValue = float.PositiveInfinity;
            }
            useDistanceLimit = newDistanceLimit;

            advancedShown = drawFoldout(advancedShown, new GUIContent("Advanced"), () =>
                {
                    EditorGUILayout.PropertyField(combineTouchPointsInterval, new GUIContent("Combine Interval (sec)", TEXT_COMBINETOUCHPOINTSINTERVAL));
                });

            base.OnInspectorGUI();
        }

    }
}
