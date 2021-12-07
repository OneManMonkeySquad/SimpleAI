using System;
using UnityEngine;

namespace SimpleAI.EQS {
    [Serializable]
    public class DistanceTest : ITest {
        public enum ScoreMode {
            PreferGreater,
            PreferLower,
            PeferExact
        }

        public QueryContext To;
        [Range(1, 1000)]
        public float MaxDistance = 100;
        public ScoreMode Mode = ScoreMode.PreferGreater;

        public float RuntimeCost => 1;

        public float Run(ref Item item, ResolvedQueryRunContext ctx) {
            var from = item.Point;
            var tos = ctx.Resolve(To);

            var c = 1f;
            foreach (var to in tos) {
                var distance = (to - from).magnitude;

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
    }
}