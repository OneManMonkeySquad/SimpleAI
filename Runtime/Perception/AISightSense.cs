using System;
using System.Collections.Generic;
using DataStructures.ViliWonka.KDTree;
using UnityEngine;
using UnityEngine.Assertions;

namespace SimpleAI.Perception {
    [Serializable]
    public class AISightSense : ISense {
        [Serializable]
        public struct Cone {
            public float MaxDistance;
            [Range(0, 360)]
            public float Angle;
        }

        public Cone[] Cones;
        public LayerMask LineOfSightBlockingMask;
        [Tooltip("At this distance anything is detected")]
        public float AnyAngleDetectionDistance = 2;

        static List<GameObject> s_sources = new List<GameObject>();
        static int s_sourceBuildAtFrame;

        static KDTree s_tree = new KDTree();
        static List<Vector3> s_points = new List<Vector3>();
        static List<int> s_results = new List<int>();
        static KDQuery s_query = new KDQuery();
        static HashSet<GameObject> s_targetsReported = new HashSet<GameObject>();

        public static void AddSource(GameObject source) => s_sources.Add(source);
        public static void RemoveSource(GameObject source) => s_sources.Remove(source);

        public void Add(AIPerception perception) { }
        public void Remove(AIPerception perception) { }

        public void Update(AIPerception perception, IEnumerable<ISenseListener> listeners) {
            Assert.IsNotNull(perception);

            if (Cones.Length == 0)
                return;

            if (s_sourceBuildAtFrame != Time.frameCount) {
                s_sourceBuildAtFrame = Time.frameCount;

                s_points.Clear();
                for (int i = 0; i < s_sources.Count; ++i) {
                    s_points.Add(s_sources[i].transform.position);
                }
                s_tree.Build(s_points);
            }

            float maxDistance = AnyAngleDetectionDistance;
            for (int coneIdx = 0; coneIdx < Cones.Length; ++coneIdx) {
                var cone = Cones[coneIdx];
                maxDistance = Mathf.Max(maxDistance, cone.MaxDistance);
            }

            var view = perception.View;

            s_results.Clear();
            s_query.Radius(s_tree, view.position, maxDistance, s_results);

            s_targetsReported.Clear();
            for (int coneIdx = 0; coneIdx < Cones.Length; ++coneIdx) {
                var cone = Cones[coneIdx];
                var sqrConeMaxDistance = cone.MaxDistance * cone.MaxDistance;

                for (int i = 0; i < s_results.Count; ++i) {
                    var sourceIdx = s_results[i];
                    var target = s_sources[sourceIdx];

                    var diffToTarget = target.transform.position - view.position;

                    var sqrDistanceToTarget = diffToTarget.sqrMagnitude;
                    if (sqrDistanceToTarget > sqrConeMaxDistance)
                        continue;

                    if (sqrDistanceToTarget > AnyAngleDetectionDistance) {
                        var angle = Vector3.Angle(view.forward, diffToTarget.normalized);
                        if (angle > cone.Angle / 2)
                            continue;

                        if (Physics.Linecast(view.position, target.transform.position + Vector3.up, LineOfSightBlockingMask))
                            continue;
                    }

                    if (!CheckTarget(target, perception, sqrDistanceToTarget, maxDistance))
                        continue;

                    // Make sure we report everything only once
                    if (s_targetsReported.Contains(target))
                        continue;

                    s_targetsReported.Add(target);

                    // Report
                    foreach (var listener in listeners) {
                        listener.OnSensed(target);
                    }
                }
            }
        }

        protected virtual bool CheckTarget(GameObject target, AIPerception perception, float sqrDistanceToTarget, float maxDistance) => true;

#if UNITY_EDITOR
        public void DebugDraw(AIPerception perception) {
            Assert.IsNotNull(perception);

            var view = perception.View;

            for (int coneIdx = 0; coneIdx < Cones.Length; ++coneIdx) {
                var cone = Cones[coneIdx];

                var p1 = view.position + Quaternion.AngleAxis(cone.Angle * -0.5f, Vector3.up) * view.forward * cone.MaxDistance;
                var p2 = view.position + Quaternion.AngleAxis(cone.Angle * 0.5f, Vector3.up) * view.forward * cone.MaxDistance;
                Debug.DrawLine(view.position, p1, Color.white);
                Debug.DrawLine(view.position, p2, Color.white);

                var lastP = p1;
                for (var f = -0.5f; f <= 0.5f; f += 0.1f) {
                    var p = view.position + Quaternion.AngleAxis(cone.Angle * f, Vector3.up) * view.forward * cone.MaxDistance;
                    Debug.DrawLine(lastP, p, Color.white);
                    lastP = p;
                }
            }

            var drawer = new SenseDebugDrawer(view);
            Update(perception, new[] { drawer });
        }
#endif
    }
}