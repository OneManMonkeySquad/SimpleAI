using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleAI {
    [CustomPropertyDrawer(typeof(Consideration))]
    public class ConsiderationEditor : PropertyDrawer {
        static float lineHeight => EditorGUIUtility.singleLineHeight * (EditorGUIUtility.wideMode ? 1 : 2);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return lineHeight * 4;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var input = property.FindPropertyRelative("input");
            var Idx = property.FindPropertyRelative("Idx");
            var curve = property.FindPropertyRelative("curve");

            EditorGUI.BeginProperty(position, label, property);

            {
                var targetObject = property.serializedObject.targetObject;

                // Code from hell: Resolve ScriptableObject Action type to Action<T> where T:IContext
                // Then instantiate the found type T (IContext) and call GetConsiderationDescriptions
                // to present the user with a list of possible considerations
                var t = targetObject.GetType();
                while (t != typeof(System.Object) && !t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBoundToContextType<>))) {
                    t = t.BaseType;
                }

                if (t != typeof(System.Object)) {
                    var boundToCtx = t.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IBoundToContextType<>));
                    var ctxType = boundToCtx.GetGenericArguments()[0];
                    var ctxTempInstance = (IContext)Activator.CreateInstance(ctxType);

                    var descs = ctxTempInstance.GetConsiderationDescriptions();
                    var indices = Enumerable.Range(0, descs.Length + 1).ToArray();

                    Idx.intValue = EditorGUI.IntPopup(new Rect(position.x, position.y, position.width, lineHeight), Idx.intValue, descs, indices);
                } else {
                    EditorGUI.LabelField(new Rect(position.x, position.y, position.width, lineHeight), "<Can't resolve, no IBoundToContextType derived base class found>");
                }
            }

            curve.animationCurveValue = EditorGUI.CurveField(new Rect(position.x, position.y + lineHeight, position.width, position.height - lineHeight), curve.animationCurveValue);

            EditorGUI.EndProperty();
        }
    }
}