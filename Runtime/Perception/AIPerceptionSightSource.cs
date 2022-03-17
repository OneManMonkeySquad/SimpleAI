using UnityEngine;

namespace SimpleAI.Perception {
    [AddComponentMenu("SimpleAI/PerceptionSightSource")]
    public class AIPerceptionSightSource : MonoBehaviour {
        [Tooltip("If not than the source has to be registered via script.")]
        public bool RegisterOnStart = true;
        public AIPerceptionGroup Group;

        public void Register() => AISightSense.AddSource(this);
        public void Unregister() => AISightSense.RemoveSource(this);

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