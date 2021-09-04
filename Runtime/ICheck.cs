using UnityEngine;

namespace SimpleAI {
    public interface ICheckBase {
        bool Evaluate(IContext ctx);
    }

    public abstract class CheckBase : ScriptableObject, ICheckBase {
        public abstract bool Evaluate(IContext ctx);
    }

    public abstract class Check<T> : CheckBase, ICheckBase {
        public override bool Evaluate(IContext ctx) => Evaluate((T)ctx);

        public abstract bool Evaluate(T ctx);
    }
}