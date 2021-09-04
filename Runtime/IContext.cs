using UnityEngine;

namespace SimpleAI {
#if UNITY_EDITOR
    public interface IAIListener {
        void LogLine(string text);
    }

    public static class AIDebugger {
        public static IContext CurrentDebugTarget;
        public static IAIListener Active;

        public static void LogLine(IContext ctx, string text) {
            if (ctx != CurrentDebugTarget || Active == null)
                return;

            Active.LogLine(text);
        }
    }
#endif

    public interface IContext {
        MonoBehaviour CoroutineTarget { get; }

        float GetCurrentConsiderationScore(int considerationIdx);
        /// Used by the inspector to display possible considerations.
        string[] GetConsiderationDescriptions();
    }

    /// Used to resolve the context type in the Consideration inspector.
    /// You don't need to use this.
    public interface IBoundToContextType<T> where T : IContext, new() {
    }
}