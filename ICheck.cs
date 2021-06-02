namespace SimpleAI {
    public interface ICheckBase {
        bool Evaluate(IContext ctx);
    }

    public abstract class Check<T> : ICheckBase {
        public bool Evaluate(IContext ctx) => Foo((T)ctx);

        public abstract bool Foo(T ctx);
    }
}