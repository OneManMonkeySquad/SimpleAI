using System;
using System.Collections.Generic;
using DataStructures.ViliWonka.KDTree;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI.Perception {
    [Serializable]
    public class AISoundSense : ISense {
        public LayerMask Blocking;

        static List<AIPerception> _receivers = new();
        static List<LayerMask> _blocking = new();
        static int _sourceBuildAtFrame;
        static int _sourceBuildForCount;

        static KDTree _tree = new KDTree();
        static List<Vector3> _points = new List<Vector3>();
        static List<int> _results = new List<int>();
        static KDQuery _query = new KDQuery();

        public static void TriggerSensedAt(GameObject source, Vector3 position, float radius) {
            Assert.IsNotNull(source);

            var listeners = GetListenersInRange(position, radius);
            foreach (var listener in listeners) {
                listener.OnSensed(source);
            }
        }

        public static void TriggerDeathAt(GameObject source, GameObject killer, Vector3 position, float radius) {
            var listeners = GetListenersInRange(position, radius);
            foreach (var listener in listeners) {
                listener.OnDeathSensed(source, killer);
            }
        }

        static IEnumerable<ISenseListener> GetListenersInRange(Vector3 position, float radius) {
            if (_sourceBuildAtFrame != Time.frameCount || _sourceBuildForCount != _receivers.Count) {
                _sourceBuildAtFrame = Time.frameCount;
                _sourceBuildForCount = _receivers.Count;

                _points.Clear();
                for (int i = 0; i < _receivers.Count; ++i) {
                    _points.Add(_receivers[i].transform.position);
                }
                _tree.Build(_points);
            }

            _results.Clear();
            _query.Radius(_tree, position, radius, _results);

            for (int i = 0; i < _results.Count; ++i) {
                var sourceIdx = _results[i];
                var perception = _receivers[sourceIdx];
                var blocking = _blocking[sourceIdx];

                var isBlocked = Physics.Linecast(position, perception.View.position, blocking);
                if (isBlocked)
                    continue;

                foreach (var listener in perception.Listeners) {
                    yield return listener;
                }
            }
        }

        public void Add(AIPerception perception) {
            Assert.IsNotNull(perception);

            _receivers.Add(perception);
            _blocking.Add(Blocking);
        }

        public void Remove(AIPerception perception) {
            Assert.IsNotNull(perception);

            var idx = _receivers.IndexOf(perception);
            if (idx != -1) {
                _receivers[idx] = _receivers[_receivers.Count - 1];
                _receivers.RemoveAt(_receivers.Count - 1);

                _blocking[idx] = _blocking[_blocking.Count - 1];
                _blocking.RemoveAt(_blocking.Count - 1);
            }
        }

        public void Update(Transform view, IEnumerable<ISenseListener> listeners) {
        }

        public void DebugDraw(Transform view) {
        }
    }
}