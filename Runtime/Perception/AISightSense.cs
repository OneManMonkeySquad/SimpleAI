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

        static List<GameObject> Sources = new List<GameObject>();
        static int sourceBuildAtFrame;

        static KDTree tree = new KDTree();
        static List<Vector3> points = new List<Vector3>();
        static List<int> results = new List<int>();
        static KDQuery query = new KDQuery();
        static HashSet<GameObject> targetsReported = new HashSet<GameObject>();

        public static void AddSource(GameObject source) => Sources.Add(source);
        public static void RemoveSource(GameObject source) => Sources.Remove(source);

        public void Add(AIPerception perception) { }
        public void Remove(AIPerception perception) { }

        public void Update(Transform view, IEnumerable<ISenseListener> listeners) {
            Assert.IsNotNull(view);

            if (Cones.Length == 0)
                return;

            if (sourceBuildAtFrame != Time.frameCount) {
                sourceBuildAtFrame = Time.frameCount;

                points.Clear();
                for (int i = 0; i < Sources.Count; ++i) {
                    points.Add(Sources[i].transform.position);
                }
                tree.Build(points);
            }

            float maxDistance = 0;
            for (int coneIdx = 0; coneIdx < Cones.Length; ++coneIdx) {
                var cone = Cones[coneIdx];
                maxDistance = Mathf.Max(maxDistance, cone.MaxDistance);
            }

            results.Clear();
            query.Radius(tree, view.position, maxDistance, results);

            targetsReported.Clear();
            for (int coneIdx = 0; coneIdx < Cones.Length; ++coneIdx) {
                var cone = Cones[coneIdx];
                var sqrConeMaxDistance = cone.MaxDistance * cone.MaxDistance;

                for (int i = 0; i < results.Count; ++i) {
                    var sourceIdx = results[i];
                    var target = Sources[sourceIdx];

                    var diffToTarget = target.transform.position - view.position;

                    var sqrDistanceToTarget = diffToTarget.sqrMagnitude;
                    if (sqrDistanceToTarget > sqrConeMaxDistance)
                        continue;

                    if (sqrDistanceToTarget > 1.5f) { // Detect everything at close distance
                        var angle = Vector3.Angle(view.forward, diffToTarget.normalized);
                        if (angle > cone.Angle / 2)
                            continue;

                        if (Physics.Linecast(view.position, target.transform.position + Vector3.up, LineOfSightBlockingMask))
                            continue;
                    }

                    // Make sure we report everything only once
                    if (targetsReported.Contains(target))
                        continue;

                    targetsReported.Add(target);

                    // Respect CharacterStats
                    /* #todo
                    var targetHumanoid = target.GetComponent<Humanoid>();
                    if (targetHumanoid != null) {
                        if (targetHumanoid == character)
                            continue;

#if UNITY_EDITOR
                        if (targetHumanoid.isServer != character.isServer)
                            continue;
#endif

                        var effectiveSightDistance = maxDistance;
                        targetHumanoid.Stats.ModifyStat(CharacterStat.PerceptionSighted, ref effectiveSightDistance);
                        if (distanceToTarget > effectiveSightDistance)
                            continue;

                        // #todo giant fucking hack.. should maybe be a Debuff
                        if (targetHumanoid.LastStealingTime >= Time.time - 1) {
                            foreach (var listener in listeners) {
                                listener.OnStealingSensed(targetHumanoid);
                            }
                        }
                    }
                    */

                    // Report
                    foreach (var listener in listeners) {
                        listener.OnSensed(target);
                    }
                }
            }
        }

#if UNITY_EDITOR
        public void DebugDraw(Transform view) {
            return; //#todo
            //if (!Globals.Instance.DebugPerception)
            //    return;

            Assert.IsNotNull(view);

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
            Update(view, new[] { drawer });
        }
#endif
    }
}