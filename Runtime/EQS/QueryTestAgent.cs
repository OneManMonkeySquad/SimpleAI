using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SimpleAI.EQS {
#if UNITY_EDITOR
    [AddComponentMenu("AI/EQSQueryTestAgent")]
    public class QueryTestAgent : MonoBehaviour {
        public Query Query;
        public Transform[] Targets;

        [MenuItem("Tools/SimpleAI/Create EQS Query Test Agent")]
        static void CreateAgent() {
            var pos = SceneView.lastActiveSceneView.camera.transform.TransformPoint(Vector3.forward * 5);

            var go = new GameObject("EQS Query Test Agent");
            go.transform.position = pos;

            var target = new GameObject("EQS Query Test Target");
            target.transform.position = pos;

            var qta = go.AddComponent<QueryTestAgent>();
            qta.Targets = new Transform[] { target.transform };

            Selection.activeGameObject = go;
        }

        void OnDrawGizmos() {
            if (Query == null)
                return;

            var ctx = new QueryRunContext() {
                Querier = new Vector3[] { transform.position },
                Target = Targets.Select(t => t.position).ToArray()
            };

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.color = Color.red;
            foreach (var target in Targets) {

                Gizmos.DrawWireSphere(target.position, 0.3f);
            }

            Query.Execute(QueryRunMode.All, ctx, item => {
                Color c = Color.blue;

                var isValidItem = item.Score >= 0.01f;
                if (isValidItem) {
                    c = Color.Lerp(Color.red, Color.green, item.Score);
                }

                Gizmos.color = c;
                Gizmos.DrawSphere(item.Point, 0.25f);

                if (isValidItem) {
                    Handles.Label(item.Point - Vector3.up / 3, ((int)(item.Score * 100)).ToString());
                }
            });
        }
    }
#endif
}