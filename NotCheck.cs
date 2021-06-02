using UnityEngine;

namespace SimpleAI {
    public class NotCheck : ICheckBase {
        [SerializeReference]
        [SerializeReferenceButton]
        public ICheckBase Check;

        public bool Evaluate(IContext ctx) {
            return !Check.Evaluate(ctx);
        }
    }
}