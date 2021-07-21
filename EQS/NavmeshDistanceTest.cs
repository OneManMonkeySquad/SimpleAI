using System;
using UnityEngine;
using UnityEngine.AI;

namespace SimpleAI.EQS {
    [Serializable]
    public class NavmeshDistanceTest : ITest {
        public enum ScoreMode {
            PreferGreater,
            PreferLower,
            PeferExact
        }

        public QueryContext To;
        public int WalkableMask = NavMesh.AllAreas;

        [Header("Score")]
        [Range(1, 1000)]
        public float MaxDistance = 100;
        public ScoreMode Mode = ScoreMode.PreferGreater;

        public float RuntimeCost => 20;

        NavMeshPath path;

        public float Run(ref Item item, QueryRunContext ctx) {
            var from = item.Point;
            var to = ctx.Resolve(To);

            if (path == null) {
                path = new NavMeshPath();
            }

            path.ClearCorners();
            if (!NavMesh.CalculatePath(from, to, WalkableMask, path))
                return 1;

            var distance = GetPathLength(path);
            if (Mode == ScoreMode.PeferExact) {
                var a = Mathf.Clamp01(Mathf.Abs(distance - MaxDistance) / MaxDistance);
                return 1f - a;
            } else {
                var a = Mathf.Clamp01(distance / MaxDistance);
                if (Mode == ScoreMode.PreferLower) {
                    a = 1f - a;
                }
                return a;
            }
        }

        static float GetPathLength(NavMeshPath path) {
            float lng = 0.0f;

            if ((path.status != NavMeshPathStatus.PathInvalid) && (path.corners.Length > 1)) {
                for (int i = 1; i < path.corners.Length; ++i) {
                    lng += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                }
            }

            return lng;
        }
    }
}