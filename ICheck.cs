namespace SimpleAI {
    public interface ICheckBase {
        bool Foo(IContext ctx);
    }

    public abstract class Check<T> : ICheckBase {
        public bool Foo(IContext ctx) {
            return Foo((T)ctx);
        }

        public abstract bool Foo(T ctx);
    }
}