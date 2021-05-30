#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SimpleAI {
    public class AgentLogWindow : EditorWindow, IAIListener {
        public static AgentLogWindow Instance;

        GUIStyle style = new GUIStyle();
        Vector2 scrollPos;
        string t;

        [MenuItem("Tools/SimpleAI/AIAgent Log")]
        static void Init() {
            if (Instance == null) {
                Instance = GetWindow<AgentLogWindow>("SimpleAI AIAgent Log");
                Instance.style.richText = true;
                Instance.style.normal.textColor = Color.white;
            }
            Instance.Show();
        }

        public void Log(string text) {
            t += text;
            scrollPos.y = float.PositiveInfinity;

            Repaint();
        }

        void OnGUI() {
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
#endif