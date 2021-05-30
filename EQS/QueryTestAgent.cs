using UnityEditor;
using UnityEngine;

namespace SimpleAI.EQS {
#if UNITY_EDITOR
    [AddComponentMenu("AI/EQSQueryTestAgent")]
    public class QueryTestAgent : MonoBehaviour {
        public Query Query;
        public Transform Target;

        void OnDrawGizmos() {
            if (!Application.isPlaying)
                return;

            var ctx = new QueryRunContext() {
                Querier = transform.position,
                Target = Target.position
            };
            Query.Execute(QueryRunMode.All, ctx, item => {
                Color c = Color.blue;
                if (item.Score >= 0.01f) {
                    c = Color.Lerp(Color.red, Color.green, item.Score);
                }
                Gizmos.color = c;
                Gizmos.DrawSphere(item.Point, 0.25f);
                Handles.Label(item.Point, ((int)(item.Score * 100)).ToString());
            });
        }
    }
#endif
}