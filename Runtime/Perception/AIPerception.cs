using System.Collections.Generic;
using UnityEngine;

namespace SimpleAI.Perception {
    [AddComponentMenu("SimpleAI/Perception")]
    public class AIPerception : MonoBehaviour {
        public AIPerceptionSettings Settings;

        public IEnumerable<ISenseListener> Listeners => _listeners;
        public Transform View;

        List<ISenseListener> _listeners = new List<ISenseListener>();

        float _nextUpdateTime;

        public void AddListener(ISenseListener listener) => _listeners.Add(listener);
        public void RemoveListener(ISenseListener listener) => _listeners.Remove(listener);

        public void UpdateSenses() {
            _nextUpdateTime = Time.time + Settings.UpdateRate * UnityEngine.Random.Range(1f, 1.2f); // Stagger updates

            if (_listeners.Count == 0)
                return;

            foreach (var sense in Settings.Senses) {
                sense.Update(this, _listeners);
            }
        }

        void OnEnable() {
            foreach (var sense in Settings.Senses) {
                sense.Add(this);
            }
        }

        void OnDisable() {
            foreach (var sense in Settings.Senses) {
                sense.Remove(this);
            }
        }

        void Update() {
            if (Time.time < _nextUpdateTime)
                return;

            UpdateSenses();
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            foreach (var sense in Settings.Senses) {
                sense.DebugDraw(this);
            }
        }
#endif
    }
}