using System.Collections.Generic;
using UnityEngine;

namespace SimpleAI.Perception {
    public interface ISense {
        void Add(AIPerception perception);
        void Update(AIPerception perception, IEnumerable<ISenseListener> listeners);
        void Remove(AIPerception perception);
#if UNITY_EDITOR
        void DebugDraw(AIPerception perception);
#endif
    }

    public interface ISenseListener {
        void OnSensed(GameObject source);
        void OnDeathSensed(GameObject source, GameObject killer);
        void OnStealingSensed(GameObject source);
    }

    class SenseDebugDrawer : ISenseListener {
        Transform _view;

        public SenseDebugDrawer(Transform view) {
            _view = view;
        }

        public void OnSensed(GameObject source) {
            Debug.DrawLine(_view.position, source.transform.position, Color.green);
        }

        public void OnStealingSensed(GameObject source) { }

        public void OnDeathSensed(GameObject source, GameObject killer) { }
    }
}