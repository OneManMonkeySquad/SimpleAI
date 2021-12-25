using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI {
#if UNITY_EDITOR
    public interface IAIListener {
        void LogLine(string text);
    }

    public static class AIDebugger {
        public static IContext CurrentDebugTarget;
        public static IAIListener Active;

        public static void LogLine(IContext ctx, string text) {
            Assert.IsTrue(ctx == CurrentDebugTarget && Active != null);
            Active.LogLine(text);
        }
    }
#endif

    public interface IContext {
        MonoBehaviour CoroutineTarget { get; }

        float GetCurrentConsiderationScore(int considerationIdx);
#if UNITY_EDITOR
        /// Used by the inspector to display possible considerations.
        string[] GetConsiderationDescriptions();
#endif
    }

    /// Used to resolve the context type in the Consideration inspector.
    /// This is not the interface you are looking for.
    public interface IBoundToContextType<T> where T : IContext {
    }
}