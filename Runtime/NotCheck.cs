using GameCore;
using UnityEngine;

namespace SimpleAI {
    public class NotCheck : ICheckBase {
        [SerializeReference]
        [SelectImplementation(typeof(ICheckBase))]
        public ICheckBase Check;

        public bool Evaluate(IContext ctx) {
            return !Check.Evaluate(ctx);
        }

        public override string ToString() {
            return $"!({Check})";
        }
    }
}