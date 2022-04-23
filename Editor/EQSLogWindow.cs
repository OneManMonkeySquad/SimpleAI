using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SimpleAI.EQS {
    public class EQSLogWindow : EditorWindow, IEQSLogger {
        struct Entry {
            public Query Query;
            public QueryRunMode RunMode;
            public ResolvedQueryRunContext Ctx;
            public Item[] Items;
            public int? BestIdx;
            public double Time;
        }

        [SerializeField]
        List<string> _entryQueryName = new();
        [SerializeField]
        List<Entry> _entries = new();
        [SerializeField]
        int _selectedIdx;
        Vector2 _scrollPos;

        [MenuItem("Tools/SimpleAI/EQS Log")]
        static void Init() {
            var instance = GetWindow<EQSLogWindow>("SimpleAI EQS Log");
            instance.Show();
        }

        public void LogQuery(Query query, QueryRunMode mode, ResolvedQueryRunContext ctx, Span<Item> items, int? bestIdx) {
            _entries.Add(new Entry() {
                Query = query,
                RunMode = mode,
                Ctx = ctx,
                Items = items.ToArray(),
                BestIdx = bestIdx,
                Time = Time.timeAsDouble
            });
            _entryQueryName.Add(query.name);
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
                _entryQueryName.Clear();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(200));
                _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

                var activeButtonStyle = new GUIStyle(GUI.skin.button);
                activeButtonStyle.fontStyle = FontStyle.Bold;

                for (int i = 0; i < _entryQueryName.Count; ++i) {
                    var style = i != _selectedIdx ? GUI.skin.button : activeButtonStyle;
                    if (GUILayout.Button(_entryQueryName[i], style)) {
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
                    EditorGUILayout.LabelField($"Run Mode: {e.RunMode}");
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }
}