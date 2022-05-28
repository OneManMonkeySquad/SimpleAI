using UnityEditor;
using UnityEngine;

namespace SimpleAI.EQS {
#if UNITY_EDITOR
    [AddComponentMenu("SimpleAI/EQSQueryTestAgent")]
    public class QueryTestAgent : MonoBehaviour {
        public Query Query;
        public Transform Target;

        [MenuItem("GameObject/SimpleAI/Environment Query Test Agent", false, 10)]
        static void CreateAgent() {
            var pos = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * 5);

            var go = new GameObject("EQS Query Test Agent");
            go.transform.position = pos;

            var target = new GameObject("EQS Query Test Target");
            target.transform.position = pos;

            var qta = go.AddComponent<QueryTestAgent>();
            qta.Target = target.transform;

            Selection.activeGameObject = go;
        }

        void OnDrawGizmos() {
            if (Query == null)
                return;

            var ctx = new QueryRunContext() {
                Querier = transform.position,
                Target = Target.position
            };

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Target.position, 0.3f);

            Query.Execute(QueryRunMode.All, ctx, DebugDrawItem);
        }

        public static void DebugDrawItem(Item item) {
            Color c = Color.blue;

            var isValidItem = item.Score >= 0.01f;
            if (isValidItem) {
                c = Color.Lerp(Color.red, Color.green, item.Score);
            }

            Handles.color = c;
            Handles.SphereHandleCap(0, item.Point, Quaternion.identity, 0.25f, EventType.Repaint);

            if (isValidItem) {
                Handles.Label(item.Point - Vector3.up / 3, ((int)(item.Score * 100)).ToString());
            }
        }
    }
#endif
}