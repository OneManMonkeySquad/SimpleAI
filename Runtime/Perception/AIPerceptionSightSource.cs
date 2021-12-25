using UnityEngine;

namespace SimpleAI.Perception {
    [AddComponentMenu("SimpleAI/PerceptionSightSource")]
    public class AIPerceptionSightSource : MonoBehaviour {
        public bool RegisterOnStart = true;

        public void Register() {
            AISightSense.AddSource(gameObject);
        }

        public void Unregister() {
            AISightSense.RemoveSource(gameObject);
        }

        void Start() {
            if (RegisterOnStart) {
                Register();
            }
        }

        void OnDisable() {
            Unregister();
        }
    }
}