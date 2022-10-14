using System.Collections;
using System.Threading.Tasks;

namespace SimpleAI {
    public static class TaskExtensions {
        public static IEnumerator AsIEnumerator(this Task task) {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                throw task.Exception;
        }

        public static IEnumerator AsIEnumeratorNonThrowing(this Task task) {
            while (!task.IsCompleted)
                yield return null;
        }
    }
}