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

        Vector2 _scrollPos;
        StringBuilder _logLines = new StringBuilder();
        bool _log = true;
        bool _context = true;
        bool _clearOnPlay = true;
        bool _clearOnSwitchAgent;
        Vector2 _typesScrollPos;

        [MenuItem("Tools/SimpleAI/AIAgent Log")]
        static void Init() {
            var instance = GetWindow<AgentLogWindow>("SimpleAI AIAgent Log");
            instance.Show();
        }

        public void LogLine(string text) {
            var linesToKeep = 1000;
            if (_logLines.Length > linesToKeep) {
                var off = 0;
                for (; off < linesToKeep - 1; ++off) {
                    if (_logLines[off] == '\n' && _logLines[off + 1] == '\n') {
                        _logLines.Remove(0, off + 1);
                        break;
                    }
                }
            }

            _logLines.AppendLine(text);
            _scrollPos.y = float.PositiveInfinity;

            Repaint();
        }

        public void OnEnterPlayMode() {
            if (!_clearOnPlay)
                return;

            _logLines.Clear();
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
                menu.AddItem(new GUIContent("Clear On Play"), _clearOnPlay, () => { _clearOnPlay = !_clearOnPlay; });
                var rect = GUILayoutUtility.GetLastRect();
                rect.y += EditorGUIUtility.singleLineHeight;
                menu.DropDown(rect);
            }
            if (clearClicked) {
                _logLines.Clear();
                GUIUtility.keyboardControl = 0;
            }

            GUILayout.Space(5);

            _log = GUILayout.Toggle(_log, "Log", EditorStyles.toolbarButton);
            _context = GUILayout.Toggle(_context, "Ctx", EditorStyles.toolbarButton);

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            var content = EditorGUILayout.BeginHorizontal();

            if (_log) {
                if (textStyle == null) {
                    textStyle = new GUIStyle();
                    textStyle.richText = true;
                    textStyle.normal.textColor = Color.white;
                }

                EditorGUILayout.BeginVertical();
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
                var text = _logLines.ToString();
                if (text.Length == 0 && AIDebugger.CurrentDebugTarget == null) {
                    text = "<set AIDebugger.CurrentDebugTarget at runtime>";
                }
                GUILayout.Label(text, textStyle);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            if (_context) {
                EditorGUILayout.BeginVertical(GUILayout.Height(content.height / 2));
                _typesScrollPos = GUILayout.BeginScrollView(_typesScrollPos);

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