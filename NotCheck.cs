using CareBoo.Serially;
using UnityEngine;

namespace SimpleAI {
    public class NotCheck : ICheckBase {
        [SerializeReference]
        [ShowSerializeReference]
        public ICheckBase Check;

        public bool Evaluate(IContext ctx) {
            return !Check.Evaluate(ctx);
        }

        public override string ToString() {
            return $"!({Check})";
        }
    }
}