using UnityEditor;
using UnityEngine;

namespace SimpleAI {
    public class AgentLogWindow : EditorWindow, IAIListener {
        GUIStyle style = new GUIStyle();
        Vector2 scrollPos;
        string t;

        [MenuItem("Tools/SimpleAI/AIAgent Log")]
        static void Init() {
            var instance = GetWindow<AgentLogWindow>("SimpleAI AIAgent Log");

            instance.Show();
        }

        public void LogLine(string text) {
            t += text + "\n";
            scrollPos.y = float.PositiveInfinity;

            Repaint();
        }

        void OnGUI() {
            AIDebugger.Active = this;

            style.richText = true;
            style.normal.textColor = Color.white;

            EditorGUILayout.BeginHorizontal();
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Label(t, style);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear")) {
                t = "";
            }
        }
    }
}