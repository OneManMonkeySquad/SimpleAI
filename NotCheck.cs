using UnityEngine;

namespace SimpleAI {
    public class NotCheck : ICheckBase {
        [SerializeReference]
        [SerializeReferenceButton]
        public ICheckBase Check;

        public bool Foo(IContext ctx) {
            return !Check.Foo(ctx);
        }
    }
}