using System.Text;
using UnityEditor;
using UnityEngine;

namespace SimpleAI {
    [InitializeOnLoad]
    public class AgentLogHook {
        static AgentLogHook() {
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        static void PlayModeStateChanged(PlayModeStateChange stateChange) {
            if (stateChange == PlayModeStateChange.EnteredPlayMode && AgentLogWindow.Instance != null) {
                AgentLogWindow.Instance.OnEnterPlayMode();
            }
        }
    }

    public class AgentLogWindow : EditorWindow, IAIListener {
        public static AgentLogWindow Instance => Object.FindObjectOfType<AgentLogWindow>();

        Vector2 scrollPos;
        StringBuilder logLines = new StringBuilder();
        bool log = true;
        bool context = true;
        bool clearOnPlay = true;
        bool clearOnSwitchAgent;
        Vector2 typesScrollPos;

        [MenuItem("Tools/SimpleAI/AIAgent Log")]
        static void Init() {
            var instance = GetWindow<AgentLogWindow>("SimpleAI AIAgent Log");
            instance.Show();
        }

        public void LogLine(string text) {
            var linesToKeep = 1000;
            if (logLines.Length > linesToKeep) {
                var off = 0;
                for (; off < linesToKeep - 1; ++off) {
                    if (logLines[off] == '\n' && logLines[off + 1] == '\n')
                        break;
                }

                logLines.Remove(off, System.Math.Min(logLines.Length, linesToKeep - off));
            }

            logLines.AppendLine(text);
            scrollPos.y = float.PositiveInfinity;

            Repaint();
        }

        public void OnEnterPlayMode() {
            if (!clearOnPlay)
                return;

            logLines.Clear();
            Repaint();
        }

        GUIStyle GetStyle(string styleName) {
            return GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
        }

        internal static bool DropDownToggle(ref bool toggled, GUIContent content, GUIStyle toggleButtonStyle) {
            Rect toggleRect = GUILayoutUtility.GetRect(content, toggleButtonStyle);
            Rect arrowRightRect = new Rect(toggleRect.xMax - toggleButtonStyle.padding.right, toggleRect.y, toggleButtonStyle.padding.right, toggleRect.height);
            bool clicked = EditorGUI.DropdownButton(arrowRightRect, GUIContent.none, FocusType.Passive, GUIStyle.none);

            if (!clicked) {
                toggled = GUI.Toggle(toggleRect, toggled, content, toggleButtonStyle);
            }

            return clicked;
        }

        GUIStyle textStyle;

        void OnGUI() {
            AIDebugger.Active = this;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            bool clearClicked = false;
            if (DropDownToggle(ref clearClicked, new GUIContent("Clear"), GetStyle("toolbarDropDownToggle"))) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Clear On Play"), clearOnPlay, () => { clearOnPlay = !clearOnPlay; });
                var rect = GUILayoutUtility.GetLastRect();
                rect.y += EditorGUIUtility.singleLineHeight;
                menu.DropDown(rect);
            }
            if (clearClicked) {
                logLines.Clear();
                GUIUtility.keyboardControl = 0;
            }

            GUILayout.Space(5);

            log = GUILayout.Toggle(log, "Log", EditorStyles.toolbarButton);
            context = GUILayout.Toggle(context, "Ctx", EditorStyles.toolbarButton);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var content = EditorGUILayout.BeginHorizontal();

            if (log) {
                if (textStyle == null) {
                    textStyle = new GUIStyle();
                    textStyle.richText = true;
                    textStyle.normal.textColor = Color.white;
                }

                EditorGUILayout.BeginVertical();
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                GUILayout.Label(logLines.ToString(), textStyle);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            if (context) {
                EditorGUILayout.BeginVertical(GUILayout.Height(content.height / 2));
                typesScrollPos = GUILayout.BeginScrollView(typesScrollPos);

                if (AIDebugger.CurrentDebugTarget != null) {
                    GUILayout.Label(AIDebugger.CurrentDebugTarget.ToString());
                }

                GUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}