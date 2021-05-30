#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace SimpleAI {
    [CustomPropertyDrawer(typeof(SerializeReferenceButtonAttribute))]
    public class SerializeReferenceButtonAttributeDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var labelPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(labelPosition, label);
            property.DrawSelectionButtonForManagedReference(position);

            EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }
    }
}

#endif