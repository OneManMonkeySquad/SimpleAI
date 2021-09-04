using UnityEngine;

namespace SimpleAI {
    [CreateAssetMenu(menuName = "AI/ActionSet")]
    public class ActionSet : ScriptableObject {
        public CheckBase[] Checks;
        public ActionBase[] actions;
        [Range(0.1f, 10f)]
        public float finalWeight = 1;
    }
}