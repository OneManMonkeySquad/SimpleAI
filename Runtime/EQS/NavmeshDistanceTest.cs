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

        public bool AllowPartialPaths = false;

        NavMeshPath path;

        public float Run(ref Item item, QueryRunContext ctx) {
            if (path == null) {
                // Not allowed to be called in ctor
                path = new NavMeshPath();
            }

            var from = item.Point;
            var tos = ctx.Resolve(To);

            var c = 1f;
            foreach (var to in tos) {
                path.ClearCorners();
                if (!NavMesh.CalculatePath(from, to, WalkableMask, path))
                    return 0;

                if (!AllowPartialPaths && path.status == NavMeshPathStatus.PathPartial)
                    return 0;

                var distance = GetPathLength(path);

                if (Mode == ScoreMode.PeferExact) {
                    var a = Mathf.Clamp01(Mathf.Abs(distance - MaxDistance) / MaxDistance);
                    c *= a;
                } else {
                    var a = Mathf.Clamp01(distance / MaxDistance);
                    if (Mode == ScoreMode.PreferGreater) {
                        a = 1f - a;
                    }
                    c *= a;
                }
            }
            return 1f - c;
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