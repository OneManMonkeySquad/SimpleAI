using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimpleAI.EQS {
    public class EQSLogWindow : EditorWindow, IEQSLogger {
        struct Entry {
            public Query Query;
            public Item[] Items;
            public int? BestIdx;
            public double Time;
        }

        [SerializeField]
        List<Entry> _entries = new List<Entry>();
        [SerializeField]
        int _selectedIdx;
        Vector2 _scrollPos;

        [MenuItem("Tools/SimpleAI/EQS Log")]
        static void Init() {
            var instance = GetWindow<EQSLogWindow>("SimpleAI EQS Log");
            instance.Show();
        }

        public void Foo(Query query, Span<Item> items, int? bestIdx) {
            _entries.Add(new Entry() {
                Query = query,
                Items = items.ToArray(),
                BestIdx = bestIdx,
                Time = Time.timeAsDouble
            });
            Repaint();
        }

        void OnFocus() {
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDestroy() {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView) {
            if (_selectedIdx >= 0 && _selectedIdx < _entries.Count) {
                var e = _entries[_selectedIdx];
                foreach (var item in e.Items) {
                    QueryTestAgent.DebugDrawItem(item);
                }
            }
        }

        void OnGUI() {
            QuerySystem.ActiveLogger = this;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton)) {
                _entries.Clear();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(200));
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                var activeButtonStyle = new GUIStyle(GUI.skin.button);
                activeButtonStyle.fontStyle = FontStyle.Bold;

                for (int i = 0; i < _entries.Count; ++i) {
                    var e = _entries[i];

                    var style = i != _selectedIdx ? GUI.skin.button : activeButtonStyle;
                    if (GUILayout.Button(e.Query.name, style)) {
                        _selectedIdx = i;
                    }
                }
                GUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            {
                EditorGUILayout.BeginVertical();
                if (_selectedIdx >= 0 && _selectedIdx < _entries.Count) {
                    var e = _entries[_selectedIdx];
                    EditorGUILayout.LabelField($"Time: {e.Time:0.00}");
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }
}