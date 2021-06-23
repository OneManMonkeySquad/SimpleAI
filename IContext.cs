using UnityEngine;

namespace SimpleAI {
#if UNITY_EDITOR
    public interface IAIListener {
        void LogLine(string text);
    }

    public static class AIDebugger {
        public static IAIListener Active;
    }
#endif

    public interface IContext {
        MonoBehaviour CoroutineTarget { get; }
#if UNITY_EDITOR
        IAIListener Listener { get; }
#endif

        float GetCurrentConsiderationScore(int considerationIdx);
        /// Used by the inspector to display possible considerations.
        string[] GetConsiderationDescriptions();
    }

    /// Used to resolve the context type in the Consideration inspector.
    /// You don't need to use this.
    public interface IBoundToContextType<T> where T : IContext, new() {
    }
}