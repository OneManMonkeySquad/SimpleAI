using System;
using GameCore;
using UnityEngine;

namespace SimpleAI.Perception {
    [CreateAssetMenu(menuName = "SimpleAI/PerceptionSettings")]
    [Serializable]
    public class AIPerceptionSettings : ScriptableObject {
        [SerializeReference]
        [SelectImplementation(typeof(ISense))]
        public ISense[] Senses;
        public float UpdateRate = 1;

        void OnValidate() {
            foreach (var sense in Senses) {
                if (sense == null) {
                    Debug.LogError("null", this);
                }
            }
        }
    }
}